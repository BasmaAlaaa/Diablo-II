using System.Collections;
using UnityEngine;

public class BashAbility : MonoBehaviour
{
  [Header("Bash Settings")]
  public float bashRange = 5f;
  public int bashDamage = 5;
  public float cooldownTime = 1f;

  private float lastUsedTime = -Mathf.Infinity;
  private GameObject selectedTarget;

  [Header("References")]
  public LayerMask targetLayerMask;
  private Animator animator;
  private AbilityManager abilityManager;

  private WandererManager wandererManager;


  void Start()
  {
    animator = GetComponent<Animator>();
    abilityManager = GetComponent<AbilityManager>();
    wandererManager = GetComponent<WandererManager>();

    if (animator == null || abilityManager == null)
    {
      Debug.LogError("Animator or AbilityManager not found on the Wanderer!");
    }
  }

  void Update()
  {
    if (Input.GetMouseButtonDown(1))
    {
      SelectTarget();
      TryUseBash();
    }
  }

  void SelectTarget()
  {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayerMask))
    {
      GameObject target = hit.collider.gameObject;

      if (target.CompareTag("Minion") || target.CompareTag("Demon") || target.CompareTag("Jolleen"))
      {
        selectedTarget = target;
        Debug.Log($"Selected target for Bash: {selectedTarget.name}");
      }
      else
      {
        Debug.Log("Invalid target for Bash!");
        selectedTarget = null;
      }
    }
    else
    {
      Debug.Log("No valid target selected!");
      selectedTarget = null;
    }
  }

  void TryUseBash()
  {
    if (!CanUseBash())
    {
      Debug.Log("Bash is on cooldown!");
      return;
    }

    if (selectedTarget == null)
    {
      Debug.Log("No target selected for Bash!");
      return;
    }

    float distanceToTarget = Vector3.Distance(transform.position, selectedTarget.transform.position);
    if (distanceToTarget > bashRange)
    {
      Debug.Log("Target is out of range for Bash!");
      return;
    }

    PerformBash();
  }

  bool CanUseBash()
  {
    if (wandererManager != null && wandererManager.toggleCooldown)
    {
      return true;
    }

    return Time.time - lastUsedTime >= cooldownTime;
  }


  void PerformBash()
  {
    if (animator != null)
    {
      animator.SetBool("DoBash", true);
    }

    if (selectedTarget.CompareTag("Minion"))
    {
      MinionManager minionManager = selectedTarget.GetComponent<MinionManager>();
      if (minionManager != null)
      {
        minionManager.TakeDamage("Bash");
        Debug.Log($"Bash dealt {bashDamage} damage to Minion: {selectedTarget.name}!");
      }
    }
    else if (selectedTarget.CompareTag("Demon"))
    {
      DemonManager demonManager = selectedTarget.GetComponent<DemonManager>();
      if (demonManager != null)
      {
        demonManager.TakeDamage("Bash");
        Debug.Log($"Bash dealt {bashDamage} damage to Demon: {selectedTarget.name}!");
      }
    }
    else if (selectedTarget.CompareTag("Jolleen"))
    {
      LilithHealth lilithHealth = selectedTarget.GetComponent<LilithHealth>();
      if (lilithHealth != null)
      {
        lilithHealth.TakeDamage(5);
        Debug.Log($"Bash dealt 5 damage to Jolleen: {selectedTarget.name}!");
      }
    }

    lastUsedTime = Time.time;

    selectedTarget = null;
    StartCoroutine(ResetDoBash());
  }

  IEnumerator ResetDoBash()
  {
    yield return new WaitForSeconds(1f);

    if (animator != null)
    {
      animator.SetBool("DoBash", false);
    }

    Debug.Log("Bash completed, DoBash reset to false.");
  }
}