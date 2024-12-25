using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WandererManager : MonoBehaviour
{
  [Header("Character Stats")]
  public int level = 1;
  public int currentXP = 0;
  public int maxHP = 100;
  public int maxXP = 100;

  public int currentHP;
  public int abilityPoints = 0;
  private int xpToNextLevel;
  private bool isInvincible = false;

  [Header("Maximum Level Cap")]
  public int maxLevel = 4;

  [Header("Potion Stats")]
  public int maxPotions = 3;
  public int currentPotions = 0;
  public float healPercentage = 0.5f;
  public ParticleSystem drinkingEffect;

  [Header("Sound Effects")]
  public AudioClip potionPickupSound;
  public AudioClip potionDrinkingSound;
  public AudioClip damageSound;
  public AudioClip deathSound;
  private AudioSource audioSource;


  private AbilityManager abilityManager;
  private Animator animator;

  [Header("Cheat Toggles")]
  private bool isSlowMotion = false;
  public bool toggleCooldown = false;



  void Start()
  {

    abilityManager = GetComponent<AbilityManager>();
    if (abilityManager == null)
    {
      Debug.LogError("AbilityManager not assigned in the Inspector!");
    }

    currentHP = maxHP;
    initializeStatus();


    UpdateXPToNextLevel();
    UpdateStatsForLevel();


    animator = GetComponent<Animator>();
    if (animator == null)
    {
      Debug.LogError("Animator not found! Please add an Animator component to the Wanderer.");
    }

    audioSource = GetComponent<AudioSource>();
    if (audioSource == null)
    {
      Debug.LogError("AudioSource not found! Please attach an AudioSource component.");
    }
  }

  void Update()
  {
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

    if (Input.GetKeyDown(KeyCode.M)) // Toggle Slow Motion
    {
      isSlowMotion = !isSlowMotion;
      Time.timeScale = isSlowMotion ? 0.5f : 1f;
      Debug.Log("Slow Motion " + (isSlowMotion ? "Enabled" : "Disabled"));
    }

    if (Input.GetKeyDown(KeyCode.U)) // Unlock all abilities
    {
      if (abilityManager != null)
      {
        abilityManager.UnlockAllAbilities();
        Debug.Log("All abilities have been unlocked!");
      }
      else
      {
        Debug.LogError("AbilityManager not assigned to unlock abilities!");
      }
    }

    if (Input.GetKeyDown(KeyCode.A)) // Gain 1 Ability Point
    {
      abilityPoints++;
      Debug.Log("Gained 1 ability point! Current Ability Points: " + abilityPoints);
    }

    if (Input.GetKeyDown(KeyCode.X)) // Gain 100 XP
    {
      GainXP(100);
      Debug.Log("Gained 100 XP!");
    }

    if (Input.GetKeyDown(KeyCode.F)) // Use healing potion
    {
      UseHealingPotion();
      Debug.Log("Using healing potion!");
    }

    if (Input.GetKeyDown(KeyCode.C))
    {
      toggleCooldown = !toggleCooldown; // Toggle the cheat on/off
      Debug.Log("Cooldowns " + (toggleCooldown ? "Disabled" : "Enabled"));
    }
  }
  private void UpdateStatsForLevel()
  {
    maxXP = 100 * level;
    maxHP = 100 * level;
    currentHP = maxHP;
    Debug.Log($"Stats updated for level {level}. Max XP: {maxXP}, Max Health: {maxHP}");

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

        abilityManager.UnlockAllAbilities();
        Debug.Log("Wanderer stats initialized for Level 2 due to restart/cheat load.");
      }
      else if (LevelManager.Instance != null && LevelManager.Instance.currentHP > 0)
      {
        Debug.Log("Loading Wanderer state from LevelManager...");
        LevelManager.Instance.LoadWandererState(out level, out currentHP, out maxHP, out currentPotions, out abilityManager.unlockedAbilitiess);

        // Debug.Log($"Loaded stats: HP={currentHP}, MaxHP={maxHP}, Potions={currentPotions}, Abilities={string.Join(", ", unlockedAbilities)}");
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

  public void GainXP(int amount)
  {
    if (level >= maxLevel) return;

    currentXP += amount;

    while (currentXP >= maxXP && level < maxLevel)
    {
      LevelUp();
    }
  }

  void LevelUp()
  {
    if (level >= maxLevel) return;

    // Overflow XP
    int overflowXP = currentXP - maxXP;

    level++;
    abilityPoints++;
    maxHP += 100;
    currentHP = maxHP;
    maxXP = 100 * level; // Update XP needed for next level
    currentXP = overflowXP; // Set current XP to overflow
    UpdateXPToNextLevel();

    Debug.Log("Level Up! You have gained an ability point!");

    // if (abilityPoints > 0)
    // {
    //     abilitySelectionManager.ShowAbilitySelection();
    // }
  }

  void UpdateXPToNextLevel()
  {
    if (level < maxLevel)
    {
      xpToNextLevel = 100 * level;
    }
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

  public void Heal(int healAmount)
  {
    currentHP += healAmount;

    if (currentHP > maxHP)
    {
      currentHP = maxHP;
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("HealingPotion"))
    {
      if (currentPotions < maxPotions)
      {
        currentPotions++;

        if (audioSource != null && potionPickupSound != null)
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

  public void TakeDamage(int damage)
  {
    if (isInvincible) return;

    currentHP -= damage;

    if (animator != null)
    {
      animator.SetBool("IsHit", true);
    }

    if (audioSource != null && damageSound != null)
    {
      audioSource.PlayOneShot(damageSound);
    }

    Debug.Log("The Wanderer is taking damage by " + damage);

    StartCoroutine(ResetIsHit());

    if (currentHP <= 0)
    {
      currentHP = 0;
      Debug.Log("The Wanderer has died!");

      if (animator != null)
      {
        animator.SetBool("IsHit", false);
        animator.SetTrigger("Die");
        if (audioSource != null && deathSound != null)
        {
          audioSource.PlayOneShot(deathSound);
        }
        StartCoroutine(WaitForDieAnimation());
      }

    }
  }

  IEnumerator ResetIsHit()
  {
    yield return new WaitForSeconds(1.5f);
    if (animator != null)
    {
      animator.SetBool("IsHit", false);
    }
  }

  public void SetInvincibility(bool invincible)
  {
    isInvincible = invincible;

    if (isInvincible)
    {
      Debug.Log("Wanderer is now invincible!");
    }
    else
    {
      Debug.Log("Wanderer is no longer invincible!");
    }
  }
  private IEnumerator WaitForDieAnimation()
  {
    yield return new WaitForSeconds(5.0f); // Replace with the actual duration of your Die animation
    SceneManager.LoadScene("Game_Over");
  }


}
