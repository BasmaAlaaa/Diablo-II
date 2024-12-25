using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{

  [Header("UI Buttons")]
  public Button sorcererButton;
  public Button barbarianButton;

  private static string selectedCharacterName;

  void Start()
  {
    sorcererButton.onClick.AddListener(() => SelectCharacter("Sorcerer"));
    barbarianButton.onClick.AddListener(() => SelectCharacter("Barbarian"));
  }

  void SelectCharacter(string characterName)
  {
    selectedCharacterName = characterName;

    SceneManager.LoadScene("LevelSelectorScene");
  }

  public static string GetSelectedCharacterName()
  {
    return selectedCharacterName;
  }
}
