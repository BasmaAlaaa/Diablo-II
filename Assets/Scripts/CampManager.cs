using System.Collections.Generic;
using UnityEngine;

public class CampManager : MonoBehaviour
{
  public List<DemonManager> demons;
  public Transform campCenter;
  public float campRange = 15f;
  public int maxAggressiveDemons = 1;

  private Transform wanderer;
  private bool isWandererInRange = false;

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
  }

  void Update()
  {
    CheckWandererProximity();

    if (isWandererInRange)
    {
      HandleAggressiveDemons();
    }
    else
    {
      ResetAllDemons();
    }
  }

  void CheckWandererProximity()
  {
    float distanceToWanderer = Vector3.Distance(campCenter.position, wanderer.position);
    isWandererInRange = distanceToWanderer <= campRange;
  }

  void HandleAggressiveDemons()
  {
    DemonManager closestDemon = null;
    float closestDistance = Mathf.Infinity;

    foreach (var demon in demons)
    {
      if (!demon.isAlive) continue;

      float distanceToWanderer = Vector3.Distance(demon.transform.position, wanderer.position);

      if (distanceToWanderer <= demon.followRange && distanceToWanderer < closestDistance)
      {
        closestDemon = demon;
        closestDistance = distanceToWanderer;
      }
    }

    foreach (var demon in demons)
    {
      if (demon == closestDemon && closestDemon != null)
      {
        if (!demon.isAggressive)
        {
          demon.BecomeAggressive();
        }
      }
      else
      {
        if (demon.isAggressive)
        {
          demon.ResetToPatrolling();
        }
      }
    }

  }

  void ResetAllDemons()
  {
    foreach (var demon in demons)
    {
      if (!demon.isAlive) continue;

      demon.ResetToPatrolling();
    }
  }

  public bool AllDemonsDefeated()
  {
    foreach (var demon in demons)
    {
      if (demon.isAlive)
      {
        return false;
      }
    }
    return true;
  }
}
