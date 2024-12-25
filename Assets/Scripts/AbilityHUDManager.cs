using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityHUDManager : MonoBehaviour
{
  [System.Serializable]
  public class AbilityButton
  {
    public Button button;           // UI Button
    public Image icon;             // Ability icon
    // public TMP_Text cooldownText;  // Text showing status (e.g., "Locked")
    public string abilityName;     // Name of the ability
  }

  [Header("HUD Elements")]
  public AbilityButton[] abilities;  // Buttons for each ability

  private AbilityManager abilityManager;  // For Sorcerer
  private WandererStats wandererStats;    // For Barbarian

  void Start()
  {
    // Determine character type and reference the correct ability handler
    CharacterManager characterManager = FindObjectOfType<CharacterManager>();
    if (characterManager != null)
    {
      GameObject activePlayer = characterManager.GetActivePlayer();
      if (activePlayer != null)
      {
        abilityManager = activePlayer.GetComponent<AbilityManager>();
        wandererStats = activePlayer.GetComponent<WandererStats>();
      }
    }

    if (abilityManager == null && wandererStats == null)
    {
      Debug.LogError("Neither AbilityManager nor WandererStats found on the active player!");
      return;
    }

    // Initialize buttons and attach click listeners
    foreach (var ability in abilities)
    {
      UpdateAbilityButton(ability);
      ability.button.onClick.AddListener(() => OnAbilityButtonClicked(ability.abilityName));
    }
  }

  void Update()
  {
    CharacterManager characterManager = FindObjectOfType<CharacterManager>();
    if (characterManager != null)
    {
      GameObject activePlayer = characterManager.GetActivePlayer();
      if (activePlayer != null)
      {
        abilityManager = activePlayer.GetComponent<AbilityManager>();
        wandererStats = activePlayer.GetComponent<WandererStats>();
      }
    }

    if (abilityManager == null && wandererStats == null)
    {
      Debug.LogError("Neither AbilityManager nor WandererStats found on the active player!");
      return;
    }
    // Update buttons every frame
    foreach (var ability in abilities)
    {
      UpdateAbilityButton(ability);
    }
  }

  private void UpdateAbilityButton(AbilityButton ability)
  {
    bool isUnlocked = false;

    // Check ability status
    if (abilityManager != null)
    {
      isUnlocked = abilityManager.IsAbilityUnlocked(ability.abilityName);
    }
    if (wandererStats != null)
    {
      isUnlocked = wandererStats.unlockedAbilities.Contains(ability.abilityName);
    }

    // Update button visuals
    if (!isUnlocked)
    {
      // ability.cooldownText.text = "Locked";
      ability.icon.color = Color.gray;
      ability.button.interactable = true; // Allow unlocking
    }
    else
    {
      // ability.cooldownText.text = "Unlocked";
      ability.icon.color = Color.white;
      ability.button.interactable = false; // Disable unlocking
    }
  }

  private void OnAbilityButtonClicked(string abilityName)
  {
    if (abilityManager != null)
    {
      UnlockAbilityUsingManager(abilityName);
    }
    else if (wandererStats != null)
    {
      UnlockAbilityUsingStats(abilityName);
    }
  }

  private void UnlockAbilityUsingManager(string abilityName)
  {
    Debug.Log($"Trying to unlock {abilityName} via AbilityManager.");
    abilityManager.UnlockAbility(abilityName);
  }

  private void UnlockAbilityUsingStats(string abilityName)
  {
    Debug.Log($"Trying to unlock {abilityName} via WandererStats.");
    wandererStats.UnlockAbility(abilityName);
  }
}
