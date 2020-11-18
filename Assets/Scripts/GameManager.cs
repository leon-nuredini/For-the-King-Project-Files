using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Hellmade.Sound;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static  GameManager Instance => _instance;

    public Unit        selectedUnit;
    public GameObject  selectedUnitSquare;
    public BarrackItem purchasedItem;

    [BoxGroup("UI")]
    public Image factionTurnIcon;

    [BoxGroup("UI")]
    [SerializeField] private Sprite _goodFactionIcon, _darkFactionIcon;

    [BoxGroup("UI")]
    public TextMeshProUGUI factionTurnText;

    [BoxGroup("UI")]
    public Color _goodFactionColor, _darkFactionColor;

    [BoxGroup("Faction Wealth")]
    public int blueFactionGold, darkFactionGold;

    [BoxGroup("Unit Prices")]
    public int blueSoldierPrice = 15,
               blueArcherPrice  = 20,
               blueScoutPrice   = 25,
               darkSoldierPrice = 10,
               darkArcherPrice  = 15,
               darkScoutPrice   = 20;

    [BoxGroup("Stats Panel")]
    public Vector2 statsPanelShift;

    [BoxGroup("Game End")]
    public bool isGameEnded, isDarkKingDead;

    private RectTransform _factionTurnIconRectTransform;
    private GameObject _darkAIUnit;
    private List<Unit> _evilUnits = new List<Unit>();
    [SerializeField] private List<Tile> _freeTiles = new List<Tile>();
    private AnimationClip[] clips;

    public bool isSinglePlayer;

    public int currentEvilUnitIndex = 0, randomTileIndex;
    private int maxSpawnableUnits;
    private float attackAnimationDuration;
    
    public enum FactionTurn
    {
        Good,
        Evil
    }

    public FactionTurn factionTurn = FactionTurn.Good;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        GameCanvas.Instance.UpdateBlueFactionGold(blueFactionGold);
        GameCanvas.Instance.UpdatedarkFactionGold(darkFactionGold);
//        GameCanvas.Instance.UpdateBanner();

        if (ES3.Load<int>("Level", 1) < SceneManager.GetActiveScene().buildIndex - 1)
            ES3.Save<int>("Level", SceneManager.GetActiveScene().buildIndex - 1);
    }

    private void Update()
    {
        if (isGameEnded) return;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space)) EndTurn();
