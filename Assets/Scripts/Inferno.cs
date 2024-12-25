using System.Collections;
using UnityEngine;

public class Inferno : MonoBehaviour
{
  public GameObject infernoPrefab;
  public float infernoRadius = 5f;
  public WandererStats wandererStats;

  private float infernoDuration = 5f;
  private float cooldownTime = 15f;
  private int initialDamage = 10;
  private int damagePerSecond = 2;
  public LayerMask groundLayer;
  public LayerMask enemyLayer;
  private float abilityRange = 100f;

  private Camera mainCamera;
  private bool isInfernoModeActive = false;
  private bool isOnCooldown = false;
  public AudioClip infernoSound;
  private AudioSource audioSource;

  void Start()
  {
    mainCamera = Camera.main;
    audioSource = GetComponent<AudioSource>();
  }

  void Update()
  {
    // Activate inferno mode on pressing "E" (only if not on cooldown)
    if (Input.GetKeyDown(KeyCode.E) && wandererStats.unlockedAbilities.Contains("Ultimate") && CanUseInfernoAbility())
    {
      Debug.Log("Inferno mode activated. Right-click to place.");
      isInfernoModeActive = true;
    }
    else if (Input.GetKeyDown(KeyCode.E) && !CanUseInfernoAbility())
    {
      Debug.Log($"Inferno ability is on cooldown.");
    }

    if (isInfernoModeActive && Input.GetMouseButtonDown(1)) // Right mouse button
    {
      HandleInfernoPlacement();
    }
  }
  private bool CanUseInfernoAbility()
  {
    if (wandererStats != null && wandererStats.toggleCooldown)
    {
      return true;
    }

    return !isOnCooldown;
  }


  private void HandleInfernoPlacement()
  {
    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

    if (Physics.Raycast(ray, out RaycastHit hit, abilityRange, groundLayer))
    {
      Debug.Log($"Inferno placed at: {hit.point}");
      CreateInferno(hit.point);
    }
    else
    {
      Debug.Log("Invalid placement: Out of range or no ground detected.");
    }

    isInfernoModeActive = false;
  }

  private void CreateInferno(Vector3 position)
  {
    GameObject inferno = Instantiate(infernoPrefab, position, Quaternion.identity);
    inferno.transform.rotation = Quaternion.Euler(-90, 0, 0);
    Destroy(inferno, infernoDuration);
    if (audioSource != null && infernoSound != null)
    {
      audioSource.PlayOneShot(infernoSound);
      Debug.Log("Inferno sound played.");
    }

    StartCoroutine(InfernoLifecycle(inferno, position));
  }

  private IEnumerator InfernoLifecycle(GameObject inferno, Vector3 position)
  {
    StartCoroutine(StartCooldown());

    ApplyDamageToEnemies(position, initialDamage);

    float elapsedTime = 0f;

    while (elapsedTime < infernoDuration)
    {
      yield return new WaitForSeconds(1f);
      ApplyDamageToEnemies(position, damagePerSecond);
      elapsedTime += 1f;
    }

  }

  private IEnumerator StartCooldown()
  {
    Debug.Log("Inferno ability is now on cooldown.");
    isOnCooldown = true;
    yield return new WaitForSeconds(cooldownTime);
    isOnCooldown = false;
    Debug.Log("Inferno ability is ready to use again.");
  }

  private void ApplyDamageToEnemies(Vector3 position, int damage)
  {
    Collider[] enemies = Physics.OverlapSphere(position, infernoRadius, enemyLayer);

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
        enemy.GetComponent<MinionManager>().TakeDamage(damage);

      }
      else if (enemy.CompareTag("Demon"))
      {
        Debug.Log("Applying damage to enemy: Demon!");
        enemy.GetComponent<DemonManager>().TakeDamage(damage);
      }
      else if (enemy.CompareTag("Jolleen"))
      {
        Debug.Log("Applying damage to enemy: Jolleen!");
        enemy.GetComponent<LilithHealth>().TakeDamage(damage);
      }
      else
      {
        Debug.LogWarning($"Collider {enemy.name} does not have the Enemy tag.");
      }
    }
  }
}
