using UnityEngine;
using UnityEngine.SceneManagement;

public class RuneCollectionManager : MonoBehaviour
{
  [Header("Rune Collection Settings")]
  private int totalRunesRequired = 1;
  public int runesCollected = 0;

  [Header("Gate Settings")]
  public Transform gatePosition;
  private float playerMoveSpeed = 8f;
  public string levelToLoad = "Scene_L2";

  private GameObject player;
  private bool moveToGate = false;

  private WandererStats wandererstats;
  private WandererManager wanderermanager;

  private AbilityManager abilitymanager;

  private void Start()
  {
    player = GameObject.FindWithTag("Player");
    wandererstats = player.GetComponent<WandererStats>();
    wanderermanager = player.GetComponent<WandererManager>();
    abilitymanager = player.GetComponent<AbilityManager>();

    if (player == null)
    {
      Debug.LogError("Player not found! Ensure the Player GameObject has the 'Player' tag.");
    }
  }

  private void Update()
  {
    if (moveToGate && player != null)
    {
      Vector3 direction = (gatePosition.position - player.transform.position).normalized;
      Quaternion lookRotation = Quaternion.LookRotation(direction);
      player.transform.rotation = Quaternion.Slerp(
          player.transform.rotation,
          lookRotation,
          Time.deltaTime * 5f
      );

      player.transform.position = Vector3.MoveTowards(
          player.transform.position,
          gatePosition.position,
          playerMoveSpeed * Time.deltaTime
      );

      if (Vector3.Distance(player.transform.position, gatePosition.position) < 10f)
      {
        moveToGate = false;
        TransitionToNextLevel();
      }
    }
  }

  public void AddRune()
  {
    runesCollected++;
    Debug.Log($"Rune collected: {runesCollected}/{totalRunesRequired}");

    if (runesCollected >= totalRunesRequired)
    {
      CompleteObjective();
    }
  }

  private void CompleteObjective()
  {
    Debug.Log("All Runes collected! Gate to Boss level unlocked.");
    if (player != null && gatePosition != null)
    {
      moveToGate = true;
    }
    else
    {
      Debug.LogWarning("Gate or Player reference is missing!");
    }
  }

  private void TransitionToNextLevel()
  {
    Debug.Log("Transitioning to the next level...");

    if (LevelManager.Instance != null)
    {
      if (wandererstats != null)
      {
        LevelManager.Instance.SaveWandererState(
            wandererstats.level,
            wandererstats.currentHP,
            wandererstats.maxHP,
            wandererstats.currentPotions,
            wandererstats.unlockedAbilities
        );
      }
      if (wanderermanager != null && abilitymanager != null)
      {
        LevelManager.Instance.SaveWandererState(
            wanderermanager.level,
            wanderermanager.currentHP,
            wanderermanager.maxHP,
            wanderermanager.currentPotions,
            abilitymanager.unlockedAbilitiess
        );
      }
    }

    SceneManager.LoadScene(levelToLoad);
  }

}
