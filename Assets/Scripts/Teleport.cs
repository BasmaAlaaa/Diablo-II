using UnityEngine;
using UnityEngine.AI;

public class Teleport : MonoBehaviour
{
  private float teleportRange = 100f; // Maximum range for teleportation  
  public LayerMask groundLayer; // Layer to specify valid teleport positions
  public GameObject teleportEffectPrefab; // The ring effect prefab
  public WandererStats wandererStats; // Reference to WandererStats

  public float cooldownTime = 10f; // Cooldown duration in seconds
  private NavMeshAgent agent; // Reference to NavMeshAgent
  private Camera mainCamera; // Reference to the main camera
  private Animator animator; // Reference to the Animator

  private bool isTeleportModeActive = false; // Track if teleport mode is active
  private float lastTeleportTime = -10f; // Time of the last teleport (initialize to ensure immediate use)

  void Start()
  {
    agent = GetComponent<NavMeshAgent>();
    animator = GetComponent<Animator>();
    mainCamera = Camera.main; // Cache the main camera
  }

  void Update()
  {
    // Step 1: Activate teleport mode on pressing "W" (if cooldown has elapsed)
    if (Input.GetKeyDown(KeyCode.W) && CanTeleport() && wandererStats.unlockedAbilities.Contains("Defensive"))
    {
      Debug.Log("Teleport mode activated. Right-click to teleport.");
      isTeleportModeActive = true;
    }
    else if (Input.GetKeyDown(KeyCode.W) && wandererStats.unlockedAbilities.Contains("Defensive"))
    {
      Debug.Log($"Teleport ability on cooldown. Time remaining: {Mathf.Ceil(lastTeleportTime + cooldownTime - Time.time)} seconds.");
    }

    // Step 2: Wait for right-click to specify the teleport target
    if (isTeleportModeActive && Input.GetMouseButtonDown(1)) // Right mouse button
    {
      HandleTeleport();
    }
  }
  private bool CanTeleport()
  {
    if (wandererStats != null && wandererStats.toggleCooldown)
    {
      return true; // Always allow teleport if the cheat is active
    }

    return Time.time >= lastTeleportTime + cooldownTime;
  }


  private void HandleTeleport()
  {
    // Cast a ray from the camera through the mouse position
    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

    if (Physics.Raycast(ray, out RaycastHit hit, teleportRange, groundLayer))
    {
      // Check if the target position is walkable on the NavMesh
      if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
      {
        PlayTeleportAnimation(targetPosition: navHit.position);
        PerformTeleport(navHit.position);

        // Set the last teleport time for cooldown tracking
        lastTeleportTime = Time.time;
      }
      else
      {
        Debug.Log("Invalid teleport target: Not walkable.");
      }
    }
    else
    {
      Debug.Log("Invalid teleport target: Out of range or no ground detected.");
    }

    // Reset teleport mode
    isTeleportModeActive = false;
  }

  private void PerformTeleport(Vector3 targetPosition)
  {
    Debug.Log($"Teleporting to: {targetPosition}");

    // Instantly move the player to the target position
    agent.Warp(targetPosition);

    // Reinitialize NavMeshAgent to ensure it's properly placed on the NavMesh
    agent.SetDestination(agent.transform.position); // Force agent to refresh its state
    PlayTeleportEffect(targetPosition);

  }

  private void PlayTeleportAnimation(Vector3 targetPosition)
  {
    Debug.Log("Playing teleport animation.");

    // Trigger teleport animation
    animator.SetTrigger("Teleport");
  }
  private void PlayTeleportEffect(Vector3 position)
  {
    if (teleportEffectPrefab != null)
    {
      // Instantiate the teleport effect at the given position
      GameObject teleportEffect = Instantiate(teleportEffectPrefab, position, Quaternion.identity);
      teleportEffect.transform.rotation = Quaternion.Euler(-90, 0, 0);

      // Destroy the effect after its duration (e.g., 2 seconds)
      Destroy(teleportEffect, 3f);
    }
    else
    {
      Debug.LogWarning("Teleport effect prefab is not assigned.");
    }
  }
}
