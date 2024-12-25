using System.Collections;
using UnityEngine;

public class IronMaelstromManager : MonoBehaviour
{
  [Header("Iron Maelstrom Settings")]
  public float maelstromRange = 5f;
  public int maelstromDamage = 10;
  public float cooldownTime = 5f;

  private float lastUsedTime = -Mathf.Infinity;

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
    if (!abilityManager.IsAbilityUnlocked("WildCard")) return;

    // Press a key to activate Iron Maelstrom 
    if (Input.GetKeyDown(KeyCode.Q))
    {
      TryUseIronMaelstrom();
    }
  }
  void TryUseIronMaelstrom()
  {
    if (wandererManager.toggleCooldown || Time.time - lastUsedTime >= cooldownTime)
    {
      PerformIronMaelstrom();
    }
    else
    {
      float remainingCooldown = Mathf.Ceil(lastUsedTime + cooldownTime - Time.time);
      Debug.Log($"Iron Maelstrom is on cooldown! Time remaining: {remainingCooldown} seconds.");
      return;
    }
  }

  void PerformIronMaelstrom()
  {
    if (animator != null)
    {
      animator.SetBool("DoIronMaelstrom", true);
    }

    Debug.Log("Iron Maelstrom activated!");

    Collider[] hitColliders = Physics.OverlapSphere(transform.position, maelstromRange, targetLayerMask);

    foreach (Collider hitCollider in hitColliders)
    {
      GameObject target = hitCollider.gameObject;

      if (target.CompareTag("Minion"))
      {
        MinionManager minionManager = target.GetComponent<MinionManager>();
        if (minionManager != null)
        {
          minionManager.TakeDamage("Iron Maelstrom");
          Debug.Log($"Iron Maelstrom hit Minion: {target.name}, dealing {maelstromDamage} damage!");
        }
      }
      else if (target.CompareTag("Demon"))
      {
        DemonManager demonManager = target.GetComponent<DemonManager>();
        if (demonManager != null)
        {
          demonManager.TakeDamage("Iron Maelstrom");
          Debug.Log($"Iron Maelstrom hit Demon: {target.name}, dealing {maelstromDamage} damage!");
        }
      }
      else if (target.CompareTag("Jolleen"))
      {
        LilithHealth lilithHealth = target.GetComponent<LilithHealth>();
        if (lilithHealth != null)
        {
          lilithHealth.TakeDamage(10);
          Debug.Log($"Iron Maelstrom hit Jolleen: {target.name}, dealing 10 damage!");
        }
      }
    }

    lastUsedTime = Time.time;

    StartCoroutine(ResetIronMaelstromAnimation());
  }

  IEnumerator ResetIronMaelstromAnimation()
  {
    yield return new WaitForSeconds(2f);

    if (animator != null)
    {
      animator.SetBool("DoIronMaelstrom", false);
    }

    Debug.Log("Iron Maelstrom animation completed.");
  }

  void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, maelstromRange);
  }
}
