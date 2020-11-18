using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Butterfly : MonoBehaviour
{
    [SerializeField]
    private float _minX = -8f, _maxX = 8f, _minY = -4.5f, _maxY = 4.5f, _minDuration = 5f, _maxDuration = 10f;

    private float _randomX, _randomY, _randomDuration;

    private void Start() { MoveRandomly(); }

    private void MoveRandomly()
    {
        _randomX        = Random.Range(_minX,        _maxX);
        _randomY        = Random.Range(_minY,        _maxY);
        _randomDuration = Random.Range(_minDuration, _maxDuration);

        transform.DOMove(new Vector3(_randomX, _randomY, transform.position.z), _randomDuration).SetEase(Ease.Linear)
                 .OnComplete(MoveRandomly);
    }
}