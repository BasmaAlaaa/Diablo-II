using UnityEngine;

public class CharacterManager : MonoBehaviour
{
  [Header("Characters")]
  public GameObject sorcerer;
  public GameObject barbarian;
  private GameObject activePlayer;

  void Start()
  {
    string selectedCharacterName = CharacterSelector.GetSelectedCharacterName();

    if (selectedCharacterName == sorcerer.name)
    {
      sorcerer.SetActive(true);
      barbarian.SetActive(false);
      activePlayer = sorcerer;
    }
    else if (selectedCharacterName == barbarian.name)
    {
      sorcerer.SetActive(false);
      barbarian.SetActive(true);
      activePlayer = barbarian;
    }
    else
    {
      sorcerer.SetActive(false);
      barbarian.SetActive(true);
      activePlayer = barbarian;
      Debug.LogWarning("No valid character selected! Defaulting to Sorcerer.");

    }

    CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
    if (cameraFollow != null)
    {
      cameraFollow.SetPlayer(activePlayer.transform);
    }
    else
    {
      Debug.LogError("CameraFollow script not found in the scene!");
    }
  }

  public GameObject GetActivePlayer()
  {
    if (sorcerer.activeSelf)
    {
      return sorcerer;
    }
    else if (barbarian.activeSelf)
    {
      return barbarian;
    }
    else
    {
      return sorcerer;
    }
  }


}
