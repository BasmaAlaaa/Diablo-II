using System.Collections;
using UnityEngine;
using System.Collections.Generic;


public class ChargeManager : MonoBehaviour
{
  [Header("Charge Settings")]
  private float chargeRange = 50f;
  public float chargeSpeed = 20f;
  public float cooldownTime = 10f;
  public int minionDamage = 20;
  public int demonDamage = 40;
  public float impactRadius = 5f;
  public LayerMask targetLayerMask;
  public LayerMask walkableLayer;
  public ParticleSystem chargeEffect;
  public AudioClip chargeSound;

  private AudioSource audioSource;

  private float lastUsedTime = -Mathf.Infinity;
  private Vector3 chargeTarget;
  private bool isSelectingChargeTarget = false;

  [Header("References")]
  private Animator animator;
  private AbilityManager abilityManager;

  private WandererManager wandererManager;

  void Start()
  {
    animator = GetComponent<Animator>();
    abilityManager = GetComponent<AbilityManager>();
    audioSource = GetComponent<AudioSource>();
    wandererManager = GetComponent<WandererManager>();

    if (animator == null || abilityManager == null)
    {
      Debug.LogError("Animator or AbilityManager not found on the Wanderer!");
    }
  }

  void Update()
  {
    if (!abilityManager.IsAbilityUnlocked("Ultimate")) return;

    // Activate Charge (press "E")
    if (Input.GetKeyDown(KeyCode.E))
    {
      StartChargeSelection();
    }

    // Select a target position with right mouse click
    if (Input.GetMouseButtonDown(1) && isSelectingChargeTarget)
    {
      SelectChargeTarget();
    }
  }

  void StartChargeSelection()
  {
    if (!CanUseCharge())
    {
      float remainingCooldown = Mathf.Ceil(lastUsedTime + cooldownTime - Time.time);
      Debug.Log($"Charge is on cooldown! Time remaining: {remainingCooldown} seconds.");
      return;
    }

    isSelectingChargeTarget = true;
    Debug.Log("Charge activated! Select a position.");
  }

  bool CanUseCharge()
  {
    if (wandererManager != null && wandererManager.toggleCooldown)
    {
      return true; // Always allow Charge when cheat is active
    }

    return Time.time - lastUsedTime >= cooldownTime;
  }

  void SelectChargeTarget()
  {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    Debug.Log($"Ray Origin: {ray.origin}, Direction: {ray.direction}");

    if (Physics.Raycast(ray, out hit, chargeRange, walkableLayer))
    {
      chargeTarget = hit.point;
      Debug.Log($"Charge target selected at: {chargeTarget} (Hit Object: {hit.collider.gameObject.name})");

      StartCoroutine(PerformCharge());
    }
    else
    {
      Debug.Log("Invalid charge target! Please select a walkable position within range.");

      if (Physics.Raycast(ray, out hit, chargeRange, LayerMask.GetMask("Everything")))
      {
        Debug.Log($"Ray hit: {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
        Debug.Log($"Hit position: {hit.point}");
      }
      else
      {
        Debug.Log("Ray did not hit any collider.");
      }
    }

    isSelectingChargeTarget = false;
  }

  IEnumerator ResetAnimatorBool(string parameterName, float delay)
  {
    yield return new WaitForSeconds(delay);

    if (animator != null)
    {
      animator.SetBool(parameterName, false);

    }
  }

  private HashSet<GameObject> impactedObjects = new HashSet<GameObject>();

  IEnumerator PerformCharge()
  {
    if (audioSource != null && chargeSound != null)
    {
      audioSource.PlayOneShot(chargeSound);
    }

    if (animator != null)
    {
      animator.SetBool("DoCharge", true);
      StartCoroutine(ResetAnimatorBool("DoCharge", 0.6f));
    }

    chargeEffect.Play();

    Vector3 startPosition = transform.position;
    float elapsedTime = 0f;
    float totalDistance = Vector3.Distance(startPosition, chargeTarget);

    impactedObjects.Clear();

    while (elapsedTime < totalDistance / chargeSpeed)
    {
      transform.position = Vector3.Lerp(startPosition, chargeTarget, (elapsedTime * chargeSpeed) / totalDistance);
      elapsedTime += Time.deltaTime;

      HandleChargeImpact(transform.position);

      yield return null;
    }

    transform.position = chargeTarget;
    chargeEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

    lastUsedTime = Time.time;
    Debug.Log("Charge completed!");
  }

  void HandleChargeImpact(Vector3 currentPosition)
  {
    Collider[] hitColliders = Physics.OverlapSphere(currentPosition, impactRadius, targetLayerMask);

    foreach (Collider hitCollider in hitColliders)
    {
      GameObject hitObject = hitCollider.gameObject;

      if (impactedObjects.Contains(hitObject)) continue;

      impactedObjects.Add(hitObject);

      if (hitObject.CompareTag("Minion"))
      {
        MinionManager minionManager = hitObject.GetComponent<MinionManager>();
        if (minionManager != null)
        {
          minionManager.TakeDamage("Charge");
          Debug.Log($"Charge hit Minion: {hitObject.name}, dealing {minionDamage} damage!");
        }
      }
      else if (hitObject.CompareTag("Demon"))
      {
        DemonManager demonManager = hitObject.GetComponent<DemonManager>();
        if (demonManager != null)
        {
          demonManager.TakeDamage("Charge");
          Debug.Log($"Charge hit Demon: {hitObject.name}, dealing {demonDamage} damage!");
        }
      }
      else if (hitObject.CompareTag("Jolleen"))
      {
        LilithHealth lilithHealth = hitObject.GetComponent<LilithHealth>();
        if (lilithHealth != null)
        {
          lilithHealth.TakeDamage(20);
          Debug.Log($"Charge hit Jolleen: {hitObject.name}, dealing 20 damage!");
        }
      }
    }
  }


  void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.blue;
    Gizmos.DrawWireSphere(transform.position, chargeRange);

    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, impactRadius);
  }
}
