using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using Hellmade.Sound;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameCanvas : MonoBehaviour
{
    private static GameCanvas instance;
    public static  GameCanvas Instance => instance;

    [BoxGroup("Faction Turn Icon")]
    [SerializeField] private Image _factionTurnIcon;

    [BoxGroup("Faction Turn Icon")]
    [SerializeField] private TextMeshProUGUI _factionTurnText;

    [BoxGroup("King's Health'")]
    [SerializeField] private TextMeshProUGUI _blueKingHealth, _darkKingHealth;

    [BoxGroup("Faction Banners")]
    [SerializeField] private Image _blueBannerImage, _darkBannerImage;

    [BoxGroup("Gold Text")]
    [SerializeField] private TextMeshProUGUI _blueFactionGoldText, _darkFactionGoldText;

    [BoxGroup("Barracks")]
    [SerializeField] private Button _blueFactionToggleBarrackButton, _darkFactionToggleBarrackButton;

    [BoxGroup("Barracks")]
    [SerializeField] private GameObject _blueFactionBarrack, _darkFactionBarrack;

    [BoxGroup("Blue Barracks Buttons")]
    [SerializeField] private Button _blueSoldierButton, _blueArcherButton, _blueScoutButton;

    [BoxGroup("Blue Barracks Buttons")]
    [SerializeField] private TextMeshProUGUI _blueSoldierPriceTmp, _blueArcherPriceTmp, _blueScoutTmp;

    [BoxGroup("Dark Barracks Buttons")]
    [SerializeField] private Button _darkSoldierButton, _darkArcherButton, _darkScoutButton;

    [BoxGroup("Dark Barracks Buttons")]
    [SerializeField] private TextMeshProUGUI _darkSoldierPriceTmp, _darkArcherPriceTmp, _darkScoutTmp;

    [BoxGroup("Win/Loose flag")]
    [SerializeField] private Image _bgImage;

    [BoxGroup("Win/Loose flag")]
    [SerializeField] private GameObject _blueFactionWin, _darkFactionWin;

    [BoxGroup("End Turn Button")]
    [SerializeField] private Button _endTurnButton;

    [BoxGroup("End Turn Button")]
    [SerializeField] private GameObject _endTurnButtonGlow;

    [BoxGroup("End Turn Button")]
    [SerializeField] private ParticleSystem _vfxEndTurnButton;

    public bool _doIdleUnitsExist;

    [DllImport("__Internal")]
    private static extern void ReplayEvent();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        GameManager.Instance.SetFactionTurnIconAndText(_factionTurnIcon, _factionTurnText);
        BarrackController.Instance.SetBarracks(_blueFactionToggleBarrackButton, _darkFactionToggleBarrackButton,
                                               _blueFactionBarrack, _darkFactionBarrack);

        _blueSoldierPriceTmp.text = GameManager.Instance.blueSoldierPrice.ToString();
        _blueArcherPriceTmp.text  = GameManager.Instance.blueArcherPrice.ToString();
        _blueScoutTmp.text        = GameManager.Instance.blueScoutPrice.ToString();

        _darkSoldierPriceTmp.text = GameManager.Instance.darkSoldierPrice.ToString();
        _darkArcherPriceTmp.text  = GameManager.Instance.darkArcherPrice.ToString();
        _darkScoutTmp.text        = GameManager.Instance.darkScoutPrice.ToString();

        if (GameManager.Instance.isSinglePlayer)
        {
            _darkFactionToggleBarrackButton.gameObject.SetActive(false);
            _darkFactionGoldText.gameObject.transform.parent.gameObject.SetActive(false);
        }

        BarrackController.Instance.btnBlueArcher = _blueArcherButton;
        BarrackController.Instance.btnBlueScout = _blueScoutButton;
    }

    public void RestoreFocusToEndTurnButton()
    {
        if (GameManager.Instance.factionTurn == GameManager.FactionTurn.Good && _doIdleUnitsExist == false)
        {
            foreach (Unit unit in ObjectHolder.Instance.units)
            {
                if (unit.faction == Unit.Faction.Good)
                {
                    if (unit.unitType != Unit.UnitType.Settlement)
                    {
                        if (unit.hasMoved == false) return;

                        if (unit.hasAttacked == false && unit.CheckForTargetableEnemies().Count > 0) return;
                    }
                }
            }

            _doIdleUnitsExist = true;
            _endTurnButton.transform.DOScale(1.2f, .5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutBack);
            _endTurnButtonGlow.transform.DOScale(1.6f, .5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutBack);
            _vfxEndTurnButton.Play();
        }
        else if (_doIdleUnitsExist)
        {
            _doIdleUnitsExist = false;
            
            DOTween.Kill(_endTurnButton.transform);
            DOTween.Kill(_endTurnButtonGlow.transform);
            
            _endTurnButton.transform.DOScale(1f, .2f);
            _endTurnButtonGlow.transform.DOScale(1f, .2f);
            _vfxEndTurnButton.Stop();
        }
    }

    public void UpdateBlueKingHealth(int health)
    {
        if (health < 0)
            _blueKingHealth.text = "0";
        else
            _blueKingHealth.text = health.ToString();
    }

    public void UpdateDarkKingHealth(int health)
    {
        if (health < 0)
            _darkKingHealth.text = "0";
        else
            _darkKingHealth.text = health.ToString();
    }

    public void UpdateBlueFactionGold(int goldAmount) { _blueFactionGoldText.text = goldAmount.ToString(); }

    public void UpdatedarkFactionGold(int goldAmount) { _darkFactionGoldText.text = goldAmount.ToString(); }

    public void UpdateBanner()
    {
        switch (GameManager.Instance.factionTurn)
        {
            case GameManager.FactionTurn.Good:
                _blueBannerImage.enabled = true;
                _darkBannerImage.enabled = false;
                break;
            case GameManager.FactionTurn.Evil:
                _blueBannerImage.enabled = false;
                _darkBannerImage.enabled = true;
                break;
        }
    }

    public void ShowBlueWinPanel()
    {
        if (GameManager.Instance.isGameEnded) return;

        _bgImage.enabled = true;
        _blueFactionWin.SetActive(true);
        
        _endTurnButton.interactable = false;
        _endTurnButtonGlow.SetActive(false);
    }

    public void ShowDarkWinPanel()
    {
        if (GameManager.Instance.isGameEnded) return;

        _bgImage.enabled = true;
        _darkFactionWin.SetActive(true);
        
        _endTurnButton.interactable = false;
        _endTurnButtonGlow.SetActive(false);
    }

    public void ToggleBarrack(GameObject barrack)
    {
        BarrackController.Instance.ToggleBarrack(barrack);

        if (GameManager.Instance.blueFactionGold >= GameManager.Instance.blueSoldierPrice)
            _blueSoldierButton.interactable = true;
        else
            _blueSoldierButton.interactable = false;

        if (GameManager.Instance.blueFactionGold >= GameManager.Instance.blueArcherPrice)
            _blueArcherButton.interactable = true;
        else
            _blueArcherButton.interactable = false;

        if (GameManager.Instance.blueFactionGold >= GameManager.Instance.blueScoutPrice)
            _blueScoutButton.interactable = true;
        else
            _blueScoutButton.interactable = false;

        if (GameManager.Instance.darkFactionGold >= GameManager.Instance.darkSoldierPrice)
            _darkSoldierButton.interactable = true;
        else
            _darkSoldierButton.interactable = false;

        if (GameManager.Instance.darkFactionGold >= GameManager.Instance.darkArcherPrice)
            _darkArcherButton.interactable = true;
        else
            _darkArcherButton.interactable = false;

        if (GameManager.Instance.darkFactionGold >= GameManager.Instance.darkScoutPrice)
            _darkScoutButton.interactable = true;
        else
            _darkScoutButton.interactable = false;
    }

    public void BuyUnit(BarrackItem item) { BarrackController.Instance.BuyUnit(item); }

    public void EndTurn()
    {
        EazySoundManager.PlaySound(AudioController.Instance.endTurnSfx);
        GameManager.Instance.EndTurn();
    }

    public void UpdateEndTurnButton()
    {
        switch (GameManager.Instance.factionTurn)
        {
            case GameManager.FactionTurn.Good:
                _endTurnButton.interactable = true;
                _endTurnButtonGlow.SetActive(true);
                break;
            case GameManager.FactionTurn.Evil:
                _endTurnButton.interactable = false;
                _endTurnButtonGlow.SetActive(false);
                break;
        }
    }

    public void LoadNextLevel()
    {
        LevelController.Instance.LoadNextLevel();
    }

    public void RestartLevel()
    {
//        #if UNITY_WEBGL
//        ReplayEvent();
//        #endif
        
        LevelController.Instance.RestartLevel();
    }

    public void ReturnToMainMenu()
    {
        LevelController.Instance.LoadLevel(1);
    }

    public void ToggleVolume()
    {
        
    }
}