using System.Collections;
using UnityEngine.SceneManagement;

using System.Collections.Generic;
using UnityEngine;

public class WandererStats : MonoBehaviour
{
  public int level; // Starting level
  public int maxLevel = 4; // Maximum level
  public int currentXP = 0; // Current XP
  public int maxXP = 100; // XP needed for the next level (100 * level)
  public int maxHP;
  public int abilityPoints; // Ability points available
  public int currentHP; // Current health
  public int maxPotions = 3;
  public int currentPotions;
  public float healPercentage = 0.5f;

  public bool isInvincible = false; // Check if the Wanderer is invincible

  public bool toggleCooldown = false; // Toggle cooldowns for abilities

  public bool isDead = false; // Check if the Wanderer is dead
  private Animator animator; // Reference to Animator

  public ParticleSystem drinkingEffect;

  public List<string> unlockedAbilities = new List<string>(); // List of unlocked abilities

  public delegate void OnStatsUpdated();
  public event OnStatsUpdated StatsUpdatedEvent; // Event to update UI or notify changes

  public delegate void OnDeath();
  public event OnDeath DeathEvent; // Event triggered on death

  [Header("Sound Effects")]
  public AudioClip potionPickupSound;
  public AudioClip potionDrinkingSound;
  public AudioClip damageSound;
  public AudioClip deathSound;
  private AudioSource audioSource;


  void Start()
  {
    initializeStatus();
    unlockedAbilities.Add("Basic");

    Debug.Log("Initializing Wanderer Stats...");
    Debug.Log($"Starting Level: {level}, Ability Points: {abilityPoints}, Max XP: {maxXP}");
    UpdateStatsForLevel();
    animator = GetComponent<Animator>(); // Cache the Animator component
    StatsUpdatedEvent?.Invoke(); // Notify listeners
    audioSource = GetComponent<AudioSource>();
    if (audioSource == null)
    {
      Debug.LogError("AudioSource not found! Please attach an AudioSource component.");
    }
  }
  private void initializeStatus()
  {
    if (SceneManager.GetActiveScene().name == "Scene_L2")
    {
      Debug.Log("Initializing Wanderer stats for Level 2.");

      if (RestartManager.Instance != null && (RestartManager.Instance.isRestarted || RestartManager.Instance.isCheatLoaded))
      {
        level = 4;
        maxHP = 400;
        currentHP = 400;
        abilityPoints = 0;
        currentPotions = 0;

        UnlockAllAbilities();
        Debug.Log("Wanderer stats initialized for Level 2 due to restart/cheat load.");
      }
      else if (LevelManager.Instance != null && LevelManager.Instance.currentHP > 0)
      {
        Debug.Log("Loading Wanderer state from LevelManager...");
        LevelManager.Instance.LoadWandererState(out level, out currentHP, out maxHP, out currentPotions, out unlockedAbilities);

        Debug.Log($"Loaded stats: HP={currentHP}, MaxHP={maxHP}, Potions={currentPotions}, Abilities={string.Join(", ", unlockedAbilities)}");
      }
      else
      {
        InitializeDefaultStats();
      }
    }
    else
    {
      Debug.Log("Initializing Wanderer stats for other levels.");
      InitializeDefaultStats();
    }
  }

