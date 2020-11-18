using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    private static LevelController instance;
    public static  LevelController Instance => instance;
    
    [DllImport("__Internal")]
    private static extern void StartLevelEvent(int level);

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void LoadLevel(string levelName) { SceneManager.LoadScene(levelName); }

    public void LoadLevel(int levelIndex) { SceneManager.LoadScene(levelIndex); }

    public void LoadNextLevel()
    {
        if (SceneManager.GetActiveScene().buildIndex + 1 > SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(0);
        else
        {
//            #if UNITY_WEBGL
//            StartLevelEvent(SceneManager.GetActiveScene().buildIndex);
//            #endif
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    public void RestartLevel() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
}