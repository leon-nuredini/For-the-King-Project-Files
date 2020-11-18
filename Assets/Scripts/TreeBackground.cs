using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBackground : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    [SerializeField] private Sprite[] _backgroundTrees;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
            
        if (_backgroundTrees.Length > 0)
            _spriteRenderer.sprite = _backgroundTrees[Random.Range(0, _backgroundTrees.Length)];
    }
}