using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
  [Header("Panels")]
  public GameObject optionsPanel;
  public GameObject creditsPanel;
  public GameObject assetCreditsPanel;
  [Header("Audio Settings")]
  public AudioClip backgroundMusic;
  private AudioSource audioSource;

  void Start()
  {
    optionsPanel.SetActive(false);
    creditsPanel.SetActive(false);
    assetCreditsPanel.SetActive(false);
    SetupBackgroundMusic();
  }

  void SetupBackgroundMusic()
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
    else
    {
      Debug.LogWarning("No background music assigned to MainMenu.");
    }
  }

  public void ShowOptionsMenu()
  {
    optionsPanel.SetActive(true);
  }

  public void HideOptionsMenu()
  {
    optionsPanel.SetActive(false);
  }

  public void ShowTeamCredits()
  {
    creditsPanel.SetActive(true);
  }

  public void HideTeamCredits()
  {
    creditsPanel.SetActive(false);
  }

  public void ShowAssetCredits()
  {
    assetCreditsPanel.SetActive(true);
  }

  public void HideAssetCredits()
  {
    assetCreditsPanel.SetActive(false);
  }

  public void QuitGame()
  {
    Debug.Log("Quitting Game...");
    Application.Quit();
  }

  public void StartNewGame()
  {
    Debug.Log("Starting New Game...");
    SceneManager.LoadScene("CharacterSelection");
  }

}
