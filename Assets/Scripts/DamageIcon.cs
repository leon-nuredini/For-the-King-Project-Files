using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DamageIcon : MonoBehaviour
{
    [SerializeField] private float       _maxScale = .35f, _duration = .5f;
    [SerializeField] private TextMeshPro _damageText;
    [SerializeField] private bool        _isEnemyDamageIcon;

    private SpriteRenderer _spriteRenderer;

    public void Setup(int damage)
    {
        Invoke(nameof(ShowDamageIcon), .5f);
        _damageText.text = damage.ToString();
    }

    private void ShowDamageIcon()
    {
        transform.DOScale(_maxScale, _duration).SetEase(Ease.OutElastic).OnComplete(DisableSpriteRenderer);
    }

    public void DisableSpriteRenderer()
    {
        transform.DOScale(0f, _duration / 2f).SetEase(Ease.InBack).OnComplete(InitializeDisable);
    }

    private void InitializeDisable()
    {
        Destroy(gameObject);
    }
}