using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockObject : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.Play(0, -1, Random.value);
    }
}