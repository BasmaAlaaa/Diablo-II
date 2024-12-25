using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilithAttack : MonoBehaviour
{
  private Animator animator;
  private LilithHealth health;
  public bool isReflectiveAuraActive = false;
  public GameObject auraEffect;
  public GameObject minionPrefab;

  [Header("Wanderer References")]
  private Transform wanderer;
  private WandererManager wandererManager;
  private WandererStats wandererStats;
  private float spikeRange = 2f;
  private float mediumRange = 10f;
  public GameObject bloodSpikesPrefab;
  public GameObject stompEffectPrefab;

  [Header("Sound Effects")]
  public AudioClip summonMinionsSound;
  public AudioClip stompSound;
  public AudioClip auraSpellSound;
  public AudioClip raiseSpikesSound;
  private AudioSource audioSource;


  private Coroutine phaseOneCoroutine;
  private bool canSpawnMinions = true;
  private bool canActivateAura = true;
  private Vector3 originalPosition;

  void Start()
  {
    CharacterManager characterManager = FindObjectOfType<CharacterManager>();
    if (characterManager != null)
    {
      GameObject activePlayer = characterManager.GetActivePlayer();
      if (activePlayer != null)
      {
        wanderer = activePlayer.transform;
        wandererManager = wanderer.GetComponent<WandererManager>();
        wandererStats = wanderer.GetComponent<WandererStats>();
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
    animator = GetComponent<Animator>();
    health = GetComponent<LilithHealth>();

    health.OnDeath += PlayDieAnimation;
    health.OnPhaseTwo += TriggerPhaseTwoAnimation;
    health.OnDamage += TakeDamageAnimation;
    health.OnPhaseOne += TriggerPhaseOneAnimation;

    originalPosition = transform.position;

    if (wanderer == null)
    {
      Debug.LogError("Wanderer GameObject not assigned in LilithAttack.");
    }

    if (wandererStats == null && wandererManager == null)
    {
      Debug.LogError("Both WandererStats and WandererManager are null. Assign at least one in the Inspector!");
    }

    audioSource = GetComponent<AudioSource>();
    if (audioSource == null)
    {
      Debug.LogError("AudioSource not found! Please attach an AudioSource component.");
    }

  }

  void Update()
  {
    if (wanderer != null)
    {
      CharacterManager characterManager = FindObjectOfType<CharacterManager>();

      GameObject activePlayer = characterManager.GetActivePlayer();
      if (activePlayer != null)
      {
        wanderer = activePlayer.transform;
        wandererManager = wanderer.GetComponent<WandererManager>();
        wandererStats = wanderer.GetComponent<WandererStats>();

      }

      // Debug.Log("Lilith is looking at the Wanderer named " + wanderer.name);
      Vector3 lookAtPosition = wanderer.transform.position;
      lookAtPosition.y = transform.position.y;
      transform.LookAt(lookAtPosition);
    }

    if (transform.position != originalPosition)
    {
      ResetToOriginalPosition();
    }
  }
  private void ResetToOriginalPosition()
  {
    transform.position = originalPosition;
  }

  void TakeDamageAnimation()
  {
    if (!isReflectiveAuraActive)
    {

      // Debug.Log("Animation Take Damage");
      animator.SetTrigger("TakeDamageTrigger");
    }
    else
    {
      Debug.Log("Reflecting damage!");
      DeactivateReflectiveAura();
    }


  }

  void TriggerPhaseTwoAnimation()
  {
    StopCoroutine(phaseOneCoroutine);
    phaseOneCoroutine = null;
    StartCoroutine(PhaseTwoBehavior());
  }

  public void Divebomb()
  {
    Vector3 originalPosition = transform.position;

    StartCoroutine(DivebombRoutine(originalPosition));
  }


  private IEnumerator DivebombRoutine(Vector3 originalPosition)
  {
    animator.SetTrigger("DivebombTrigger");
    Debug.Log("Divebomb animation started!");

    yield return new WaitForSeconds(2f);
    audioSource.PlayOneShot(stompSound);
    yield return new WaitForSeconds(1f);

    GameObject stompEffect = Instantiate(stompEffectPrefab, transform.position, Quaternion.identity);
    stompEffect.transform.localScale = new Vector3(mediumRange, 1, mediumRange);
    stompEffect.SetActive(true);
    Destroy(stompEffect, 2f); // Destroy effect after 2 seconds

    Collider[] hitColliders = Physics.OverlapSphere(transform.position, mediumRange);
    foreach (Collider collider in hitColliders)
    {
      if (collider.CompareTag("Player"))
      {
        if (wandererStats != null)
        {
          if (wandererStats.currentHP > 0)
          {
            wandererStats.TakeDamage(20);
            Debug.Log($"Divebomb hit the Wanderer! Damage taken: 20. Current Health: {wandererStats.currentHP}");
          }
        }
        else if (wandererManager != null)
        {
          if (wandererManager.currentHP > 0)
          {
            wandererManager.TakeDamage(20);
            Debug.Log("Divebomb hit the Wanderer! Damage taken: 20. Current Health: + { wandererManager.currentHP}");
          }
        }
        else
        {
          Debug.LogError("No valid WandererStats or WandererManager found to apply damage!");
        }
      }
    }

    yield return new WaitForSeconds(1f);

    transform.position = originalPosition;
    Debug.Log("Lilith's position reset after Divebomb.");
  }


  //PHASE TWO ATTACKS
  public void ReflectiveAura()
  {
    if (!canActivateAura)
    {
      Debug.Log("Reflective Aura is on cooldown.");
      return;
    }

    if (isReflectiveAuraActive)
    {
      Debug.Log("Aura is already active.");
      return;
    }

    Debug.Log("Aura is not active. Activating Aura.");
    StartCoroutine(ActivateAuraWithDelay());

  }

  private IEnumerator AuraCooldown()
  {
    canActivateAura = false;
    yield return new WaitForSeconds(10);
    canActivateAura = true;
    Debug.Log("Reflective Aura is ready to use again.");
  }
  private IEnumerator ActivateAuraWithDelay()
  {

    animator.SetTrigger("ReflectiveAuraTrigger");
    audioSource.PlayOneShot(auraSpellSound);
    // wait for 3 seconds then show aura
    yield return new WaitForSeconds(5);
    auraEffect.SetActive(true);
    isReflectiveAuraActive = true;

    Debug.Log("Reflective Aura activated!");
  }

  public void DeactivateReflectiveAura()
  {
    isReflectiveAuraActive = false;
    auraEffect.SetActive(false);

    Debug.Log("Reflective Aura deactivated!, START COOLDOWN.");
    StartCoroutine(AuraCooldown());

  }

  public void ReflectDamage(int damage)
  {
    Debug.Log("REFLECTING DAMAGE!");

    if (wandererStats != null)
    {
      wandererStats.TakeDamage(damage + 15);
      Debug.Log($"Damage reflected to Wanderer! Total damage: {damage + 15}. Current Health: {wandererStats.currentHP}");
    }
    else if (wandererManager != null)
    {
      wandererManager.TakeDamage(damage + 15);
      Debug.Log($"Damage reflected to Wanderer! Total damage: {damage + 15} (Handled by WandererManager).");
    }
    else
    {
      Debug.LogError("No valid WandererStats or WandererManager found to reflect damage!");
    }
  }




  void BloodSpikes()
  {
    if (isReflectiveAuraActive)
    {
      return;
    }
    else
    {
      animator.SetTrigger("BloodSpikesTrigger");
      audioSource.PlayOneShot(raiseSpikesSound);

      Debug.Log("Blood Spikes attack!");

      Vector3 attackPosition = transform.position + transform.forward * 3f;
      Debug.Log("Attack Position: " + attackPosition);
      CreateBloodSpikes(attackPosition);
    }

  }

  void CreateBloodSpikes(Vector3 startPosition)
  {
    int numberOfSpikes = 5;
    float spacing = 2f;
    float spawnDelay = 0.2f;
    float destructionDelay = 3f;
    int spikeDamage = 30;

    StartCoroutine(SpawnAndDestroySpikes(startPosition, numberOfSpikes, spacing, spawnDelay, destructionDelay, spikeDamage));
  }

  IEnumerator SpawnAndDestroySpikes(Vector3 startPosition, int numberOfSpikes, float spacing, float spawnDelay, float destructionDelay, int spikeDamage)
  {
    List<GameObject> spikesInstances = new List<GameObject>();

    for (int i = 0; i < numberOfSpikes; i++)
    {
      Vector3 spikePosition = startPosition + transform.forward * (i * spacing);

      GameObject bloodSpikesInstance = Instantiate(bloodSpikesPrefab, spikePosition, Quaternion.identity);
      spikesInstances.Add(bloodSpikesInstance);

      bloodSpikesInstance.SetActive(true);

      Animator animator2 = bloodSpikesInstance.GetComponent<Animator>();
      if (animator2 != null)
      {
        animator2.SetTrigger("open");
      }

      if (IsWandererOnSpike(spikePosition))
      {
        ApplyDamageToWanderer(spikeDamage);
      }

      yield return new WaitForSeconds(spawnDelay);
    }

    yield return new WaitForSeconds(destructionDelay);

    foreach (GameObject spike in spikesInstances)
    {
      if (spike != null)
      {
        Destroy(spike);
      }
    }

    Debug.Log("All spikes destroyed.");
  }

  bool IsWandererOnSpike(Vector3 spikePosition)
  {
    float distance = Vector3.Distance(wanderer.transform.position, spikePosition);

    if (distance <= spikeRange)
    {
      Debug.Log($"Wanderer is on the spike at position: {spikePosition}");
      return true;
    }

    return false;
  }


  void ApplyDamageToWanderer(int damage)
  {
    if (wandererStats != null)
    {
      wandererStats.TakeDamage(damage);
      Debug.Log($"Wanderer took {damage} damage. Current Health: {wandererStats.currentHP}");
    }
    if (wandererManager != null)
    {
      wandererManager.TakeDamage(damage);
      Debug.Log($"Wanderer took {damage} damage. Handled by WandererManager.");
    }
  }

  IEnumerator PhaseTwoBehavior()
  {
    animator.SetTrigger("PhaseTwoTrigger");
    yield return new WaitForSeconds(8);
    while (true)
    {
      ReflectiveAura();
      yield return new WaitForSeconds(5);

      BloodSpikes();
      yield return new WaitForSeconds(5);
    }
  }


  public void TriggerPhaseOneAnimation()
  {
    phaseOneCoroutine = StartCoroutine(PhaseOneBehaviour());


  }

  IEnumerator PhaseOneBehaviour()
  {
    yield return new WaitForSeconds(2);
    while (true)
    {
      Divebomb();
      yield return new WaitForSeconds(5);

      CheckIfSpawnMinion();
      yield return new WaitForSeconds(5);
    }
  }


  public void CheckIfSpawnMinion()
  {
    GameObject[] minions = GameObject.FindGameObjectsWithTag("Minion");
    if (minions.Length == 0 && !canSpawnMinions)
    {
      Debug.Log("Minions are dead. Cooldown 7 seconds before summoning again.");
      StartCoroutine(CooldownBeforeSummoning());
    }
    if (!canSpawnMinions)
    {
      Debug.Log("Minions are ALIVE.");
      return;
    }

    if (minions.Length == 0 && canSpawnMinions)
    {
      StartCoroutine(SummonMinionsWithDelay(minionPrefab));
    }
  }

  private IEnumerator CooldownBeforeSummoning()
  {
    canSpawnMinions = false;
    yield return new WaitForSeconds(7);
    canSpawnMinions = true;
  }

  private IEnumerator SummonMinionsWithDelay(GameObject minionPrefab)
  {
    canSpawnMinions = false;
    animator.SetTrigger("SummonTrigger");
    Debug.Log("Summon animation started.");
    audioSource.PlayOneShot(summonMinionsSound);

    yield return new WaitForSeconds(3f);

    Debug.Log("Summoning Minions!");

    int maxMinions = 3;
    float spawnRadius = 10f;
    float minimumDistance = 5f;

    GameObject minionCamp = GameObject.Find("Minion Camp");
    if (minionCamp == null)
    {
      minionCamp = new GameObject("Minion Camp");
      minionCamp.transform.position = transform.position;
      var campCollider = minionCamp.AddComponent<SphereCollider>();
      campCollider.isTrigger = true;
      campCollider.radius = 15f;

      minionCamp.AddComponent<MinionCampManager>();
    }

    MinionCampManager campManager = minionCamp.GetComponent<MinionCampManager>();
    campManager.SetLevel(2);
    if (campManager == null)
    {
      Debug.LogError("MinionCampManager component not found or could not be added.");
      yield break;
    }

    for (int i = 0; i < maxMinions; i++)
    {
      Vector3 randomPosition = GenerateRandomPosition(spawnRadius);

      if (Vector3.Distance(randomPosition, transform.position) < minimumDistance)
      {
        i--;
        continue;
      }

      GameObject minion = Instantiate(minionPrefab, randomPosition, Quaternion.identity);

      minion.SetActive(true);
      minion.transform.parent = minionCamp.transform;

      campManager.RefreshMinions();
    }
  }



  private Vector3 GenerateRandomPosition(float radius)
  {
    float angle = Random.Range(0, Mathf.PI * 2);
    float distance = Random.Range(0, radius);

    Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
    return transform.position + offset;
  }


  public void PlayDieAnimation()
  {
    animator.SetTrigger("DieTrigger");

  }


}

