using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverEffect : MonoBehaviour
{
    [SerializeField] private float _hoverAmount;

    private float _originalScale;

    private Unit _thisUnit;

    private float _direction;

    private void Awake()
    {
        _thisUnit = GetComponent<Unit>();

        _originalScale = transform.localScale.x;
    }

    public void Reset()
    {
        _direction = Mathf.Sign(transform.localScale.x);

        transform.DOScale(new Vector2(_direction * _originalScale, _originalScale), 0f);
    }

    private void OnMouseOver()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        if (_thisUnit.hasMoved == false && (int) _thisUnit.faction == (int) GameManager.Instance.factionTurn)
        {
            _direction = Mathf.Sign(transform.localScale.x);

            transform.DOScale(new Vector2(_direction * _hoverAmount, _hoverAmount), 0f);
        }
    }

    private void OnMouseExit()
    {
        if (_thisUnit.hasMoved == false && (int) _thisUnit.faction == (int) GameManager.Instance.factionTurn)
        {
            _direction = Mathf.Sign(transform.localScale.x);

            transform.DOScale(new Vector2(_direction * _originalScale, _originalScale), 0f);
        }
    }
}