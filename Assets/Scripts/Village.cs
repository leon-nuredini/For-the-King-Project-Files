using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class Village : MonoBehaviour
{
    [BoxGroup("Properties")]
    [SerializeField] private int _goldPerTurn;

    [BoxGroup("Properties")]
    public VillageState villageState = VillageState.None;

    [BoxGroup("Properties")]
    [SerializeField] private int _unitSpawnRange = 2;

    [BoxGroup("Properties")]
    [SerializeField] private LayerMask _tileLayerMask;

    [BoxGroup("Health Indicator")]
    [SerializeField] private GameObject _healthGameObject;

    [BoxGroup("Sword Indicator")]
    [SerializeField] private GameObject _swordObject;

    [BoxGroup("Sword Indicator")]
    [SerializeField] private Sprite _blueSword, _redSword;

    [BoxGroup("Village Prefabs")]
    [SerializeField] private GameObject _blueVillage, _evilVillage;

    [BoxGroup("Village Upgrade")]
    public GameObject upgradeGameobject;

    [BoxGroup("Village Upgrade")]
    public int healthToAdd = 1, goldPerTurnAdd = 1, upgradeCost = 10, currentLevel = 1, maxLevel = 5;

    [BoxGroup("Gold Gained Per Turn TMP")]
    [SerializeField] private TextMeshPro _tmpGoldGainedPerTurn;

    private SpriteRenderer _upgradeSpriteRenderer, _swordSpriteRenderer;
    private UpgradeVillage _upgradeVillage;
    private Unit           _unit;
    private Collider2D[]   _colliders;
    private BoxCollider2D  _upgradeBoxCollider;

    public enum VillageState
    {
        Blue,
        Dark,
        None
    }

    private void Awake()
    {
        ObjectHolder.Instance.villages.Add(this);

        _healthGameObject.transform.parent = null;

        if (_tmpGoldGainedPerTurn != null) _tmpGoldGainedPerTurn.transform.parent = null;
        if (upgradeGameobject     != null) upgradeGameobject.transform.parent     = null;

        if (upgradeGameobject != null)
        {
            _upgradeSpriteRenderer = upgradeGameobject.GetComponent<SpriteRenderer>();
            _upgradeVillage        = upgradeGameobject.GetComponent<UpgradeVillage>();
            _upgradeBoxCollider    = upgradeGameobject.GetComponent<BoxCollider2D>();
        }

        if (_swordObject != null) _swordSpriteRenderer = _swordObject.GetComponent<SpriteRenderer>();

        _unit = GetComponent<Unit>();

        if (villageState != VillageState.None) UpdateSpawnLocation();
    }

    public int GetGoldPerTurn() { return _goldPerTurn; }

    public void SpawnEnemyVillage(Unit attacker)
    {
        Destroy(_healthGameObject);
        Destroy(upgradeGameobject);

        if (_tmpGoldGainedPerTurn != null) Destroy(_tmpGoldGainedPerTurn.gameObject);

        switch (villageState)
        {
            case VillageState.Blue:
                Instantiate(_evilVillage, transform.position, Quaternion.identity);
                break;
            case VillageState.Dark:
                Instantiate(_blueVillage, transform.position, Quaternion.identity);
                break;
            case VillageState.None:
                if (attacker.faction == Unit.Faction.Good)
                    Instantiate(_blueVillage, transform.position, Quaternion.identity);
                else if (attacker.faction == Unit.Faction.Evil)
                    Instantiate(_evilVillage, transform.position, Quaternion.identity);
                break;
        }
    }

    public void EnableUpgradeButton()
    {
        if (upgradeGameobject.activeSelf)
        {
            _upgradeVillage.UpdateButton();
            _upgradeVillage.cost.SetActive(true);
            _upgradeSpriteRenderer.enabled = true;
            _upgradeBoxCollider.enabled    = true;
        }
    }

    public void DisableUpgradeButton()
    {
        if (upgradeGameobject.activeSelf)
        {
            _upgradeVillage.cost.SetActive(false);
            _upgradeSpriteRenderer.enabled = false;
            _upgradeBoxCollider.enabled    = false;
        }
    }

    public void EnableHealthDisplay() { _healthGameObject.SetActive(true); }

    public void DisableHealthDisplay() { _healthGameObject.SetActive(false); }

    public void UpdateStats()
    {
        _unit.SetHealth(healthToAdd * currentLevel - 1);
        upgradeCost  =  10             * currentLevel;
        _goldPerTurn += goldPerTurnAdd * currentLevel;

        if (currentLevel == 3)
            _goldPerTurn--;
        else if (currentLevel == 4)
            _goldPerTurn--;
        else if (currentLevel == 5) _goldPerTurn -= 2;

        _unit.UpdateHealthText();

        if (currentLevel == maxLevel)
        {
            upgradeGameobject.SetActive(false);
            _healthGameObject.SetActive(true);
        }
    }

    public void DisplayGoldGainedText()
    {
        DOTween.Kill(_tmpGoldGainedPerTurn.transform);

        _tmpGoldGainedPerTurn.text               = "+" + _goldPerTurn + "<sprite=3>";
        _tmpGoldGainedPerTurn.transform.position = transform.position;
        _tmpGoldGainedPerTurn.transform.DOScale(0f, 0f);
        _tmpGoldGainedPerTurn.transform.DOScale(.2f, .5f).SetEase(Ease.OutBounce);
        _tmpGoldGainedPerTurn.transform.DOMove(new Vector3(transform.position.x, transform.position.y + 1f, 0f), 1f)
                             .OnComplete(DisableGoldGainedText);
    }

    public void UpdateSpawnLocation()
    {
        _colliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(_unitSpawnRange, _unitSpawnRange), 0f,
                                             _tileLayerMask);

        foreach (Collider2D collider in _colliders)
            if (collider.gameObject.CompareTag(Tags.Tile))
                collider.GetComponent<Tile>().SetVillage(this);
    }

    private void DisableGoldGainedText() { _tmpGoldGainedPerTurn.transform.DOScale(0f, .5f).SetEase(Ease.InBounce); }

    public void UpdateSwordIcon(Unit unit)
    {
        if (unit.faction == Unit.Faction.Good)
            _swordSpriteRenderer.sprite                                         = _blueSword;
        else if (unit.faction == Unit.Faction.Evil) _swordSpriteRenderer.sprite = _redSword;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(_unitSpawnRange, _unitSpawnRange, _unitSpawnRange));
    }
}