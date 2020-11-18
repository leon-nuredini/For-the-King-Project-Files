using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Hellmade.Sound;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeVillage : MonoBehaviour
{
    [SerializeField] private Color _whiteColor, _disabledColor;

    [SerializeField] private Village _village;

    public GameObject cost;

    private SpriteRenderer _spriteRenderer;
    private TextMeshPro    _costText;

    private int _upgradeCost;

    private bool _canUpgrade;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _costText       = gameObject.GetComponentInChildren<TextMeshPro>(true);

        transform.localScale =
            new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        transform.DOScale(.7f, .5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutBounce);
    }

    public void UpdateButton()
    {
        if ((int) GameManager.Instance.factionTurn == (int) _village.villageState)
        {
            switch (GameManager.Instance.factionTurn)
            {
                case GameManager.FactionTurn.Good:
                    _upgradeCost = _village.upgradeCost;

                    if (GameManager.Instance.blueFactionGold >= _upgradeCost)
                    {
                        _canUpgrade           = true;
                        _spriteRenderer.color = _whiteColor;
                    }
                    else
                    {
                        _spriteRenderer.color = _disabledColor;
                        _canUpgrade           = false;
                    }

                    _costText.text = _upgradeCost.ToString();

                    break;
                case GameManager.FactionTurn.Evil:
                    _upgradeCost = _village.upgradeCost;

                    if (GameManager.Instance.darkFactionGold >= _upgradeCost)
                    {
                        _canUpgrade           = true;
                        _spriteRenderer.color = _whiteColor;
                    }
                    else
                    {
                        _spriteRenderer.color = _disabledColor;
                        _canUpgrade           = false;
                    }

                    _costText.text = _upgradeCost.ToString();

                    break;
            }
        }
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (_canUpgrade)
        {
            switch (GameManager.Instance.factionTurn)
            {
                case GameManager.FactionTurn.Good:
                    _upgradeCost = _village.upgradeCost;

                    if (GameManager.Instance.blueFactionGold >= _upgradeCost)
                    {
                        GameManager.Instance.blueFactionGold -= _upgradeCost;
                        _village.currentLevel++;
                        _village.UpdateStats();

                        EazySoundManager.PlaySound(AudioController.Instance.villageUpgradeSfx, .4f);
                        GameCanvas.Instance.UpdateBlueFactionGold(GameManager.Instance.blueFactionGold);
                        UpdateButton();
                    }

                    break;
                case GameManager.FactionTurn.Evil:
                    _upgradeCost = _village.upgradeCost;

                    if (GameManager.Instance.darkFactionGold >= _upgradeCost)
                    {
                        GameManager.Instance.darkFactionGold -= _upgradeCost;
                        _village.currentLevel++;
                        _village.UpdateStats();

                        EazySoundManager.PlaySound(AudioController.Instance.villageUpgradeSfx, .4f);
                        GameCanvas.Instance.UpdatedarkFactionGold(GameManager.Instance.darkFactionGold);
                        UpdateButton();
                    }

                    break;
            }
        }
    }
}