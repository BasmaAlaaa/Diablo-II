using UnityEngine;

public class RuneFragment : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject runeFragment;

    [Header("Audio Settings")]
    public AudioClip pickupSound; 
    private AudioSource audioSource;

    private void Start()
    {
        if (runeFragment != null)
        {
            runeFragment.SetActive(true);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player picked up the Rune Fragment!");
            CollectRune();
        }
    }

    private void CollectRune()
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        RuneCollectionManager runeManager = FindObjectOfType<RuneCollectionManager>();
        if (runeManager != null)
        {
            runeManager.AddRune();
        }

        Destroy(gameObject);
    }
}
