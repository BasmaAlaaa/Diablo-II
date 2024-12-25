using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartManager : MonoBehaviour
{
  public static RestartManager Instance; 

  [Header("Cheat/Restart Flags")]
  public bool isRestarted = false; 
  public bool isCheatLoaded = false; 
  private void Awake()
  {

    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else
    {
      Destroy(gameObject);
    }
  }
}

