using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SelectionSquare : MonoBehaviour
{
    [SerializeField] private float _scaleValue = .25f, _duration = .5f;

    private void Start()
    {
        GameManager.Instance.selectedUnitSquare = gameObject;

        transform.DOScale(_scaleValue, _duration).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }
}