using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Splash : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] private Button _startGameButton;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _startGameButton.gameObject.transform.DOScale(1.2f, .5f).SetEase(Ease.OutFlash).SetLoops(-1, LoopType.Yoyo);
    }

    public void Play()
    {
        DOTween.Kill(_startGameButton.gameObject.transform);
        
        _startGameButton.gameObject.transform.DOScale(1f, 0f);
        _startGameButton.interactable = false;
        _startGameButton.gameObject.SetActive(false);
        
        AnimatorParam.SetParams(_animator, AnimatorParams.Start, AnimatorParams.AnimParamType.Trigger);
    }
        
    public void LoadMenu() { SceneManager.LoadScene(1); }
    
}
