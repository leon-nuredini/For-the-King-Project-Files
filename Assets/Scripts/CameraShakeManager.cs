using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    private static CameraShakeManager _instance;
    public static  CameraShakeManager Instance => _instance;

    private Vector3 _initPosition;

    [BoxGroup("Attack")]
    [SerializeField] private float _attackDuration, _attackStrength, _attackRandomness;

    [BoxGroup("Attack")]
    [SerializeField] private int _attackVibratio;

    [BoxGroup("Attack")]
    [SerializeField] private bool _attackFadeOut;

    [BoxGroup("Take Damage")]
    [SerializeField] private float _takeDamageDuration, _takeDamageStrength, _takeDamageRandomness;

    [BoxGroup("Take Damage")]
    [SerializeField] private int _takeDamageVibratio;

    [BoxGroup("Take Damage")]
    [SerializeField] private bool _takeDamageFadeOut;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void Start() { _initPosition = ObjectHolder.Instance.mainCamera.transform.position; }

    public void ShakeCameraAttack()
    {
        ObjectHolder.Instance.mainCamera.transform.position = _initPosition;

        ObjectHolder.Instance.mainCamera.DOShakePosition(_attackDuration, _attackStrength, _attackVibratio,
                                                         _attackRandomness, _attackFadeOut);
    }

    public void ShakeCameraTakeDamage()
    {
        ObjectHolder.Instance.mainCamera.transform.position = _initPosition;

        ObjectHolder.Instance.mainCamera.DOShakePosition(_takeDamageDuration, _takeDamageStrength, _takeDamageVibratio,
                                                         _takeDamageRandomness, _takeDamageFadeOut);
    }
}