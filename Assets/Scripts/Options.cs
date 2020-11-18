using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    private static Options _instance;
    public static  Options Instance => _instance;

    [SerializeField] private Sprite _soundOn, _soundOff;
    [SerializeField] private Image  _volumeIcon;

    private GameObject _tutorial;

    private float volume = 1f;
    
    [DllImport("__Internal")]
    private static extern void ReplayEvent();

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (ES3.Load<float>("Volume", 1f) > 0f)
        {
            _volumeIcon.sprite = _soundOn;
            volume = 1f;
        }
        else
        {
            _volumeIcon.sprite = _soundOff;
            volume = 0f;
        }
        
        AudioListener.volume = volume;

        if (Tutorial.Instance != null)
            _tutorial = Tutorial.Instance.gameObject;
    }

    public void RestartLevel()
    {
//        #if UNITY_WEBGL
//        ReplayEvent();
//        #endif
        
        LevelController.Instance.RestartLevel();
    }

    public void ReturnToMainMenu() { LevelController.Instance.LoadLevel(1); }

    public void ToggleVolume()
    {
        if (volume > 0f)
        {
            _volumeIcon.sprite = _soundOff;
            ES3.Save<float>("Volume", 0f);
        }
        else
        {
            _volumeIcon.sprite = _soundOn;
            ES3.Save<float>("Volume", 1f);
        }

        volume = ES3.Load<float>("Volume");

        AudioListener.volume = volume;
    }

    public void ToggleTutorial()
    {
        if (_tutorial != null)
            _tutorial.SetActive(!_tutorial.activeSelf);
    }
}