using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsManager : MonoBehaviour
{
  [Header("Panels")]
  public GameObject mainMenuPanel;
  public GameObject optionsPanel;
  public GameObject teamCreditsPanel;
  public GameObject assetCreditsPanel;

  [Header("Audio Settings")]
  public Slider musicSlider;
  public Slider effectsSlider;

  // public AudioSource musicSource;
  // public AudioSource effectsSource;

  void Start()
  {
    // Initialize sliders with saved values or defaults
    float savedMusicLevel = PlayerPrefs.GetFloat("MusicLevel", 0.5f);
    float savedEffectsLevel = PlayerPrefs.GetFloat("EffectsLevel", 0.5f);

    musicSlider.value = savedMusicLevel;
    effectsSlider.value = savedEffectsLevel;

    ApplyAudioSettings();
    mainMenuPanel.SetActive(true);
  }

  public void OpenOptions()
  {
    mainMenuPanel.SetActive(false);
    optionsPanel.SetActive(true);
  }

  public void CloseOptions()
  {
    optionsPanel.SetActive(false);
    mainMenuPanel.SetActive(true);
  }

  public void OpenTeamCredits()
  {
    optionsPanel.SetActive(false);
    teamCreditsPanel.SetActive(true);
  }

  public void OpenAssetCredits()
  {
    optionsPanel.SetActive(false);
    assetCreditsPanel.SetActive(true);
  }

  public void CloseTeamCredits()
  {
    teamCreditsPanel.SetActive(false);
    optionsPanel.SetActive(true);
  }

  public void CloseAssetCredits()
  {
    assetCreditsPanel.SetActive(false);
    optionsPanel.SetActive(true);
  }

  public void ApplyAudioSettings()
  {
    float musicLevel = musicSlider.value;
    float effectsLevel = effectsSlider.value;

    // Adjust audio sources
    // if (musicSource != null)
    //   musicSource.volume = musicLevel;

    // if (effectsSource != null)
    //   effectsSource.volume = effectsLevel;

    // Save settings
    PlayerPrefs.SetFloat("MusicLevel", musicLevel);
    PlayerPrefs.SetFloat("EffectsLevel", effectsLevel);
    PlayerPrefs.Save();
  }
}
