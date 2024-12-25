using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
  [Header("Audio Settings")]
  public AudioClip backgroundMusic;  
  private AudioSource audioSource;   
  void Start()
  {
    audioSource = GetComponent<AudioSource>();
    if (audioSource == null)
    {
      audioSource = gameObject.AddComponent<AudioSource>();
    }

    if (backgroundMusic != null)
    {
      audioSource.clip = backgroundMusic;
      audioSource.loop = true;  
      audioSource.playOnAwake = true; 
      audioSource.volume = 0.5f; 
      audioSource.Play();  
    }
  }
  public void ResumeGame()
  {
    GameObject gameManager = GameObject.FindObjectOfType<GameManager>().gameObject;
    if (gameManager != null)
    {
      gameManager.GetComponent<GameManager>().ResumeGame();
    }
  }

  public void RestartLevel()
  {
    Time.timeScale = 1f; 
    RestartManager.Instance.isRestarted = true; 
    SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
  }

  public void QuitToMainMenu()
  {
    Time.timeScale = 1f; 
    SceneManager.LoadScene("Main Menu"); 
  }
}
