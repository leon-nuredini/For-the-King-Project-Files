using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using DG.Tweening;

public class Menu : MonoBehaviour
{
    private static Menu _instance;
    public static  Menu Instance => _instance;

    [SerializeField] private GameObject      _btnPlay, _levelSelection, _nextButton, _previousButton, _logo;
    [SerializeField] private TextMeshProUGUI _titleText;

    [SerializeField] private string[]     _levelTitles;
    [SerializeField] private GameObject[] _levelPanels;

    private int selectedLevelIndex = 1;
    private int _maxLevelIndex;

    [DllImport("__Internal")]
    private static extern void RedirectTo();

    [DllImport("__Internal")]
    private static extern void StartGameEvent();

    [DllImport("__Internal")]
    private static extern void StartLevelEvent(int level);

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        Sequence mySequence = DOTween.Sequence();
        Sequence mySequence2 = DOTween.Sequence();

        _btnPlay.GetComponent<Button>().enabled = false;

        _btnPlay.GetComponent<TextMeshProUGUI>().DOFade(0f, 0f);

        mySequence.Append(_btnPlay.GetComponent<TextMeshProUGUI>().DOFade(1f, .5f).SetEase(Ease.Linear)).PrependInterval(1.5f).OnComplete(EnablePlayButton);

        _maxLevelIndex = ES3.Load<int>("Level", 1);
        _levelPanels[selectedLevelIndex - 1].SetActive(true);
        _titleText.text = _levelTitles[selectedLevelIndex - 1];

        UpdateButtons();
    }

    private void EnablePlayButton()
    {
        _btnPlay.GetComponent<Button>().enabled = true;
    }

    public void UpdateButtonsAfterUnlockAllLevels()
    {
        _maxLevelIndex = ES3.Load<int>("Level", 1);
        UpdateButtons();
    }

    public void DisplayLevelSelections()
    {
//        #if UNITY_WEBGL
//        StartGameEvent();
//        #endif
        
        _btnPlay.SetActive(false);
        _logo.SetActive(false);
        _levelSelection.SetActive(true);
    }

    public void SelectLevel()
    {
//        #if UNITY_WEBGL
//        StartLevelEvent(selectedLevelIndex);
//        #endif
        
        LevelController.Instance.LoadLevel(selectedLevelIndex + 1);
    }

    public void NextLevel()
    {
        if (selectedLevelIndex < 10) selectedLevelIndex++;

        DisableLevelPanels();

        _levelPanels[selectedLevelIndex - 1].SetActive(true);
        _titleText.text = _levelTitles[selectedLevelIndex - 1];

        UpdateButtons();
    }

    public void PreviousLevel()
    {
        if (selectedLevelIndex > 1) selectedLevelIndex--;

        DisableLevelPanels();

        _levelPanels[selectedLevelIndex - 1].SetActive(true);
        _titleText.text = _levelTitles[selectedLevelIndex - 1];

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        if (selectedLevelIndex <= _maxLevelIndex)
        {
            _nextButton.GetComponent<Button>().interactable     = true;
            _previousButton.GetComponent<Button>().interactable = true;
        }

        if (selectedLevelIndex >= _maxLevelIndex) _nextButton.GetComponent<Button>().interactable = false;

        if (selectedLevelIndex <= 1) _previousButton.GetComponent<Button>().interactable = false;
    }

    private void DisableLevelPanels()
    {
        foreach (GameObject obj in _levelPanels) { obj.SetActive(false); }
    }
}