using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
  [Header("Abilities")]
  private List<string> allAbilities = new List<string> { "Basic", "Defensive", "WildCard", "Ultimate" };
  public Dictionary<string, bool> unlockedAbilities = new Dictionary<string, bool>();
  private string activeExclusiveAbility = null;
  public List<string> unlockedAbilitiess = new List<string>();

  private bool areCooldownsDisabled = false;
  private Dictionary<string, float> abilityCooldowns = new Dictionary<string, float>
{
    { "Basic", 1f },
    { "Defensive", 10f },
    { "WildCard", 5f },
    { "Ultimate", 10f }
};

  private Dictionary<string, float> defaultCooldowns = new Dictionary<string, float>
{
    { "Basic", 1f },
    { "Defensive", 10f },
    { "WildCard", 5f },
    { "Ultimate", 10f }
};


  [Header("References")]
  private WandererManager wandererManager;

  void Start()
  {
    wandererManager = GetComponent<WandererManager>();
    if (wandererManager == null)
    {
      Debug.LogError("WandererManager not found on the player!");
    }
    else
    {
      Debug.Log("WandererManager successfully found.");
    }

    foreach (string ability in allAbilities)
    {
      Debug.Log($"Ability initialized: {ability} ");

      unlockedAbilities[ability] = false;
    }

    unlockedAbilities["Basic"] = true;
    unlockedAbilitiess.Add("Basic");
    DebugAbilities();
  }

  void DebugAbilities()
  {
    foreach (var ability in allAbilities)
    {
      Debug.Log($"Ability: {ability}, Cooldown: {abilityCooldowns.ContainsKey(ability)}, DefaultCooldown: {defaultCooldowns.ContainsKey(ability)}");
    }
  }
  public void SetCooldownsDisabled(bool cooldownsDisabled)
  {
    areCooldownsDisabled = cooldownsDisabled;

    // Create a copy of the keys to avoid modifying the collection during iteration
    List<string> keys = new List<string>(abilityCooldowns.Keys);

    foreach (var ability in keys)
    {
      abilityCooldowns[ability] = cooldownsDisabled ? 0f : defaultCooldowns[ability];
    }

    Debug.Log(cooldownsDisabled
        ? "All ability cooldowns have been disabled (set to 0)."
        : "All ability cooldowns have been reset to their default values.");
  }

  public float GetCooldown(string abilityName)
  {
    if (abilityCooldowns.ContainsKey(abilityName))
    {
      return abilityCooldowns[abilityName];
    }

    Debug.LogError($"Cooldown for {abilityName} not found!");
    return 0f;
  }

  public bool IsAbilityActive(string abilityName)
  {
    return activeExclusiveAbility == null || activeExclusiveAbility == abilityName;
  }

  public void ActivateExclusiveAbility(string abilityName)
  {
    if (!IsAbilityUnlocked(abilityName))
    {
      Debug.Log($"{abilityName} is not unlocked.");
      return;
    }

    activeExclusiveAbility = abilityName;
    Debug.Log($"{abilityName} is now active. Other abilities are temporarily disabled.");
  }

  public void DeactivateExclusiveAbility()
  {
    Debug.Log($"Exclusive ability {activeExclusiveAbility} is now deactivated.");
    activeExclusiveAbility = null;
  }

  public bool IsExclusiveAbilityActive()
  {
    return activeExclusiveAbility != null;
  }

  public bool IsAbilityUnlocked(string abilityName)
  {
    // Check if the ability exists in the unlockedAbilitiess list
    if (unlockedAbilitiess.Contains(abilityName))
    {
      // Ensure it's also marked as unlocked in the dictionary
      if (unlockedAbilities.ContainsKey(abilityName) && !unlockedAbilities[abilityName])
      {
        unlockedAbilities[abilityName] = true;
        Debug.Log($"Synchronized ability '{abilityName}' from list to dictionary as unlocked.");
      }
      return true; // Ability is unlocked
    }

    // If not in the list, fall back to checking the dictionary
    return unlockedAbilities.ContainsKey(abilityName) && unlockedAbilities[abilityName];
  }

  public void UnlockAbility(string abilityName)
  {
    Debug.Log($"Trying to unlock {abilityName}.");
    if (!unlockedAbilities.ContainsKey(abilityName))
    {
      Debug.LogError($"Ability {abilityName} does not exist!");
      return;
    }

    if (unlockedAbilities[abilityName])
    {
      Debug.Log($"{abilityName} is already unlocked!");
      return;
    }

    Debug.Log($"Player has {wandererManager.abilityPoints} ability points.");
    if (wandererManager.abilityPoints > 0)
    {
      unlockedAbilities[abilityName] = true;
      unlockedAbilitiess.Add(abilityName);
      wandererManager.abilityPoints--;
      Debug.Log($"Unlocked ability: {abilityName}. Remaining points: {wandererManager.abilityPoints}");
    }
    else
    {
      Debug.Log("Not enough ability points to unlock an ability!");
    }
  }


  public void UnlockAllAbilities()
  {
    foreach (string ability in allAbilities)
    {
      if (!unlockedAbilities[ability])
      {
        unlockedAbilities[ability] = true;
        Debug.Log($"Unlocked ability: {ability}");
      }
    }
    unlockedAbilitiess = allAbilities;
    wandererManager.abilityPoints = 0;
    Debug.Log("All abilities unlocked!");
  }

  public List<string> GetUnlockedAbilities()
  {
    List<string> unlocked = new List<string>();
    foreach (var ability in unlockedAbilities)
    {
      if (ability.Value) unlocked.Add(ability.Key);
    }
    return unlocked;
  }

  public List<string> GetLockedAbilities()
  {
    List<string> locked = new List<string>();
    foreach (var ability in unlockedAbilities)
    {
      if (!ability.Value) locked.Add(ability.Key);
    }
    return locked;
  }
}

