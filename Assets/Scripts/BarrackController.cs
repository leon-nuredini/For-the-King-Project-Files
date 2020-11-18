using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Hellmade.Sound;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class BarrackController : MonoBehaviour
{
    private static BarrackController instance;
    public static  BarrackController Instance => instance;

    private Button        _blueFactionToggleBarrackButton, _darkFactionToggleBarrackButton;
    private GameObject    _blueFactionBarrack,             _darkFactionBarrack;
    private RectTransform _tempBarrackRectTransform;

    private Village _village;

    [BoxGroup("Unit Buttons")]
    public Button btnBlueArcher, btnBlueScout;

    [BoxGroup("Properties")]
    public bool isBarracksEnabled, isArcherAllowed, isScoutAllowed;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateToggleButtons();

        if (isBarracksEnabled == false)
        {
            _blueFactionToggleBarrackButton.gameObject.SetActive(false);
            _darkFactionToggleBarrackButton.gameObject.SetActive(false);
        }
        else
            UpdateBuyButtons();
    }


    private void UpdateBuyButtons()
    {
        if (isArcherAllowed)
            btnBlueArcher.gameObject.SetActive(true);
        else
            btnBlueArcher.gameObject.SetActive(false);

        if (isScoutAllowed)
            btnBlueScout.gameObject.SetActive(true);
        else
            btnBlueScout.gameObject.SetActive(false);
    }

    public void SetBarracks(Button blueToggle, Button darkToggle, GameObject blueBarrack, GameObject darkBarrack)
    {
        _blueFactionToggleBarrackButton = blueToggle;
        _darkFactionToggleBarrackButton = darkToggle;
        _blueFactionBarrack             = blueBarrack;
        _darkFactionBarrack             = darkBarrack;
    }

    public void UpdateToggleButtons()
    {
        switch (GameManager.Instance.factionTurn)
        {
            case GameManager.FactionTurn.Good:
                _darkFactionToggleBarrackButton.interactable = false;
                _blueFactionToggleBarrackButton.interactable = true;
                break;
            case GameManager.FactionTurn.Evil:
                _blueFactionToggleBarrackButton.interactable = false;

                if (GameManager.Instance.isSinglePlayer == false) _darkFactionToggleBarrackButton.interactable = true;
                break;
        }
    }

    public void BuyUnit(BarrackItem item)
    {
        if (GameManager.Instance.factionTurn == GameManager.FactionTurn.Good &&
            item.cost                        <= GameManager.Instance.blueFactionGold)
        {
            //GameManager.Instance.blueFactionGold -= item.cost;
            _blueFactionBarrack.SetActive(false);
        }
        else if (GameManager.Instance.factionTurn == GameManager.FactionTurn.Evil &&
                 item.cost                        <= GameManager.Instance.darkFactionGold)
        {
            //GameManager.Instance.darkFactionGold -= item.cost;
            _darkFactionBarrack.SetActive(false);
        }
        else { return; }

        GameManager.Instance.purchasedItem = item;

        if (GameManager.Instance.selectedUnit != null)
        {
            if (GameManager.Instance.selectedUnit.unitType == Unit.UnitType.Settlement)
            {
                GameManager.Instance.selectedUnit.GetVillage().DisableUpgradeButton();
                GameManager.Instance.selectedUnit.GetVillage().EnableHealthDisplay();
            }

            GameManager.Instance.selectedUnit.isSelected = false;
            GameManager.Instance.selectedUnit            = null;
        }

        GetPlaceableTiles();
    }

    public void ToggleBarrack(GameObject barrack)
    {
        EazySoundManager.PlaySound(AudioController.Instance.toggleBarracksSfx, .2f);

        _tempBarrackRectTransform = barrack.GetComponent<RectTransform>();

        _tempBarrackRectTransform.localScale = new Vector3(.8f, .8f, 1f);
        barrack.SetActive(!barrack.activeSelf);
        _tempBarrackRectTransform.DOScale(1f, .25f).SetEase(Ease.OutBounce);
    }

    public void CloseBarracks()
    {
        _blueFactionBarrack.SetActive(false);
        _darkFactionBarrack.SetActive(false);
    }

    private void GetPlaceableTiles()
    {
        GameManager.Instance.ResetTiles();
        
        foreach (Tile tile in ObjectHolder.Instance.tiles)
            if (tile.IsClear() && tile.GetVillage() != null)
            {
                if ((int) GameManager.Instance.factionTurn == (int) tile.GetVillage().villageState) tile.SetPlaceable();
            }
    }
}