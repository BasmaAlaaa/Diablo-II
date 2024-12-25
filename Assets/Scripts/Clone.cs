using System.Collections;
using UnityEngine;
using UnityEngine.AI;
public class Clone : MonoBehaviour
{
  public GameObject clonePrefab;
  public GameObject cloneEffectPrefab;
  public WandererStats wandererStats;

  public float cloneLifetime = 5f;
  public float explosionRadius = 10f;
  private int explosionDamage = 10;
  public LayerMask groundLayer;
  public LayerMask enemyLayer;
  private float abilityRange = 100f;
  private float cooldownTime = 10f;
  public AudioClip cloningSound;
  private AudioSource audioSource;

  private Camera mainCamera;
  private bool isCloneModeActive = false;
  private bool isOnCooldown = false;
  private GameObject activeClone = null;
  private bool isOriginal = true;

  void Start()
  {
    mainCamera = Camera.main;
    audioSource = GetComponent<AudioSource>();
  }

  void Update()
  {
    if (!isOriginal) return;

    // Activate clone mode on pressing "Q" (only if not on cooldown and no active clone)
    if (Input.GetKeyDown(KeyCode.Q) && CanUseCloneAbility() && activeClone == null && wandererStats.unlockedAbilities.Contains("WildCard"))
    {
      Debug.Log("Clone mode activated. Right-click to place.");
      isCloneModeActive = true;
    }
    else if (Input.GetKeyDown(KeyCode.Q) && !CanUseCloneAbility() && wandererStats.unlockedAbilities.Contains("WildCard"))
    {
      Debug.Log("Clone ability is on cooldown. Please wait.");
    }
    else if (Input.GetKeyDown(KeyCode.Q) && activeClone != null)
    {
      Debug.Log("A clone is already active. Wait until it's destroyed.");
    }

    if (isCloneModeActive && Input.GetMouseButtonDown(1))
    {
      HandleClonePlacement();
    }
  }
  private bool CanUseCloneAbility()
  {
    if (wandererStats != null && wandererStats.toggleCooldown)
    {
      return true;
    }

    return !isOnCooldown;
  }

  private void HandleClonePlacement()
  {
    if (activeClone != null)
    {
      Debug.LogWarning("Cannot create a new clone while one is already active.");
      return;
    }

    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

    if (Physics.Raycast(ray, out RaycastHit hit, abilityRange, groundLayer))
    {
      if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
      {
        CreateClone(navHit.position);
      }
      else
      {
        Debug.Log("Invalid clone target: Not walkable.");
      }
    }
    else
    {
      Debug.Log("Invalid clone target: Out of range or no ground detected.");
    }

    isCloneModeActive = false;
  }

  private void CreateClone(Vector3 position)
  {
    Debug.Log($"Creating clone at: {position}");
    if (audioSource != null && cloningSound != null)
    {
      audioSource.PlayOneShot(cloningSound);
    }

    activeClone = Instantiate(clonePrefab, position, Quaternion.identity);

    Clone cloneScript = activeClone.GetComponent<Clone>();
    if (cloneScript != null)
    {
      cloneScript.isOriginal = false;
    }

    if (cloneEffectPrefab != null)
    {
      GameObject effect = Instantiate(cloneEffectPrefab, position, Quaternion.identity);
      Destroy(effect, 3f);
    }
    DisableMovementOnClone(activeClone);

    StartCoroutine(CloneLifecycle(activeClone));
  }

  private IEnumerator CloneLifecycle(GameObject clone)
  {
    yield return new WaitForSeconds(cloneLifetime);

    ExplodeClone(clone);

    Destroy(clone);

    activeClone = null;

    StartCoroutine(StartCooldown());
  }

  private void ExplodeClone(GameObject clone)
  {
    if (clone == null)
    {
      Debug.LogError("Clone is null. Explosion aborted.");
      return;
    }

    Debug.Log("Clone exploded!");

    Collider[] enemies = Physics.OverlapSphere(clone.transform.position, explosionRadius, enemyLayer);

    if (enemies.Length == 0)
    {
      Debug.LogWarning("No enemies found within the explosion radius.");
      return;
    }

    foreach (Collider enemy in enemies)
    {
      if (enemy.CompareTag("Minion"))
      {
        Debug.Log($"Applying damage to enemy: {enemy.name}");
        enemy.GetComponent<MinionManager>().TakeDamage(explosionDamage);
      }
      else if (enemy.CompareTag("Demon"))
      {
        Debug.Log("Applying damage to enemy: Demon!");
        enemy.GetComponent<DemonManager>().TakeDamage(explosionDamage);
      }
      else if (enemy.CompareTag("Jolleen"))
      {
        Debug.Log("Applying damage to enemy: Jolleen!");
        enemy.GetComponent<LilithHealth>().TakeDamage(explosionDamage);
      }
      else
      {
        Debug.LogWarning($"Collider {enemy.name} does not have the Enemy tag.");
      }
    }
  }

  private IEnumerator StartCooldown()
  {
    Debug.Log("Clone ability is now on cooldown.");
    isOnCooldown = true;
    yield return new WaitForSeconds(cooldownTime);
    isOnCooldown = false;
    Debug.Log("Clone ability is ready to use again.");
  }

  private void DisableMovementOnClone(GameObject clone)
  {
    NavMeshAgent agent = clone.GetComponent<NavMeshAgent>();
    if (agent != null)
    {
      agent.enabled = false;
      Debug.Log("NavMeshAgent disabled on clone.");
    }

    MovementController movement = clone.GetComponent<MovementController>();
    if (movement != null)
    {
      movement.enabled = false;
      Debug.Log("MovementController disabled on clone.");
    }

    Rigidbody rb = clone.GetComponent<Rigidbody>();
    if (rb != null)
    {
      rb.isKinematic = true;
      rb.constraints = RigidbodyConstraints.FreezeAll;
      Debug.Log("Rigidbody constraints applied to clone.");
    }
  }
}