#endif

        if (selectedUnit != null)
        {
            if (selectedUnitSquare.activeSelf == false) selectedUnitSquare.SetActive(true);

            selectedUnitSquare.transform.position = selectedUnit.transform.position;
        }
        else
        {
            if (selectedUnitSquare.activeSelf) selectedUnitSquare.SetActive(false);
        }
    }

    public void EndTurn()
    {
        if (isGameEnded) return;

        switch (factionTurn)
        {
            case FactionTurn.Good:
                factionTurn            = FactionTurn.Evil;
                factionTurnIcon.sprite = _darkFactionIcon;
                factionTurnText.text   = "Dark Faction";
                factionTurnText.color  = _darkFactionColor;
                break;
            case FactionTurn.Evil:
                factionTurn            = FactionTurn.Good;
                factionTurnIcon.sprite = _goodFactionIcon;
                factionTurnText.text   = "Light Faction";
                factionTurnText.color  = _goodFactionColor;
                break;
        }

        UpdateFactionGold();

        _factionTurnIconRectTransform.localScale = new Vector3(.8f, .8f, 1f);
        _factionTurnIconRectTransform.DOScale(1f, .5f).SetEase(Ease.OutBounce);

        if (selectedUnit != null)
        {
            if (selectedUnit.unitType == Unit.UnitType.Settlement)
            {
                selectedUnit.GetVillage().EnableHealthDisplay();
                selectedUnit.GetVillage().DisableUpgradeButton();
            }

            selectedUnit.isSelected = false;
            selectedUnit            = null;
        }

        ResetTiles();

//        ObjectHolder.Instance.damageIconCurrentPlayer.DisableSpriteRenderer();
//        ObjectHolder.Instance.damageIconEnemy.DisableSpriteRenderer();

        _evilUnits.Clear();
        currentEvilUnitIndex = 0;
        
        foreach (Unit unit in ObjectHolder.Instance.units)
        {
            if (unit.faction == Unit.Faction.Evil && unit.unitType != Unit.UnitType.Settlement)
            {
                _evilUnits.Add(unit);
                unit.isAiFinished = false;
                unit.GetAIPathCheck().CheckForWalkablePath();
                unit.movementActionPoints = unit.initActionPoints;
            }
            
            if (unit.unitType != Unit.UnitType.Settlement)
            {
                unit.hasMoved    = false;
                unit.hasAttacked = false;
                unit.ResetDefenseDamage();

                if (unit.faction == Unit.Faction.Good && unit.unitType == Unit.UnitType.Spirit) unit.hasHealed = false;

                unit.GetComponent<HoverEffect>().Reset();
                unit.ResetIcons();
            }
            else
            {
                switch (factionTurn)
                {
                    case FactionTurn.Good:
                        if (unit.faction == Unit.Faction.Good) unit.GetVillage().DisplayGoldGainedText();
                        break;
                    case FactionTurn.Evil:
                        if (isSinglePlayer == false)
                            if (unit.faction == Unit.Faction.Evil) unit.GetVillage().DisplayGoldGainedText();
                        break;
                }
            }

            if (unit.unitReach != null) unit.unitReach.GetNearbyReachableTiles();
        }

        BarrackController.Instance.CloseBarracks();
        BarrackController.Instance.UpdateToggleButtons();
        GameCanvas.Instance.UpdateEndTurnButton();
//        GameCanvas.Instance.UpdateBanner();

        if (isSinglePlayer && isDarkKingDead == false)
        {
            if (DarkVillage() == 0)
            {
                foreach (Unit unit in ObjectHolder.Instance.units)
                {
                    if (unit.faction == Unit.Faction.Evil && unit.unitType == Unit.UnitType.King &&
                        unit.GetIsStationary() && unit.isTutorial == false)
                    {
                        unit.SetIsStationary(false);
                    }
                }
            }
            
            StopAllCoroutines();
            StartCoroutine(UpdateAi());
        }
    }

    public void ResumeAiCoroutine(float duration)
    {
        if (isDarkKingDead) return;
        
        Invoke(nameof(ResumeAi), duration);
    }

    public void ResumeAi()
    {
        if (isDarkKingDead) return;
        
        StartCoroutine(UpdateAi());
    }

    private IEnumerator UpdateAi()
    {
        if (isDarkKingDead) yield break;
        
        if (currentEvilUnitIndex == _evilUnits.Count)
        {
            maxSpawnableUnits = AIUnitSpawnRegulator.Instance.maxSpawnableUnits;
            PurchaseAIUnits();
            EndTurn();
            yield break;
        }

        for (int i = currentEvilUnitIndex; i < _evilUnits.Count; i++)
        {
            currentEvilUnitIndex = i;

            if (_evilUnits[i].faction == Unit.Faction.Evil && _evilUnits[i].unitType != Unit.UnitType.Settlement && factionTurn == FactionTurn.Evil)
                _evilUnits[i].GetNearestEnemy();

            clips = _evilUnits[i].GetAnimator().runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                switch (clip.name)
                {
                    case "attack":
                        attackAnimationDuration = clip.length;
                        break;
                }
            }

            currentEvilUnitIndex++;

            yield break;
        }
    }

    private void PurchaseAIUnits()
    {
        if (_evilUnits.Count >= AIUnitSpawnRegulator.Instance.maxUnits) return;
        
        int randomNumber;

        SelectSpawnableTile();
        
        if (AIUnitSpawnRegulator.Instance.isScoutUnlocked)
            randomNumber = Random.Range(0, 10);
        else if (AIUnitSpawnRegulator.Instance.isArcherUnlocked)
            randomNumber = Random.Range(0, 7);
        else
            randomNumber = Random.Range(0, 4);
        
        if (randomNumber <= 4 && darkFactionGold >= darkSoldierPrice)
            InstantiateAIUnit(AIUnitSpawnRegulator.Instance.darkSoldier, darkSoldierPrice);
        else if (randomNumber > 4 && randomNumber <= 7 && darkFactionGold >= darkArcherPrice)
            InstantiateAIUnit(AIUnitSpawnRegulator.Instance.darkArcher, darkArcherPrice);
        else if (randomNumber > 7 && randomNumber <= 10 && darkFactionGold >= darkScoutPrice)
            InstantiateAIUnit(AIUnitSpawnRegulator.Instance.darkScout, darkScoutPrice);
        
        maxSpawnableUnits--;

        if (maxSpawnableUnits > 0) PurchaseAIUnits();
        else maxSpawnableUnits = AIUnitSpawnRegulator.Instance.maxSpawnableUnits;
    }

    private void SelectSpawnableTile()
    {
        _freeTiles.Clear();
        
        foreach (Tile tile in ObjectHolder.Instance.tiles)
        {
            if (tile.GetVillage() != null && tile.GetVillage().villageState == Village.VillageState.Dark && tile.IsClear()) _freeTiles.Add(tile);
        }

        randomTileIndex = Random.Range(0, _freeTiles.Count);
    }

    private Unit _darkUnit;
    
    private void InstantiateAIUnit(GameObject unitToSpawn, int cost)
    {
        if (DarkVillage() <= 0) return;

        if (_freeTiles.Count > 0)
            _darkAIUnit = Instantiate(unitToSpawn, _freeTiles[randomTileIndex].transform.position,
                                      Quaternion.identity);
        else
            return;

        _darkUnit = _darkAIUnit.GetComponent<Unit>();
        _darkUnit.GetDeathVFX().SetActive(true);     
        EazySoundManager.PlaySound(AudioController.Instance.poofSfx, .2f);

        darkFactionGold -= cost;
        
        Instance.ResetTiles();
        Unit darkUnit = _darkAIUnit.GetComponent<Unit>();
        
        if (darkUnit != null)
        {
            darkUnit.hasMoved    = true;
            darkUnit.hasAttacked = true;
        }
    }

    private int DarkVillage()
    {
        int _darkVillageCount = 0;

        if (ObjectHolder.Instance.villages.Count > 0)
        {
            foreach (Village village in ObjectHolder.Instance.villages)
            {
                if (village.villageState == Village.VillageState.Dark) _darkVillageCount++;
            }
        }

        return _darkVillageCount;
    }

    private void UpdateFactionGold()
    {
        if (isSinglePlayer)
        {
            if (factionTurn == FactionTurn.Good)
                EazySoundManager.PlaySound(AudioController.Instance.villageGoldPerTurnSfx);
        }
        else { EazySoundManager.PlaySound(AudioController.Instance.villageGoldPerTurnSfx); }

        foreach (Village village in ObjectHolder.Instance.villages)
        {
            if (village.villageState == Village.VillageState.Blue && factionTurn == FactionTurn.Good)
            {
                blueFactionGold += village.GetGoldPerTurn();
                GameCanvas.Instance.UpdateBlueFactionGold(blueFactionGold);
            }
            else if (village.villageState == Village.VillageState.Dark && factionTurn == FactionTurn.Evil)
            {
                darkFactionGold += village.GetGoldPerTurn();
                
                if (isSinglePlayer == false)
                    GameCanvas.Instance.UpdatedarkFactionGold(darkFactionGold);
            }
        }
    }

    public void ResetTiles()
    {
        foreach (Tile tile in ObjectHolder.Instance.tiles) { tile.Reset(); }
    }

    public void SetFactionTurnIconAndText(Image icon, TextMeshProUGUI text)
    {
        factionTurnIcon = icon;
        factionTurnText = text;

        _factionTurnIconRectTransform = factionTurnIcon.GetComponent<RectTransform>();
    }

    public void EnableStatsPanel(Unit unit)
    {
        if (unit == null)
        {
            DisableStatsPanel();
            return;
        }

        if (unit.unitType == Unit.UnitType.Settlement)
        {
            VillageStatsCanvas.Instance.canvasGroup.alpha = 1f;
            VillageStatsCanvas.Instance.UpdateVillageStatsPanel(unit, unit.GetComponent<Village>());
            ObjectHolder.Instance.villageStatsPanel.transform.position =
                (Vector2) unit.transform.position + statsPanelShift;
        }
        else
        {
            UnitStatsCanvas.Instance.canvasGroup.alpha = 1f;
            UnitStatsCanvas.Instance.UpdateStatsPanel(unit);
            ObjectHolder.Instance.statsPanel.transform.position = (Vector2) unit.transform.position + statsPanelShift;
        }
    }

    public void MoveStatsPanel(Unit unit)
    {
        if (unit != null)
        {
            if (unit.unitType == Unit.UnitType.Settlement)
                ObjectHolder.Instance.villageStatsPanel.transform.position =
                    (Vector2) unit.transform.position + statsPanelShift;
            else
                ObjectHolder.Instance.statsPanel.transform.position =
                    (Vector2) unit.transform.position + statsPanelShift;
        }
    }

    public void DisableStatsPanel()
    {
        UnitStatsCanvas.Instance.canvasGroup.alpha    = 0f;
        VillageStatsCanvas.Instance.canvasGroup.alpha = 0f;
    }
}