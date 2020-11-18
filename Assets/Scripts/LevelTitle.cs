using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTitle : MonoBehaviour
{
    [SerializeField] private string[] _titles;
    private TextMeshPro _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshPro>();
        _text.text = _titles[SceneManager.GetActiveScene().buildIndex - 2];

        _text.DOScale(.5f, 0f);
        _text.DOScale(1f, 1f).SetEase(Ease.OutBounce);
        
        Invoke(nameof(SizeDown), 2f);
    }

    private void SizeDown()
    {
        _text.DOScale(0f, 1f).SetEase(Ease.InBack).OnComplete(DisableText);
    }

    private void DisableText()
    {
        gameObject.SetActive(false);
    }
}
