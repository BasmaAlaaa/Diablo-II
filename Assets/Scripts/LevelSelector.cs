using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
  public void SelectLevel(string levelName)
  {
    Debug.Log($"Loading Level: {levelName}");

    string selectedCharacter = CharacterSelector.GetSelectedCharacterName();
    Debug.Log($"Selected Character for Level: {selectedCharacter}");

    SceneManager.LoadScene(levelName); 
  }
}