using System.Collections;
using UnityEngine;

public class BasicAbility : MonoBehaviour
{
  public GameObject fireballPrefab;
  public Transform fireballSpawnPoint;
  public float cooldownTime = 1f;
  private Animator animator;
  private MovementController movementController;
  public WandererStats wandererStats;
  private Camera mainCamera;
  private bool fireballSpawned = false;
  private float lastFireballTime = -Mathf.Infinity;
  private Vector3 lockedTargetPosition;
  private Quaternion targetRotation;
  private bool isRotating = false;
  private bool isThrowing = false;
  public AudioClip fireballSound;
  private AudioSource audioSource;


  void Start()
  {
    animator = GetComponent<Animator>();
    movementController = GetComponent<MovementController>();
    mainCamera = Camera.main;
    audioSource = GetComponent<AudioSource>();
  }

  private void Update()
  {
    // Check for right mouse click
    if (Input.GetMouseButtonDown(1))
    {
      HandleMouseClick();
    }

    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

    if (stateInfo.IsName("Throw"))
    {
      if (stateInfo.normalizedTime >= 0.5f && !fireballSpawned)
      {
        SpawnFireball();
        fireballSpawned = true;
      }

      if (stateInfo.normalizedTime >= 1.0f)
      {
        animator.SetInteger("Throw", 0);
        movementController.canMove = true;
        isThrowing = false;

      }
    }
    if (isRotating)
    {
      transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

      if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
      {
        isRotating = false;
      }
    }
  }

  private void HandleMouseClick()
  {
    if (isThrowing)
    {
      Debug.Log("Already throwing, ignoring additional input.");
      return;
    }

    if (!CanUseAbility())
    {
      Debug.Log($"Fireball on cooldown. Time remaining: {Mathf.Ceil(lastFireballTime + cooldownTime - Time.time)} seconds.");
      return;
    }

    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

    if (Physics.Raycast(ray, out RaycastHit hit))
    {
      if (hit.collider.CompareTag("Demon") || hit.collider.CompareTag("Minion") || hit.collider.CompareTag("Jolleen"))
      {
        Debug.Log("Enemy clicked, initiating throw animation");

        lockedTargetPosition = hit.collider.transform.position;
        lockedTargetPosition.y += hit.collider.bounds.size.y / 2;

        Vector3 lookDirection = (lockedTargetPosition - transform.position).normalized;
        lookDirection.y = 0;
        targetRotation = Quaternion.LookRotation(lookDirection);

        isRotating = true;

        animator.SetInteger("Throw", 1);
        fireballSpawned = false;
        movementController.canMove = false;

        lastFireballTime = Time.time;
        isThrowing = true;
        StartCoroutine(ResetThrowAnimation());
      }
    }
  }

  private IEnumerator ResetThrowAnimation()
  {
    yield return new WaitForSeconds(3f);
    animator.SetInteger("Throw", 0); 
    movementController.canMove = true; 
    isThrowing = false; 
    Debug.Log("Throw animation reset to 0.");
  }

  private bool CanUseAbility()
  {
    if (wandererStats != null && wandererStats.toggleCooldown)
    {
      return true;
    }

    return Time.time >= lastFireballTime + cooldownTime;
  }
  private void SpawnFireball()
  {
    Debug.Log("Spawning Fireball");

    if (lockedTargetPosition == null)
    {
      Debug.Log("No locked target position for fireball.");
      return;
    }

    GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);

    Vector3 direction = (lockedTargetPosition - fireballSpawnPoint.position).normalized;

    fireball.GetComponent<Fireball>().SetDirection(direction);
    Quaternion targetRotation = Quaternion.LookRotation(direction);
    fireball.transform.rotation = targetRotation * Quaternion.Euler(0, -90, 0);

    Debug.Log($"Fireball spawned at {fireballSpawnPoint.position} and traveling to {lockedTargetPosition}");
    if (audioSource != null && fireballSound != null)
    {
      audioSource.PlayOneShot(fireballSound);
    }
  }
}