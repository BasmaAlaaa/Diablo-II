using System.Collections.Generic;
using UnityEngine;

public class MinionCampManager : MonoBehaviour
{
  public List<MinionManager> minions;
  public Transform campCenter;
  public float campRange = 15f;
  public int maxAggressiveMinions = 5;
  public int currentLevel = 1;
  private Transform wanderer;
  private bool isWandererInRange = false;

  private bool isMinionAttacking = false;

  private List<MinionManager> currentAggressiveMinions = new List<MinionManager>();
  private int currentAttackerIndex = 0;

  void Start()
  {
    CharacterManager characterManager = FindObjectOfType<CharacterManager>();
    if (characterManager != null)
    {
      GameObject activePlayer = characterManager.GetActivePlayer();
      if (activePlayer != null)
      {
        // Debug.Log($"Active player found by CharacterManager: {activePlayer.name}");
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
  }

  void Update()
  {
    CharacterManager characterManager = FindObjectOfType<CharacterManager>();
    if (characterManager != null)
    {
      GameObject activePlayer = characterManager.GetActivePlayer();
      if (activePlayer != null)
      {
        // Debug.Log($"Active player found by CharacterManager: {activePlayer.name}");
        wanderer = activePlayer.transform;
      }
      else
      {
        Debug.LogError("Active player not found by CharacterManager.");
      }
    }
    CheckWandererProximity();

    if (isWandererInRange)
    {
      HandleAggressiveMinions();
    }
    else
    {
      ResetAllMinions();
    }
  }

  void CheckWandererProximity()
  {
    if (wanderer == null) return;
    float distanceToWanderer = Vector3.Distance(campCenter.position, wanderer.position);
    isWandererInRange = distanceToWanderer <= campRange;
  }

  void HandleAggressiveMinions()
  {
    List<MinionManager> eligibleMinions = new List<MinionManager>();

    foreach (var minion in minions)
    {
      if (!minion.isAlive) continue;

      float distanceToWanderer = Vector3.Distance(minion.transform.position, wanderer.position);
      if (distanceToWanderer <= minion.followRange)
      {
        eligibleMinions.Add(minion);
      }
    }

    eligibleMinions.Sort((a, b) =>
        Vector3.Distance(a.transform.position, wanderer.position)
        .CompareTo(Vector3.Distance(b.transform.position, wanderer.position))
    );

    List<MinionManager> minionsToBeAggressive = new List<MinionManager>();
    for (int i = 0; i < Mathf.Min(maxAggressiveMinions, eligibleMinions.Count); i++)
    {
      minionsToBeAggressive.Add(eligibleMinions[i]);
    }

    foreach (var minion in minionsToBeAggressive)
    {
      if (!minion.IsAggressive())
      {
        minion.BecomeAggressive();
      }
    }

    foreach (var minion in minions)
    {
      if (!minionsToBeAggressive.Contains(minion) && minion.IsAggressive())
      {
        minion.SetIdle();
      }
    }

    if (!AreSameMinionSets(currentAggressiveMinions, minionsToBeAggressive))
    {
      currentAggressiveMinions = new List<MinionManager>(minionsToBeAggressive);
      currentAttackerIndex = 0;

      if (currentAggressiveMinions.Count > 0 && !isMinionAttacking)
      {
        StartNextMinionAttack();
      }
    }
  }

  bool AreSameMinionSets(List<MinionManager> a, List<MinionManager> b)
  {
    if (a.Count != b.Count) return false;
    foreach (var min in a)
    {
      if (!b.Contains(min)) return false;
    }
    return true;
  }

  void ResetAllMinions()
  {
    foreach (var minion in minions)
    {
      if (!minion.isAlive) continue;
      minion.SetIdle();
    }
    currentAggressiveMinions.Clear();
    currentAttackerIndex = 0;
    isMinionAttacking = false;
  }

  public bool TryStartAttack(MinionManager minion)
  {
    if (!isMinionAttacking && currentAggressiveMinions.Count > 0 && currentAggressiveMinions[currentAttackerIndex] == minion)
    {
      isMinionAttacking = true;
      return true;
    }
    return false;
  }

  public void AttackFinished()
  {
    isMinionAttacking = false;
    if (currentAggressiveMinions.Count > 0)
    {
      currentAttackerIndex = (currentAttackerIndex + 1) % currentAggressiveMinions.Count;
      StartCoroutine(WaitThenStartNextAttack(3f));
    }
  }

  System.Collections.IEnumerator WaitThenStartNextAttack(float delay)
  {
    yield return new WaitForSeconds(delay);
    StartNextMinionAttack();
  }

  void StartNextMinionAttack()
  {
    if (currentAggressiveMinions.Count > 0 && !isMinionAttacking)
    {
      var nextMinion = currentAggressiveMinions[currentAttackerIndex];
      nextMinion.AttemptAttack();
    }
  }
  public bool AllMinionsDefeated()
  {
    foreach (var minion in minions)
    {
      if (minion.isAlive)
      {
        return false;
      }
    }
    return true;
  }

  public void RefreshMinions()
  {
    if (minions == null || minions.Count == 0)
    {
      minions = new List<MinionManager>();
    }

    foreach (Transform child in transform)
    {
      MinionManager minion = child.GetComponent<MinionManager>();
      if (minion != null && !minions.Contains(minion))
      {
        minions.Add(minion);
      }
    }

    Debug.Log($"MinionCampManager refreshed. Total minions: {minions.Count}");
  }

  void OnTransformChildrenChanged()
  {
    RefreshMinions();
  }
  public void SetLevel(int level)
  {
    currentLevel = level;
    AdjustAggressiveMinionCount();
  }

  private void AdjustAggressiveMinionCount()
  {
    if (currentLevel == 1)
    {
      maxAggressiveMinions = 5;
    }
    else if (currentLevel == 2)
    {
      maxAggressiveMinions = 3;
    }
    else
    {
      maxAggressiveMinions = 5; // Default value
    }

    Debug.Log($"Level set to {currentLevel}. Max aggressive minions: {maxAggressiveMinions}");
  }
}
