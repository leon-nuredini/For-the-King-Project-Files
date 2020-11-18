using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Hellmade.Sound;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    [BoxGroup("Tiles")]
    [SerializeField] private Sprite[] _tileSprites;

    [BoxGroup("Background Trees")]
    [SerializeField] private GameObject _backgroundTree;

    [BoxGroup("Background Trees")]
    [SerializeField] private float _minSpawnX,
                                   _maxSpawnX,
                                   _minSpawnY,
                                   _maxSpawnY,
                                   _minZRotation,
                                   _maxZRotation,
                                   _minScale,
                                   _maxScale;

    [BoxGroup("Background Trees")] [Header("Chances are from 0 - 10, 0% to 100%")] [Range(0, 10)]
    [SerializeField] private int _treeSpawnChance = 5, _doubleTreeSpawnChance = 6, _rotateTreeChance = 5;

    [BoxGroup("Tile Properties")]
    [SerializeField] private float _hoverAmount;

    [BoxGroup("Tile Properties")]
    public Color highlightedColor;

    [BoxGroup("Tile Properties")]
    public bool isWalkable;

    [BoxGroup("Obstacle Check")]
    [SerializeField] private LayerMask _obstacleLayer;

    [BoxGroup("Obstacle Check")]
    [SerializeField] private float _obstacleCheckRadius = .2f;

    [BoxGroup("Place Unit Color")]
    public Color placeableColor;

    [BoxGroup("Place Unit Color")]
    public bool isPlaceable;

    [SerializeField] private Collider2D _obstacle;

    private SpriteRenderer _spriteRenderer;
    private GameObject     _tree1GameObject, _tree2GameObject;
    private Transform      _tree1Transform,  _tree2Transform;
    private Vector2        _backgroundTreeSpawnPosition;
    private Village        _village;

    private int   _doubleTreeChance, _spawnTreeChance;
    private float _originalScale,    _randomZRotation, _randomScale;
    private bool  _isSettlementTile;
    private GameObject _ghostUnit;

    private void Awake()
    {
        _originalScale = transform.localScale.x;

        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_tileSprites.Length > 0) _spriteRenderer.sprite = _tileSprites[Random.Range(0, _tileSprites.Length)];

        _spawnTreeChance = Random.Range(0, 9);

        if (_spawnTreeChance >= _treeSpawnChance) InstantiateBackgroundTree();

        ObjectHolder.Instance.tiles.Add(this);
    }

    private void InstantiateBackgroundTree()
    {
        _doubleTreeChance = Random.Range(0, 9);

        SpawnTree(_tree1GameObject, _tree1Transform, 2);

        if (_doubleTreeChance >= _doubleTreeSpawnChance) SpawnTree(_tree2GameObject, _tree2Transform, 3);
    }

    private void SpawnTree(GameObject treeGameObject, Transform treeTransform, int sortingLayerIndex)
    {
        _backgroundTreeSpawnPosition =
            new Vector2(Random.Range(_minSpawnX, _maxSpawnX), Random.Range(_minSpawnY, _maxSpawnY));

        _randomScale = Random.Range(_minScale, _maxScale);

        treeGameObject = Instantiate(_backgroundTree,
                                     _backgroundTreeSpawnPosition, Quaternion.identity, transform);

        treeGameObject.GetComponent<SpriteRenderer>().sortingOrder = sortingLayerIndex;

        treeTransform               = treeGameObject.transform;
        treeTransform.localPosition = _backgroundTreeSpawnPosition;

        if (treeTransform.position.x < 0f)
            _randomZRotation                                     = Random.Range(-_minZRotation, -_maxZRotation);
        else if (treeTransform.position.x > 0f) _randomZRotation = Random.Range(_minZRotation,  _maxZRotation);

        if (_rotateTreeChance >= Random.Range(0, 10))
            treeTransform.rotation =
                Quaternion.Euler(new Vector3(treeTransform.localRotation.x, treeTransform.localRotation.y,
                                             _randomZRotation));

        treeTransform.localScale = new Vector2(_randomScale, _randomScale);
    }

    public bool IsClear()
    {
        if (_isSettlementTile) return false;

        _obstacle = Physics2D.OverlapCircle(transform.position, _obstacleCheckRadius, _obstacleLayer);

        if (_obstacle == null) return true;
        if (_obstacle.gameObject.CompareTag(Tags.Settlement)) _isSettlementTile = true;
        if (_obstacle.gameObject.CompareTag(Tags.Upgrade)) return true;

        return false;
    }

    public void Highlight()
    {
        _spriteRenderer.color = highlightedColor;
        isWalkable            = true;
    }

    public void Reset()
    {
        _spriteRenderer.color = Color.white;
        isWalkable            = false;
        isPlaceable           = false;
    }

    public void SetPlaceable()
    {
        _spriteRenderer.color = placeableColor;
        isPlaceable           = true;
    }

    public void SetVillage(Village newVillage)
    {
        _village = newVillage;
    }

    public Village GetVillage() { return _village; }

    private void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        transform.DOScale(_hoverAmount, 0f);
        _spriteRenderer.sortingOrder = 1;

        if (isPlaceable)
        {
            switch (GameManager.Instance.purchasedItem.GetComponent<Unit>().unitType)
            {
                case Unit.UnitType.Soldier: _ghostUnit = Instantiate(ObjectHolder.Instance.blueSoldierGhost, transform.position, Quaternion.identity);
                    break;
                case Unit.UnitType.Archer: _ghostUnit = Instantiate(ObjectHolder.Instance.blueArcherGhost, transform.position, Quaternion.identity);
                    break;
                case Unit.UnitType.Spirit: _ghostUnit = Instantiate(ObjectHolder.Instance.blueScoutGhost, transform.position, Quaternion.identity);
                    break;
            }
        }
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (isWalkable && GameManager.Instance.selectedUnit != null)
        {
            GameManager.Instance.selectedUnit.Move(transform.position, false);
        }
        else if (isPlaceable)
        {
            if (_ghostUnit != null)
                Destroy(_ghostUnit);
            
            BarrackItem item = Instantiate(GameManager.Instance.purchasedItem,
                                           new Vector2(transform.position.x, transform.position.y),
                                           Quaternion.identity);
            GameManager.Instance.ResetTiles();
            Unit unit = item.GetComponent<Unit>();
            
            unit.GetDeathVFX().gameObject.SetActive(true);
            EazySoundManager.PlaySound(AudioController.Instance.poofSfx, .2f);

            if (unit.faction == Unit.Faction.Good)
            {
                switch (unit.unitType)
                {
                    case Unit.UnitType.Soldier:
                        item.cost = GameManager.Instance.blueSoldierPrice;
                        break;
                    case Unit.UnitType.Archer:
                        item.cost = GameManager.Instance.blueArcherPrice;
                        break;
                    case Unit.UnitType.Spirit:
                        item.cost = GameManager.Instance.blueScoutPrice;
                        break;
                }

                GameManager.Instance.blueFactionGold -= item.cost;
                EazySoundManager.PlaySound(AudioController.Instance.villageGoldPerTurnSfx);
            }
            else if (unit.faction == Unit.Faction.Evil)
            {
                switch (unit.unitType)
                {
                    case Unit.UnitType.Soldier:
                        item.cost = GameManager.Instance.darkSoldierPrice;
                        break;
                    case Unit.UnitType.Archer:
                        item.cost = GameManager.Instance.darkArcherPrice;
                        break;
                    case Unit.UnitType.Spirit:
                        item.cost = GameManager.Instance.darkScoutPrice;
                        break;
                }

                GameManager.Instance.darkFactionGold -= item.cost;
                EazySoundManager.PlaySound(AudioController.Instance.villageGoldPerTurnSfx);
            }

            GameCanvas.Instance.UpdateBlueFactionGold(GameManager.Instance.blueFactionGold);
            GameCanvas.Instance.UpdatedarkFactionGold(GameManager.Instance.darkFactionGold);

            if (unit != null)
            {
                unit.hasMoved    = true;
                unit.hasAttacked = true;
            }
        }
    }

    private void OnMouseExit()
    {
        transform.DOScale(_originalScale, 0f);
        _spriteRenderer.sortingOrder = 0;
        
        if (_ghostUnit != null)
            Destroy(_ghostUnit); 
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag(Tags.EvilUnit))
            Debug.Log(col.gameObject.name);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _obstacleCheckRadius);
    }
}