using System.Collections;
using UnityEngine;

public class ShieldManager : MonoBehaviour
{
  [Header("Shield Settings")]
  public float shieldDuration = 3f;
  public float cooldownTime = 10f;
  public ParticleSystem shieldEffect;
  public AudioClip shieldSound;
  private bool isShieldActive = false;
  private float lastUsedTime = -Mathf.Infinity;

  [Header("References")]
  private Animator animator;
  private WandererManager wandererManager;
  private AbilityManager abilityManager;
  private AudioSource audioSource;

  void Start()
  {
    animator = GetComponent<Animator>();
    abilityManager = GetComponent<AbilityManager>();
    audioSource = GetComponent<AudioSource>();

    if (animator == null || abilityManager == null || audioSource == null)
    {
      Debug.LogError("Animator, AbilityManager, or AudioSource not found on the Wanderer!");
    }

    wandererManager = GetComponent<WandererManager>();
    if (wandererManager == null)
    {
      Debug.LogError("WandererManager not found! Please ensure the WandererManager script is attached.");
    }
  }

  void Update()
  {
    if (!abilityManager.IsAbilityUnlocked("Defensive")) return;

    // Press a key to activate the Shield
    if (Input.GetKeyDown(KeyCode.W))
    {
      TryActivateShield();
    }
  }

  void TryActivateShield()
  {
    if (!CanUseShield())
    {
      float remainingCooldown = Mathf.Ceil(lastUsedTime + cooldownTime - Time.time);
      Debug.Log($"Shield is on cooldown! Time remaining: {remainingCooldown} seconds.");
      return;
    }

    ActivateShield();
  }

  bool CanUseShield()
  {
    if (wandererManager != null && wandererManager.toggleCooldown)
    {
      return true;
    }

    return Time.time - lastUsedTime >= cooldownTime;
  }
  void ActivateShield()
  {
    isShieldActive = true;
    lastUsedTime = Time.time;

    if (wandererManager != null)
    {
      wandererManager.SetInvincibility(true);
    }

    // Play shield sound
    if (audioSource != null && shieldSound != null)
    {
      audioSource.PlayOneShot(shieldSound);
    }

    // Play shield visual effect
    shieldEffect.Play();

    // Trigger shield animation
    if (animator != null)
    {
      animator.SetBool("DoShield", true);
    }

    Debug.Log("Shield activated!");

    StartCoroutine(StopShieldAnimation());
    StartCoroutine(DeactivateShieldAfterDuration());
  }

  IEnumerator StopShieldAnimation()
  {
    yield return new WaitForSeconds(2f);

    if (animator != null)
    {
      animator.SetBool("DoShield", false);
    }

    if (shieldEffect != null)
    {
      shieldEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    Debug.Log("Shield animation stopped, shield remains active.");
  }

  IEnumerator DeactivateShieldAfterDuration()
  {
    yield return new WaitForSeconds(shieldDuration);

    isShieldActive = false;

    if (wandererManager != null)
    {
      wandererManager.SetInvincibility(false);
    }

    Debug.Log("Shield deactivated!");
  }

  public bool IsShieldActive()
  {
    return isShieldActive;
  }
}
