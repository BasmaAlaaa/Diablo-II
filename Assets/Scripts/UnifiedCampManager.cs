using System.Collections.Generic;
using UnityEngine;

public class UnifiedCampManager : MonoBehaviour
{
    [Header("Enemy Camp Settings")]
    public MinionCampManager minionCamp; 
    public CampManager demonCamp; 
    public Transform runeSpawnLocation;
    public GameObject runeFragmentPrefab; 

    private bool runeSpawned = false;

    void Update()
    {
        if (!runeSpawned && AllEnemiesDefeated())
        {
            SpawnRuneFragment();
        }
    }

    private bool AllEnemiesDefeated()
    {
        bool minionsDefeated = minionCamp != null && minionCamp.AllMinionsDefeated();

        bool demonsDefeated = demonCamp == null || demonCamp.AllDemonsDefeated();

        return minionsDefeated && demonsDefeated;
    }

    private void SpawnRuneFragment()
    {
        runeSpawned = true;
        if (runeFragmentPrefab != null && runeSpawnLocation != null)
        {
            Instantiate(runeFragmentPrefab, runeSpawnLocation.position, Quaternion.identity);
            Debug.Log("Rune Fragment spawned!");
        }
        else
        {
            Debug.LogWarning("Rune Fragment prefab or spawn location is missing!");
        }
    }
}
