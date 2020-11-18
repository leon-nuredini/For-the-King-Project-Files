using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageAnimatorEvents : MonoBehaviour
{
    [SerializeField] private ParticleSystem _darkSmokeVfx;

    private Animator _animator;
    private Village  _village;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _village  = GetComponent<Village>();

        if (_village.villageState != Village.VillageState.None)
        {
            _darkSmokeVfx.transform.parent     = null;
            _darkSmokeVfx.transform.localScale = Vector3.one;
        }

        _animator.Play(0, -1, Random.value);
    }

    public void DarkVillagePlaySmokeVfx()
    {
        if (_village.villageState != Village.VillageState.None) _darkSmokeVfx.Play();
    }
}