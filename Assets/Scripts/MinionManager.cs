using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class MinionManager : MonoBehaviour
{
  [Header("Minion Attributes")]
  public int health = 20;
  public int maxHealth = 20;
  public int xpReward = 10;
  private float attackRange = 3f;
  public float minimumDistance = 2f;
  public float followRange = 10f;
  public bool isAggressive = false;
  public bool isAlive = true;
  public bool isLevelTwo = false;
  private float lastAttackTime = 0f;
  private float attackCooldown = 5f;
  private bool isCoolingDown = false;

  [Header("UI Components")]
  [SerializeField] FloatingHealthBar healthBar;

  [Header("Sound Effects")]
  public AudioClip deathSound;
  private AudioSource audioSource;

  private Transform wanderer;
  private Animator animator;
  private NavMeshAgent agent;
  private Vector3 initialPosition;
  private WandererManager barbarianManager;
  private WandererStats wandererStats;
  private MinionCampManager campManager;

  void Start()
  {
    StartCoroutine(DelayedStart());

    animator = GetComponent<Animator>();
    agent = GetComponent<NavMeshAgent>();
    audioSource = GetComponent<AudioSource>();

    initialPosition = transform.position;

    agent.stoppingDistance = Mathf.Max(0.5f, minimumDistance);

    campManager = GetComponentInParent<MinionCampManager>();
    if (campManager == null)
    {
      Debug.LogWarning($"{gameObject.name} could not find a MinionCampManager parent.");
    }

    healthBar.UpdateHealthBar(health, maxHealth);

    SetIdle();
  }

  void Update()
  {
    if (!isAlive || wanderer == null) return;

    float distanceToWanderer = Vector3.Distance(transform.position, wanderer.position);

    if (isAggressive)
    {
      if (distanceToWanderer <= attackRange)
      {
        // Try attacking if close enough
        AttemptAttack();
      }
      else if (distanceToWanderer <= followRange && distanceToWanderer > minimumDistance)
      {
        // Follow if in follow range but not close enough to attack
        FollowWanderer();
      }
      else
      {
        // If player out of range, return to idle
        SetIdle();
      }
    }

    if (SceneManager.GetActiveScene().name == "Scene_L2")
    {
      isLevelTwo = true;
    }
  }

  private void Awake()
  {
    healthBar = GetComponentInChildren<FloatingHealthBar>();
  }

  public bool IsAggressive()
  {
    return isAggressive && isAlive;
  }

  public void SetIdle()
  {
    if (!isAlive) return;

    isAggressive = false;
    isCoolingDown = false;

    agent.ResetPath();
    animator.SetBool("IsWalking", false);
    animator.SetBool("IsAttacking", false);
    animator.SetTrigger("Idle");

    // Debug.Log($"{gameObject.name} is now idle.");
  }

  public void BecomeAggressive()
  {
    if (isAggressive || !isAlive) return;

    isAggressive = true;
    //  Debug.Log($"{gameObject.name} is now aggressive!");
    animator.SetBool("IsWalking", true);

    if (wanderer != null)
    {
      agent.SetDestination(wanderer.position);
    }
  }

  void FollowWanderer()
  {
    if (!isAggressive || wanderer == null) return;

    float distanceToWanderer = Vector3.Distance(transform.position, wanderer.position);

    if (distanceToWanderer > minimumDistance)
    {
      // Move towards the Wanderer, respecting stopping distance
      animator.SetBool("IsWalking", true);
      animator.SetBool("IsAttacking", false);
      agent.SetDestination(wanderer.position);
    }
    else
    {
      // Stop moving if within minimum distance
      agent.ResetPath();
      animator.SetBool("IsWalking", false);
    }
  }


  public void AttemptAttack()
  {
    if (isCoolingDown) return;

    float distanceToWanderer = Vector3.Distance(transform.position, wanderer.position);

    if (distanceToWanderer > minimumDistance)
    {
      FollowWanderer();
      return;
    }

    agent.ResetPath();
    animator.SetBool("IsWalking", false);

    if (campManager != null)
    {
      bool canAttack = campManager.TryStartAttack(this);
      if (canAttack)
      {
        PerformAttack();
      }
      else
      {
        WaitForTurn();
      }
    }
    else
    {
      PerformAttack();
    }
  }

  void PerformAttack()
  {
    if (Time.time - lastAttackTime >= attackCooldown)
    {
      animator.SetBool("IsAttacking", true);

      Vector3 directionToWanderer = wanderer.position - transform.position;
      directionToWanderer.y = 0;
      transform.rotation = Quaternion.Slerp(
          transform.rotation,
          Quaternion.LookRotation(directionToWanderer),
          Time.deltaTime * 5f
      );

      lastAttackTime = Time.time;
      // Debug.Log($"{gameObject.name} attacks the Wanderer!");

      if (barbarianManager != null)
      {
        barbarianManager.TakeDamage(5);
      }

      if (wandererStats != null)
      {
        wandererStats.TakeDamage(5);
      }

      isCoolingDown = true;
      StartCoroutine(Cooldown());
    }
    else
    {
      if (campManager != null)
      {
        campManager.AttackFinished();
      }
      WaitForTurn();
    }
  }

  void WaitForTurn()
  {
    animator.SetBool("IsAttacking", false);
    animator.SetBool("IsWalking", false);
  }

  IEnumerator Cooldown()
  {
    yield return new WaitForSeconds(0.5f);
    animator.SetBool("IsAttacking", false);
    animator.SetTrigger("Idle");
    // Debug.Log($"{gameObject.name} is cooling down!");
    yield return new WaitForSeconds(attackCooldown);
    isCoolingDown = false;
    // Debug.Log($"{gameObject.name} is ready to attack again!");

    if (campManager != null)
    {
      campManager.AttackFinished();
    }
  }

  public void TakeDamage(string attackType)
  {
    if (!isAlive) return;

    int damage = 0;
    switch (attackType)
    {
      case "Bash":
        damage = 5;
        break;
      case "Iron Maelstrom":
        damage = 10;
        break;
      case "Charge":
        damage = health;
        break;
      default:
        Debug.LogWarning($"{gameObject.name} received unknown attack type: {attackType}");
        break;
    }

    health -= damage;
    healthBar.UpdateHealthBar(health, maxHealth);

    Debug.Log($"{gameObject.name} took {damage} damage from {attackType}!");
    animator.SetTrigger("Hit");

    if (health <= 0)
    {
      Die();
    }
  }

  public void TakeDamage(int damage)
  {
    if (!isAlive) return;

    health -= damage;
    healthBar.UpdateHealthBar(health, maxHealth);

    Debug.Log($"{gameObject.name} took {damage} damage!");
    animator.SetTrigger("Hit");

    if (health <= 0)
    {

      Die();
    }
  }

  void Die()
  {
    if (audioSource != null && deathSound != null)
    {
      audioSource.PlayOneShot(deathSound);
      Debug.Log("MINION DEATH SOUND");
    }
    isAlive = false;
    Debug.Log($"{gameObject.name} has died!");
    animator.SetTrigger("Die");
    agent.enabled = false;
    Collider collider = GetComponent<Collider>();

    Rigidbody rb = GetComponent<Rigidbody>();
    if (rb != null)
    {
      rb.isKinematic = true;
    }

    if (collider != null)
    {
      collider.enabled = false;
    }

    if (barbarianManager != null)
    {
      barbarianManager.GainXP(xpReward);
    }

    if (wandererStats != null)
    {
      wandererStats.GainXP(xpReward);
    }

    if (campManager != null)
    {
      campManager.AttackFinished();
    }

    if (isLevelTwo)
    {
      Destroy(gameObject, 5f);

    }
  }

  void OnDrawGizmosSelected()
  {
    // Draw follow range
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, followRange);

    // Draw attack range
    Gizmos.color = Color.blue;
    Gizmos.DrawWireSphere(transform.position, attackRange);
  }
  IEnumerator DelayedStart()
  {
    yield return new WaitForEndOfFrame();

    CharacterManager characterManager = FindObjectOfType<CharacterManager>();
    if (characterManager != null)
    {
      GameObject activePlayer = characterManager.GetActivePlayer();
      if (activePlayer != null)
      {
        Debug.Log($"Active player found by CharacterManager: {activePlayer.name}");
        wanderer = activePlayer.transform;

        if (activePlayer.name == "Maria")
        {
          barbarianManager = wanderer.GetComponent<WandererManager>();
        }
        else if (activePlayer.name == "Sorcerer")
        {
          wandererStats = wanderer.GetComponent<WandererStats>();
        }
      }
      else
      {
        Debug.LogError("Active player not found by CharacterManager.");
      }
    }
    else
    {
      Debug.LogError("CharacterManager not found in the scene.");
    }
  }

}
