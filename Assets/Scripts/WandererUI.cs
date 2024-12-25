using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AI;
using TMPro;

public class WandererUI : MonoBehaviour
{
  [Header("Player HUD Elements")]
  public Slider healthBar;
  public TMP_Text healthText;

  public Slider xpBar;
  public TMP_Text xpText;

  public TMP_Text levelText;
  public TMP_Text abilityPointsText;
  public TMP_Text healingPotionsText;
  public TMP_Text runeFragmentsText;
  private Transform wanderer;

  private WandererStats wandererStats;
  private WandererManager wandererManager;

  private RuneCollectionManager runeFragments;
  private bool isInitialized = false;


  void Start()
  {
    StartCoroutine(LateStart());
  }

  IEnumerator LateStart()
  {
    // Wait until CharacterManager initializes
    yield return new WaitUntil(() => FindObjectOfType<CharacterManager>() != null);

    CharacterManager characterManager = FindObjectOfType<CharacterManager>();
    if (characterManager != null)
    {
      GameObject activePlayer = characterManager.GetActivePlayer();
      if (activePlayer != null)
      {
        wanderer = activePlayer.transform;
        wandererManager = wanderer.GetComponent<WandererManager>();
        wandererStats = wanderer.GetComponent<WandererStats>();
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

    runeFragments = wanderer.GetComponent<RuneCollectionManager>();

    if (wandererStats == null && wandererManager == null)
    {
      Debug.Log("WandererStats or WandererManager not found in the scene!");
    }
    if (runeFragments == null)
    {
      Debug.Log("RuneCollectionManager not found in the scene!");
    }
    Debug.Log($"wanderer: {wanderer?.name}, wandererStats: {wandererStats}, wandererManager: {wandererManager}, runeFragments: {runeFragments}");


    isInitialized = true;
    Debug.Log("HUD Initialization Complete.");

  }


  void Update()
  {

    if (!isInitialized) return; // Skip if not initialized
    UpdateHUD();

  }
  private void UpdateHUD()
  {
    // Update stats from WandererStats or WandererManager
    if (wandererStats != null)
    {
      //  Debug.Log("Updating HUD from WandererStats");
      //  Debug.Log($"wandererStats.currentHP: {wandererStats.currentHP}, wandererStats.maxHP: {wandererStats.maxHP}, wandererStats.currentXP: {wandererStats.currentXP}, wandererStats.maxXP: {wandererStats.maxXP}, wandererStats.level: {wandererStats.level}, wandererStats.abilityPoints: {wandererStats.abilityPoints}, wandererStats.currentPotions: {wandererStats.currentPotions}, runeFragments.runesCollected: {runeFragments.runesCollected}");
      UpdatePlayerHUD(wandererStats.currentHP, wandererStats.maxHP, wandererStats.currentXP, wandererStats.maxXP, wandererStats.level, wandererStats.abilityPoints, wandererStats.currentPotions, runeFragments.runesCollected);
    }
    if (wandererManager != null)
    {
      //    Debug.Log("Updating HUD from WandererManager");
      //   Debug.Log($"wandererManager.currentHP: {wandererManager.currentHP}, wandererManager.maxHP: {wandererManager.maxHP}, wandererManager.currentXP: {wandererManager.currentXP}, wandererManager.maxXP: {wandererManager.maxXP}, wandererManager.level: {wandererManager.level}, wandererManager.abilityPoints: {wandererManager.abilityPoints}, wandererManager.currentPotions: {wandererManager.currentPotions}, runeFragments.runesCollected: {runeFragments.runesCollected}");
      UpdatePlayerHUD(wandererManager.currentHP, wandererManager.maxHP, wandererManager.currentXP, wandererManager.maxXP, wandererManager.level, wandererManager.abilityPoints, wandererManager.currentPotions, runeFragments.runesCollected);
    }

  }

  private void UpdatePlayerHUD(int currentHP, int maxHP, int currentXP, int maxXP, int level, int abilityPoints, int healingPotions, int runeFragments)
  {
    // Update Health Bar
    healthBar.value = (float)currentHP / maxHP;
    healthText.text = $"{currentHP}/{maxHP}";

    // Update XP Bar
    xpBar.value = (float)currentXP / maxXP;
    xpText.text = $"{currentXP}/{maxXP}";

    // Update Level
    levelText.text = $"Level: {level}";

    // Update Ability Points
    abilityPointsText.text = $"Ability Points: {abilityPoints}";

    // Update Healing Potions
    healingPotionsText.text = $"Potions: {healingPotions} / 3";

    // Update Rune Fragments
    runeFragmentsText.text = $"Runes: {runeFragments} / 3";
  }
}
