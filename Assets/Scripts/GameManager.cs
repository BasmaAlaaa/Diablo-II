using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
  private string pauseSceneName = "Pause"; 

  void Update()
  {
    // Check for Escape key press to pause the game
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      PauseGame();
    }
  }

  public void PauseGame()
  {
    Time.timeScale = 0f; 
    SceneManager.LoadScene(pauseSceneName, LoadSceneMode.Additive); 
    Debug.Log("Game Paused");
  }

  public void ResumeGame()
  {
    Time.timeScale = 1f; 
    SceneManager.UnloadSceneAsync(pauseSceneName); 
    Debug.Log("Game Resumed");
  }

  public void RestartLevel()
  {
    Time.timeScale = 1f; 
    RestartManager.Instance.isRestarted = true; 
    SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    Debug.Log("Level Restarted");
  }

  public void QuitToMainMenu()
  {
    Time.timeScale = 1f; 
    SceneManager.LoadScene("Main Menu"); 
    Debug.Log("Quit to Main Menu");
  }
}