  private void InitializeDefaultStats()
  {
    level = 1;
    maxHP = 100;
    currentHP = 100;
    abilityPoints = 0;
    currentPotions = 0;
    Debug.Log($"Default stats initialized: HP={currentHP}/{maxHP}, Potions={currentPotions}, Level={level}");
  }


  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.U)) // Unlock all abilities instantly
    {
      UnlockAllAbilities();
      Debug.Log("All abilities have been unlocked!");
    }
    if (Input.GetKeyDown(KeyCode.X)) // Gain 100 XP
    {
      GainXP(100);
      Debug.Log("Gained 100 XP!");
    }

    if (Input.GetKeyDown(KeyCode.H)) // Heal 20 HP
    {
      Heal(20);
      Debug.Log("Healed 20 HP!");
    }

    if (Input.GetKeyDown(KeyCode.D)) // Decrease 20 HP
    {
      TakeDamage(20);
      Debug.Log("Took 20 damage!");
    }

    if (Input.GetKeyDown(KeyCode.I)) // Toggle Invincibility
    {
      isInvincible = !isInvincible;
      Debug.Log("Invincibility " + (isInvincible ? "Enabled" : "Disabled"));
    }
    if (Input.GetKeyDown(KeyCode.A)) // Gain 1 Ability Point
    {
      abilityPoints++;
      Debug.Log("Gained 1 ability point! Current Ability Points: " + abilityPoints);
    }

    if (Input.GetKeyDown(KeyCode.F)) // Use healing potion
    {
      UseHealingPotion();
      Debug.Log("Using healing potion!");
    }

    if (Input.GetKeyDown(KeyCode.C)) // Unlock all abilities
    {
      toggleCooldown = !toggleCooldown; // Toggle the cheat on/off
      Debug.Log("Cooldowns " + (toggleCooldown ? "Disabled" : "Enabled"));
    }
  }
  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("HealingPotion"))
    {
      if (currentPotions < maxPotions)
      {
        currentPotions++;

        {
          audioSource.PlayOneShot(potionPickupSound);
        }

        Debug.Log($"Picked up a healing potion! Current potions: {currentPotions}");
        Destroy(other.gameObject);
      }
      else
      {
        Debug.Log("Potion capacity is full. Can't pick up more.");
      }
    }
  }

  // Call this when XP is gained
  public void GainXP(int amount)
  {
    if (level >= maxLevel)
    {
      Debug.Log("Max level reached. XP will not increase.");
      return;
    }

    currentXP += amount;
    Debug.Log($"Gained {amount} XP. Current XP: {currentXP}/{maxXP}");

    while (currentXP >= maxXP)
    {
      LevelUp();
    }

    StatsUpdatedEvent?.Invoke(); // Notify listeners of stats update
  }

  private void LevelUp()
  {
    if (level >= maxLevel) return;

    // Overflow XP
    int overflowXP = currentXP - maxXP;

    // Increase level
    level++;
    abilityPoints++; // Gain an ability point
    maxHP += 100; // Increase max health
    currentHP = maxHP; // Refill health
    maxXP = 100 * level; // Update XP needed for next level
    currentXP = overflowXP; // Set current XP to overflow

    Debug.Log($"Level Up! New Level: {level}, Max Health: {maxHP}, Ability Points: {abilityPoints}");

    StatsUpdatedEvent?.Invoke(); // Notify listeners of stats update
  }

  public void UnlockAbility(string abilityName)
  {
    Debug.Log($"Attempting to unlock ability: {abilityName}");
    Debug.Log($"Ability Points Available: {abilityPoints}");

    if (abilityPoints > 0 && !unlockedAbilities.Contains(abilityName))
    {
      unlockedAbilities.Add(abilityName);
      abilityPoints--;
      Debug.Log($"Successfully unlocked ability: {abilityName}. Remaining Points: {abilityPoints}");
    }
    else if (unlockedAbilities.Contains(abilityName))
    {
      Debug.Log($"Ability {abilityName} is already unlocked.");
    }
    else
    {
      Debug.Log("Not enough ability points to unlock ability.");
    }

    StatsUpdatedEvent?.Invoke(); // Notify listeners of stats update
  }

  public void TakeDamage(int damage)
  {
    if (isInvincible) return; // Check if invincible
    if (isDead) return; // Don't process damage if already dead

    if (animator != null)
    {
      animator.SetTrigger("Damage"); // Trigger the death animation
      if (audioSource != null && damageSound != null)
      {
        audioSource.PlayOneShot(damageSound);
      }
    }
    else
    {
      Debug.LogWarning("Animator component not found. Unable to play damaged animation.");
    }

    currentHP -= damage;
    Debug.Log($"Wanderer took {damage} damage. Current Health: {currentHP}/{maxHP}");

    StatsUpdatedEvent?.Invoke(); // Notify listeners

    if (currentHP <= 0)
    {
      Die();
    }
  }
  public void Heal(int amount)
  {
    if (isDead) return; // Don't heal a dead Wanderer

    currentHP += amount;
    if (currentHP > maxHP)
    {
      currentHP = maxHP;
    }
    Debug.Log($"Wanderer healed {amount}. Current Health: {currentHP}/{maxHP}");

    StatsUpdatedEvent?.Invoke(); // Notify listeners
  }
  private void Die()
  {
    isDead = true;
    Debug.Log("The Wanderer has died.");

    // Play death animation
    if (animator != null)
    {
      animator.SetTrigger("Die"); // Trigger the death animation
      if (audioSource != null && deathSound != null)
      {
        audioSource.PlayOneShot(deathSound);
      }
    }
    else
    {
      Debug.LogWarning("Animator component not found. Unable to play death animation.");
    }

    DeathEvent?.Invoke(); // Notify listeners of death
    StartCoroutine(WaitForDieAnimation());
    // Additional logic, e.g., disabling controls, restarting the game, etc.
  }

  private void UpdateStatsForLevel()
  {
    maxXP = 100 * level;
    maxHP = 100 * level;
    currentHP = maxHP;
    Debug.Log($"Stats updated for level {level}. Max XP: {maxXP}, Max Health: {maxHP}");

  }

  public void UnlockAllAbilities()
  {
    unlockedAbilities.Clear();
    unlockedAbilities.Add("Basic");
    unlockedAbilities.Add("Defensive");
    unlockedAbilities.Add("WildCard");
    unlockedAbilities.Add("Ultimate");
    abilityPoints = 0;
    Debug.Log("All abilities have been unlocked.");
    StatsUpdatedEvent?.Invoke(); // Notify listeners
  }

  private IEnumerator WaitForDieAnimation()
  {
    yield return new WaitForSeconds(5.0f);
    SceneManager.LoadScene("Game_Over");
  }


  public void UseHealingPotion()
  {
    if (currentPotions > 0 && currentHP < maxHP)
    {
      currentPotions--;
      int healAmount = Mathf.RoundToInt(maxHP * healPercentage);
      Heal(healAmount);

      if (animator != null)
      {
        animator.SetBool("DoDrink", true);
        drinkingEffect.Play();
      }

      if (audioSource != null && potionDrinkingSound != null)
      {
        audioSource.PlayOneShot(potionDrinkingSound);
      }

      Debug.Log($"Used a healing potion! Healed {healAmount} HP.");

      StartCoroutine(ResetDoDrink());
    }
    else if (currentHP >= maxHP)
    {
      Debug.Log("HP is already full. Can't use a healing potion.");
    }
    else
    {
      Debug.Log("No healing potions available.");
    }
  }


  IEnumerator ResetDoDrink()
  {
    yield return new WaitForSeconds(5f);
    if (animator != null)
    {
      animator.SetBool("DoDrink", false);
      drinkingEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

    }
    Debug.Log("Healing potion animation reset.");
  }
}
