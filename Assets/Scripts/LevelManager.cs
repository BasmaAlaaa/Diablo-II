using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
  public static LevelManager Instance;

  // Persistent Wanderer stats
  public int currentHP;
  public int currentlevel;
  public int maxHP;
  public int currentPotions;
  public List<string> unlockedAbilities = new List<string>();

  void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject); // Persist across scenes
    }
    else
    {
      Destroy(gameObject); // Prevent duplicates
    }
  }

  // Save the Wanderer's current state
  public void SaveWandererState(int level, int hp, int maxHp, int potions, List<string> abilities)
  {
    currentlevel = level;
    currentHP = hp;
    maxHP = maxHp;
    currentPotions = potions;
    unlockedAbilities = new List<string>(abilities); // Copy abilities
    Debug.Log($"Wanderer state saved: HP: {currentHP}/{maxHP}, Potions: {currentPotions}, Abilities: {string.Join(", ", unlockedAbilities)}");
  }

  // Load the Wanderer's saved state
  public void LoadWandererState(out int level, out int hp, out int maxHp, out int potions, out List<string> abilities)
  {
    level = currentlevel;
    hp = currentHP;
    maxHp = maxHP;
    potions = currentPotions;
    abilities = new List<string>(unlockedAbilities); // Return a copy of abilities
    Debug.Log($"Wanderer state loaded: HP: {hp}/{maxHp}, Potions: {potions}, Abilities: {string.Join(", ", abilities)}");
  }
}
