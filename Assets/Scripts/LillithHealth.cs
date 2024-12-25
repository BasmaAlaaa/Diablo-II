using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilithHealth : MonoBehaviour
{
  public int maxHealth = 50;
  public int maxShield = 50;
  public int currentHealth = 50;
  private int currentShield;
  public bool isPhaseOne = false;
  public bool isPhaseTwo = false;
  public int reflectiveDamage = 15;
  private LilithAttack lillithAttack;
  public GameObject shieldEffect;
  public delegate void PhaseTwoHandler();
  public event PhaseTwoHandler OnPhaseTwo;

  public delegate void DeathHandler();
  public event DeathHandler OnDeath;

  public delegate void DamageHandler();
  public event DamageHandler OnDamage;

  public delegate void PhaseOneHandler();
  public event PhaseOneHandler OnPhaseOne;

  [Header("Sound Effects")]
  public AudioClip takeDamageSound;
  public AudioClip deathSound;
  private AudioSource audioSource;

  [SerializeField] private BossHealthBar healthBar; // Assign this in the Inspector

  void Start()
  {
    currentHealth = maxHealth;
    currentShield = 0; // No shield in Phase 1
    lillithAttack = GetComponent<LilithAttack>();
    UpdateHealthBar(); // Initialize the health bar at the start


    // Timer 3 seconds for testing 
    //Invoke("StartPhaseTwo", 0.5f);
    audioSource = GetComponent<AudioSource>();
    if (audioSource == null)
    {
      Debug.LogError("AudioSource not found! Please attach an AudioSource component.");
    }
  }
  private void UpdateHealthBar()
  {
    if (healthBar != null)
    {
      healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }
  }

  /*
      public void TestAnimation()
      {
          Debug.Log("Test Animation");
          TakeDamage(10);
      }*/

  public void OnTriggerEnter(Collider other)
  {
    // Debug.Log($"Collider entered: {other.gameObject.name} with tag: {other.tag}");

    if (other.gameObject.layer == LayerMask.NameToLayer("Fireball"))
    {
      Debug.Log("Fireball hit detected via CompareTag!");
      TakeDamage(10);
      return;
    }
  }

  public void TakeDamage(int damage)
  {
    if (!isPhaseOne && !isPhaseTwo)
    {
      isPhaseOne = true;
      OnPhaseOne?.Invoke();
      Debug.Log("Phase One started!");
    }

    GameObject[] minions = GameObject.FindGameObjectsWithTag("Minion");
    if (minions.Length > 0)
    {
      Debug.Log("Lilith cannot take damage while Minions are alive!");
      return;
    }
    if (lillithAttack.isReflectiveAuraActive)
    {
      Debug.Log("Break Aura!");

      lillithAttack.DeactivateReflectiveAura();
      lillithAttack.ReflectDamage(damage);
      UpdateHealthBar();

      // CALL FUNCTION TO DAMAGE PLAYER
      return;
    }

    if (isPhaseTwo && currentShield > 0)
    {
      Debug.Log("Shield takes damage!");
      currentShield -= damage;
      UpdateHealthBar();

      if (currentShield <= 0)
      {
        currentHealth += currentShield;
        currentShield = 0;
        OnDamage?.Invoke();
        Debug.Log("Shield broken!");
        shieldEffect.SetActive(false);
        StartCoroutine(RegenerateShield());
        UpdateHealthBar();

      }
    }

    else
    {
      Debug.Log("Lilith takes damage!");
      currentHealth -= damage;
      UpdateHealthBar();

      OnDamage?.Invoke();
      audioSource.PlayOneShot(takeDamageSound);

    }

    if (currentHealth <= 0)
    {
      UpdateHealthBar();

      if (!isPhaseTwo)
      {
        OnDeath?.Invoke();
        StartPhaseTwo();
      }
      else
        Die();
    }
  }
  IEnumerator RegenerateShield()
  {
    yield return new WaitForSeconds(10);
    currentShield = maxShield;
    shieldEffect.SetActive(true);

    Debug.Log("Shield fully regenerated!");
  }


  void StartPhaseTwo()
  {
    isPhaseOne = false;
    // stop coroutine of phaseOne

    isPhaseTwo = true;
    currentHealth = maxHealth;
    currentShield = maxShield;
    // wait for 6 seconds then show shield
    StartCoroutine(ActivateShieldDelay());
    OnPhaseTwo?.Invoke();

    Debug.Log("Phase Two started!");
  }

  private IEnumerator ActivateShieldDelay()
  {
    yield return new WaitForSeconds(6);
    shieldEffect.SetActive(true);
    Debug.Log("Shield activated!");
  }

  void Die()
  {
    OnDeath?.Invoke();
    Debug.Log("Lilith defeated!");
    audioSource.PlayOneShot(deathSound);
    lillithAttack.PlayDieAnimation();
  }
}

