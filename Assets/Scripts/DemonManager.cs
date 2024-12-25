using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DemonManager : MonoBehaviour
{
  public int health = 40;
  public int maxHealth = 40;
  public int xpReward = 30;
  public float attackRange = 1f;
  public float followRange = 10f;
  private float attackCounter = 0;
  public Transform[] patrolPoints;
  public bool isAggressive = false;
  public bool isAlive = true;

  [Header("UI Components")]
  [SerializeField] FloatingHealthBar healthBar;
  private Transform wanderer;
  private Animator animator;
  private NavMeshAgent agent;

  [Header("Sound Effects")]
  public AudioClip explosiveThrowSound;
  public AudioClip deathSound;
  private AudioSource audioSource;

  private int currentPatrolIndex = 0;
  private float patrolTimer = 0f;
  private WandererManager wandererManager;
  private WandererStats wandererStats;

  void Start()
  {
    CharacterManager characterManager = FindObjectOfType<CharacterManager>();
    if (characterManager != null)
    {
      GameObject activePlayer = characterManager.GetActivePlayer();
      if (activePlayer != null)
      {
        Debug.Log($"Active player found by CharacterManager: {activePlayer.name}");
        wanderer = activePlayer.transform;
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
    wandererManager = wanderer.GetComponent<WandererManager>();
    wandererStats = wanderer.GetComponent<WandererStats>();
    animator = GetComponent<Animator>();
    agent = GetComponent<NavMeshAgent>();
    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    healthBar.UpdateHealthBar(health, maxHealth);
    audioSource = GetComponent<AudioSource>();
  }

  void Update()
  {
    if (!isAlive) return;

    float distanceToWanderer = Vector3.Distance(transform.position, wanderer.position);

    if (isAggressive)
    {
      HandleAggressiveBehavior(distanceToWanderer);
    }
    else
    {
      HandleNonAggressiveBehavior(distanceToWanderer);
    }
  }

  private void Awake()
  {
    healthBar = GetComponentInChildren<FloatingHealthBar>();
  }

  void HandleAggressiveBehavior(float distanceToWanderer)
  {
    if (distanceToWanderer > attackRange && distanceToWanderer <= followRange)
    {
      FollowWanderer();
    }
    else if (distanceToWanderer <= attackRange)
    {
      AttackWanderer();
    }
    else
    {
      ResetToPatrolling();
    }
  }

  void HandleNonAggressiveBehavior(float distanceToWanderer)
  {
    if (distanceToWanderer <= followRange)
    {
      Patrol();
    }
    else
    {
      Patrol();
    }
  }

  public void BecomeAggressive()
  {
    if (isAggressive) return;

    isAggressive = true;
    Debug.Log($"{gameObject.name} is now aggressive!");
    animator.SetBool("IsWalking", true);
    agent.SetDestination(wanderer.position);
  }

  public void ResetToPatrolling()
  {
    if (!isAlive) return;
    if (agent == null || !agent.enabled) return;

    isAggressive = false;
    // Debug.Log($"{gameObject.name} is returning to patrolling.");
    animator.SetBool("IsWalking", true);
    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
  }

  void Patrol()
  {
    if (patrolPoints.Length == 0) return;

    animator.SetBool("IsWalking", true);

    if (agent.remainingDistance < 0.5f)
    {
      patrolTimer += Time.deltaTime;
      currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
      agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }
  }

  void FollowWanderer()
  {
    agent.SetDestination(wanderer.position);
    animator.SetBool("IsWalking", true);
  }

  private float lastAttackTime = 0f;
  private float attackCooldown = 5f;

  void AttackWanderer()
  {
    if (Time.time - lastAttackTime < attackCooldown) return;

    lastAttackTime = Time.time;
    agent.SetDestination(transform.position);
    animator.SetBool("IsWalking", false);

    Vector3 directionToWanderer = wanderer.position - transform.position;
    directionToWanderer.y = 0;
    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        Quaternion.LookRotation(directionToWanderer),
        Time.deltaTime * 5f
    );

    if (attackCounter < 2)
    {
      animator.SetBool("IsMeleeAttacking", true);
      if (wandererManager != null)
      {
        wandererManager.TakeDamage(10);
      }
      if (wandererStats != null)
      {
        wandererStats.TakeDamage(10);
      }
      attackCounter++;
      StartCoroutine(ResetAttackAfterDelay(1.0f));
    }
    else
    {
      Debug.Log("throwing explosive now");
      animator.SetBool("IsExplosiveThrowing", true);

      if (audioSource != null && explosiveThrowSound != null)
      {
        audioSource.PlayOneShot(explosiveThrowSound);
        Debug.Log("PLAY EXPLOSIVE SOUND");
      }

      if (wandererManager != null)
      {
        wandererManager.TakeDamage(15);
      }
      if (wandererStats != null)
      {
        wandererStats.TakeDamage(15);
      }
      attackCounter = 0;
      StartCoroutine(ResetAttackAfterDelay(1.5f));
    }
  }

  IEnumerator ResetAttackAfterDelay(float delay)
  {
    yield return new WaitForSeconds(delay);
    ResetAttack();
  }

  void ResetAttack()
  {
    animator.SetBool("IsMeleeAttacking", false);
    animator.SetBool("IsExplosiveThrowing", false);
    Debug.Log($"{gameObject.name} attack reset.");
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
        damage = health; // Instant kill
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
    isAlive = false;
    Debug.Log($"{gameObject.name} has died!");
    animator.SetTrigger("Die");
    if (audioSource != null && deathSound != null)
    {
      audioSource.PlayOneShot(deathSound);
    }
    agent.enabled = false;
    Collider collider = GetComponent<Collider>();
    if (collider != null)
    {
      collider.enabled = false;
    }

    if (wandererManager != null)
    {
      wandererManager.GainXP(xpReward);
    }
    if (wandererStats != null)
    {
      wandererStats.GainXP(xpReward);
    }

    Destroy(gameObject, 2f);
  }

  void OnDrawGizmosSelected()
  {
    if (agent != null)
    {
      Gizmos.color = agent.isOnNavMesh ? Color.green : Color.red;
      Debug.Log("Gizmos Color:" + Gizmos.color);
      Gizmos.DrawSphere(transform.position, 0.5f);
    }
  }

}
