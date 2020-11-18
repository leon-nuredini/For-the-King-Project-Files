using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    private static Tutorial _instance;
    public static  Tutorial Instance => _instance;

    [SerializeField] private int      _steps;
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private string[] _texts;

    [SerializeField] private Image           _image;
    [SerializeField] private TextMeshProUGUI _text, _btnText;

    [SerializeField] private Button _btnBack;

    [SerializeField] private bool _startClosed;

    private int currentStep = -1;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        _steps = _sprites.Length;
        Next();

        if (_startClosed) gameObject.SetActive(false);

        _btnText.DOFontSize(26f, .5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        _btnBack.GetComponent<TextMeshProUGUI>().DOFontSize(26f, .5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDisable()
    {
        currentStep   = -1;
        _btnText.text = ">>";
        Next();
    }

    public void Next()
    {
        if (currentStep == _steps - 1)
        {
            gameObject.SetActive(false);
            return;
        }

        currentStep++;

        _image.sprite = _sprites[currentStep];
        _text.text    = _texts[currentStep];

        if (currentStep == _steps - 1) _btnText.text = "x";

        if (currentStep > 0) { _btnBack.gameObject.SetActive(true); }
        else { _btnBack.gameObject.SetActive(false); }
    }

    public void Back()
    {
        if (currentStep == 0)
        {
            _btnBack.gameObject.SetActive(false);
            return;
        }

        if (currentStep != _steps) _btnText.text = ">>";

        currentStep--;

        _image.sprite = _sprites[currentStep];
        _text.text    = _texts[currentStep];

        if (currentStep == 0) { _btnBack.gameObject.SetActive(false); }
    }
}