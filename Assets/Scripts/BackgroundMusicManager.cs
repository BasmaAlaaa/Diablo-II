using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusicManager : MonoBehaviour
{
    private static BackgroundMusicManager instance;
    private AudioSource audioSource;

    [Header("Music Tracks")]
    public AudioClip sceneL1Music;
    public AudioClip sceneL2Music;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
        }
        audioSource.volume = 0.4f;
    }

    void Start()
    {
        PlayMusicForCurrentScene();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }

    private void PlayMusicForCurrentScene()
    {
        if (audioSource == null)
        {
            Debug.LogError("No AudioSource found on BackgroundMusicManager!");
            return;
        }

        AudioClip newClip = null;

        if (SceneManager.GetActiveScene().name == "Scene_L1")
        {
            newClip = sceneL1Music;
        }
        else if (SceneManager.GetActiveScene().name == "Scene_L2")
        {
            newClip = sceneL2Music;
        }

        if (newClip != null && audioSource.clip != newClip)
        {
            audioSource.clip = newClip;
            audioSource.Play();
        }
    }

    public void PlayMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }
}
