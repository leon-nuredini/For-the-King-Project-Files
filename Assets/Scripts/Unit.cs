using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Hellmade.Sound;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour
{
    public bool isSelected, hasProjectile, hasMagic, hasMagicPath, isAiFinished;

    private static readonly string DeadCharactersSortingLayerName = "Dead Characters";

    [HideInInspector] public int initActionPoints;

    [BoxGroup("Unit Properties")]
    public UnitType unitType = UnitType.Soldier;

    [BoxGroup("Unit Properties")]
    [SerializeField] private int _tileRange, _attackRange, _health, _attackDamage, _defenseDamage, _armor;

    [BoxGroup("Unit Properties")]
    [SerializeField] private float _moveSpeedPerTile; // the lesser the value the faster it will move

    [BoxGroup("Unit Properties")]
    public bool hasMoved, hasAttacked, hasHealed;

    [BoxGroup("Unit Properties")]
    public Faction faction = Faction.Good;

    [BoxGroup("Unit Properties")]
    [SerializeField] private GameObject _deathVfx;

    [BoxGroup("Unit Properties")]
    [ShowIf("hasProjectile")] [SerializeField]
    private GameObject _arrow;

    [BoxGroup("Unit Properties")]
    [ShowIf("hasMagic")] [SerializeField]
    private GameObject _magic;

    [BoxGroup("Unit Properties")]
    [ShowIf("hasMagic")] [SerializeField]
    private ParticleSystem _magicVfx;

    [BoxGroup("Unit Properties")]
    [ShowIf(EConditionOperator.And, "hasMagic", "hasMagicPath")] [SerializeField]
    private Transform[] _magicPath;

    [BoxGroup("Tile Check")]
    [SerializeField] private LayerMask _tileMask;

    [BoxGroup("Tile Check")]
    [SerializeField] private float _tileCheckRadius = .1f;

    [BoxGroup("Tile Check")]
    [SerializeField] private Collider2D _tilecollider;

    [BoxGroup("Weapon Icon")]
    [SerializeField] private GameObject _weaponIcon;

    [BoxGroup("Heal Icon")]
    [SerializeField] private GameObject _healIcon;

    [BoxGroup("Sword Break Icon")]
    [SerializeField] private GameObject _swordBreakIcon;

    [BoxGroup("Health Indicator")]
    [SerializeField] private GameObject _healthGameobject;

    [BoxGroup("Health Indicator")]
    [SerializeField] private TextMeshPro _healthTmp;

    [BoxGroup("Unit Reach")]
    public UnitReach unitReach;

    [BoxGroup("Target")]
    public Unit target;

    [BoxGroup("Target")]
    public Tile targetTile;

    [BoxGroup("Target")]
    [SerializeField] private Vector3 _cubeSizeVertical   = new Vector3(.5f, 3f,  0f),
                                     _cubeSizeHorizontal = new Vector3(3f,  .5f, 0f);

    [BoxGroup("Target")]
    [SerializeField] private LayerMask _obstacleLayer;

    [BoxGroup("AI")]
    [SerializeField] private bool _isStationary;
    
    [BoxGroup("AI")]
    public bool isTutorial;

    [BoxGroup("Movement Action Points / AI Only")]
    public int movementActionPoints;

    [SerializeField] private List<Unit>    _enemiesInRange    = new List<Unit>();
    [SerializeField] private List<Unit>    _blueUnitsToAttack = new List<Unit>();
    [SerializeField] private List<Village> _villagesInRange   = new List<Village>();
    [SerializeField] private List<Unit>    _friendliesInRange = new List<Unit>();

    public enum Faction
    {
        Good,
        Evil,
        Neutral
    }

    public enum UnitType
    {
        Soldier,
        Archer,
        Spirit,
        King,
        Settlement
    }

    private Transform      _tileTransform, _otherUnitTransform, _otherVillageTransform;
    private Animator       _animator;
    private BoxCollider2D  _boxCollider2D;
    private Collider2D     _unitToAttack;
    private Unit           _tempUnit,            _enemyUnit, _tempBlueUnit;
    private SpriteRenderer _arrowSpriteRenderer, _magicSpriteRenderer;
    private Color          _magicSpriteColor;
    private Village        _village;
    private AIPathCheck    _aiPathCheck;

    private AnimationClip[]  _clips;
    private SpriteRenderer[] _bodyPartsSpriteRenderers;
    private Vector3[]        _magicProjectilePaths;
    private Collider2D[]     _blueUnitsColliders;

    private bool  _isCentered, _isAiMoving, _hasMovedUp, _hasMovedDown;
    private float _attackAnimationDuration;
    private int   _initHealth, _initDefenseDamage;

    private void Awake()
    {
        _animator             = GetComponent<Animator>();
        _boxCollider2D        = GetComponent<BoxCollider2D>();
        _magicProjectilePaths = new Vector3[_magicPath.Length];

        _animator.Play(0, -1, Random.value);

        ObjectHolder.Instance.units.Add(this);

        PositionToTile();

        if (unitType == UnitType.King)
        {
            _magicSpriteRenderer       = _magic.GetComponent<SpriteRenderer>();
            _magicSpriteColor          = _magicSpriteRenderer.color;
            _magicSpriteColor.a        = 0f;
            _magicSpriteRenderer.color = _magicSpriteColor;

            UpdateKingHealthUi();
        }

        if (hasProjectile) _arrowSpriteRenderer = _arrow.GetComponent<SpriteRenderer>();

        if (faction == Faction.Evil)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        _healthGameobject.transform.localScale =
            new Vector3(Mathf.Sign(transform.localScale.x) * Mathf.Abs(_healthGameobject.transform.localScale.x),
                        _healthGameobject.transform.localScale.y, 1f);

        if (unitType == UnitType.Settlement) _village = GetComponent<Village>();

        UpdateHealthText();

        _initHealth        = _health;
        _initDefenseDamage = _defenseDamage;
        movementActionPoints = _tileRange;
        
        initActionPoints = movementActionPoints;
    }

    private void OnMouseOver()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        GameManager.Instance.EnableStatsPanel(this);
    }

    private void OnMouseExit() { GameManager.Instance.DisableStatsPanel(); }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (_health <= 0) return;

        if (isSelected)
        {
            isSelected                        = false;
            GameManager.Instance.selectedUnit = null;
            GameManager.Instance.ResetTiles();

            if (unitType == UnitType.Settlement && _village.currentLevel < _village.maxLevel)
            {
                _village.EnableHealthDisplay();
                _village.DisableUpgradeButton();
            }

            ResetIcons();
        }
        else
        {
            if ((int) faction == (int) GameManager.Instance.factionTurn)
            {
                if (GameManager.Instance.selectedUnit != null)
                {
                    PlayEmoteSound();

                    if (GameManager.Instance.selectedUnit.faction     == Faction.Good    &&
                        GameManager.Instance.selectedUnit.unitType    == UnitType.Spirit &&
                        GameManager.Instance.selectedUnit.hasHealed   == false           &&
                        GameManager.Instance.selectedUnit.hasAttacked == false           &&
                        GameManager.Instance.selectedUnit._friendliesInRange.Contains(this))
                    {
                        GameManager.Instance.selectedUnit.hasHealed = true;

                        if (GameManager.Instance.selectedUnit._magicVfx != null)
                        {
                            GameManager.Instance.selectedUnit._magicVfx.transform.position = transform.position;
                            GameManager.Instance.selectedUnit._magicVfx.Play();
                        }

                        ResetIcons();
                        HealUnit(this);
                        return;
                    }

                    GameManager.Instance.selectedUnit.isSelected = false;

                    if (GameManager.Instance.selectedUnit.unitType == UnitType.Settlement &&
                        GameManager.Instance.selectedUnit.GetVillage().currentLevel <
                        GameManager.Instance.selectedUnit.GetVillage().maxLevel)
                    {
                        GameManager.Instance.selectedUnit.GetVillage().EnableHealthDisplay();
                        GameManager.Instance.selectedUnit.GetVillage().DisableUpgradeButton();
                    }
                }

                ResetIcons();
                isSelected = true;
                PlayEmoteSound();
                GameManager.Instance.selectedUnit = this;
                GameManager.Instance.ResetTiles();

                if (unitType == UnitType.Settlement && _village.currentLevel < _village.maxLevel)
                {
                    _village.DisableHealthDisplay();
                    _village.EnableUpgradeButton();
                }

                GetTargetableUnits();
                GetWalkableTiles();
            }
        }

        _unitToAttack =
            Physics2D.OverlapCircle(ObjectHolder.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition), .15f);

        _tempUnit = _unitToAttack.GetComponent<Unit>();

        if (GameManager.Instance.selectedUnit != null)
        {
            if (GameManager.Instance.selectedUnit._enemiesInRange.Contains(_tempUnit) &&
                GameManager.Instance.selectedUnit.hasAttacked == false)
            {
                if (GameManager.Instance.selectedUnit.faction  == Faction.Evil &&
                    GameManager.Instance.selectedUnit.unitType == UnitType.Spirit)
                {
                    GameManager.Instance.selectedUnit._magicVfx.transform.position = transform.position;
                    GameManager.Instance.selectedUnit._magicVfx.Play();
                    _tempUnit._defenseDamage = 0;
                }

                if (GameManager.Instance.selectedUnit.unitType == UnitType.Spirit &&
                    GameManager.Instance.selectedUnit.faction  == Faction.Good)
                    if (GameManager.Instance.selectedUnit.hasHealed)
                        return;

                GameManager.Instance.selectedUnit.PlayAttackSound();
                GameManager.Instance.selectedUnit.Attack(_tempUnit);
                ResetIcons();
            }
        }
    }

    private void GetTargetableUnits()
    {
        _enemiesInRange.Clear();
        _villagesInRange.Clear();
        _friendliesInRange.Clear();

        ResetIcons();

        foreach (Unit unit in ObjectHolder.Instance.units)
        {
            if (unit._health <= 0) return;

            _otherUnitTransform = unit.transform;

            if (Mathf.Abs(transform.position.x - _otherUnitTransform.position.x) +
                Mathf.Abs(transform.position.y - _otherUnitTransform.position.y) <= _attackRange + 1)
            {
                if ((int) unit.faction != (int) GameManager.Instance.factionTurn && hasAttacked == false)
                {
                    _enemiesInRange.Add(unit);
                    if (GameManager.Instance.isSinglePlayer && GameManager.Instance.factionTurn == GameManager.FactionTurn.Evil) return;

                    if (faction == Faction.Evil && unitType == UnitType.Spirit && unit.unitType != UnitType.Settlement)
                    {
                        if (unit._swordBreakIcon.activeSelf == false) unit._swordBreakIcon.SetActive(true);
                    }
                    else
                    {
                        if (unitType == UnitType.Spirit && hasHealed) return;

                        if (unit._weaponIcon.activeSelf == false) unit._weaponIcon.SetActive(true);
                    }

                    if (unit.unitType == UnitType.Settlement)
                    {
                        if (GameManager.Instance.isSinglePlayer && GameManager.Instance.factionTurn == GameManager.FactionTurn.Evil) return;
                        
                        if (unit.GetVillage().villageState == Village.VillageState.None)
                            unit.GetVillage().UpdateSwordIcon(this);
                    }
                }
                else if (!Equals(unit) && faction == Faction.Good && unitType    == UnitType.Spirit &&
                         hasHealed                == false        && hasAttacked == false)
                {
                    if (unit._health < unit._initHealth)
                    {
                        _friendliesInRange.Add(unit);

                        if (unit._healIcon != null)
                        {
                            if (unit._healIcon.activeSelf == false) unit._healIcon.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    public List<Unit> CheckForTargetableEnemies()
    {
        List<Unit> units = new List<Unit>();

        foreach (Unit unit in ObjectHolder.Instance.units)
        {
            if (unit._health > 0)
            {
                _otherUnitTransform = unit.transform;

                if (Mathf.Abs(transform.position.x - _otherUnitTransform.position.x) +
                    Mathf.Abs(transform.position.y - _otherUnitTransform.position.y) <= _attackRange + 1)
                {
                    if ((int) unit.faction != (int) GameManager.Instance.factionTurn && hasAttacked == false)
                    {
                        units.Add(unit);
                    }
                }   
            }
        }

        return units;
    }

    public void ResetIcons()
    {
        foreach (Unit unit in ObjectHolder.Instance.units)
        {
            if (unit._weaponIcon.activeSelf) unit._weaponIcon.SetActive(false);

            if (unit.faction == Faction.Good && unit._healIcon != null) unit._healIcon.SetActive(false);

            if (unit.faction == Faction.Good && unit._swordBreakIcon != null) unit._swordBreakIcon.SetActive(false);
        }
    }

    #region Move

    public void Move(Vector2 destination, bool isAi)
    {
        PlayEmoteSound();
        AnimatorParam.SetParams(_animator, AnimatorParams.Walk, AnimatorParams.AnimParamType.Trigger);

        hasMoved = true;
        GameManager.Instance.ResetTiles();

        if (isAi)
        {
            _hasMovedUp   = false;
            _hasMovedDown = false;
            AIMove();
            return;
        }

        if (destination.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y,
                                               transform.localScale.z);
        else if (destination.x < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y,
                                               transform.localScale.z);

        _healthGameobject.transform.localScale =
            new Vector3(Mathf.Sign(transform.localScale.x) * Mathf.Abs(_healthGameobject.transform.localScale.x),
                        _healthGameobject.transform.localScale.y, 1f);

        transform.DOMoveX(destination.x,
                          _moveSpeedPerTile * Mathf.Abs(transform.position.x - destination.x))
                 .SetEase(Ease.Linear).OnComplete(() => UpdatePositionY(destination, isAi));

        GameManager.Instance.MoveStatsPanel(this);
    }

    public void UpdatePositionY(Vector2 destination, bool isAi)
    {
        transform.DOMoveY(destination.y,
                          _moveSpeedPerTile * Mathf.Abs(transform.position.y - destination.y))
                 .SetEase(Ease.Linear).OnComplete(() => SetToIdleAnimation(isAi));

        GameManager.Instance.MoveStatsPanel(this);
    }

    private void SetToIdleAnimation(bool isAi)
    {
        AnimatorParam.SetParams(_animator, AnimatorParams.Idle, AnimatorParams.AnimParamType.Trigger);
        GetTargetableUnits();

        if (isAi)
        {
            foreach (Unit unit in ObjectHolder.Instance.units)
                if (unit.faction == Faction.Good && unit.unitReach != null)
                    unit.unitReach.GetNearbyReachableTiles();

            Invoke(nameof(GetNearestEnemy), .2f);
        }
        
        GameCanvas.Instance.RestoreFocusToEndTurnButton();
    }

    private void GetWalkableTiles()
    {
        if (GameManager.Instance.isSinglePlayer && faction == Faction.Evil) return;
        if (hasMoved) return;

        foreach (Tile tile in ObjectHolder.Instance.tiles)
        {
            _tileTransform = tile.transform;

            if (Mathf.Abs(transform.position.x - _tileTransform.position.x) +
                Mathf.Abs(transform.position.y - _tileTransform.position.y) <= _tileRange + 1)
                if (tile.IsClear()) { tile.Highlight(); }
        }
    }

    #endregion

    private void PositionToTile()
    {
        _tilecollider = Physics2D.OverlapCircle(transform.position, _tileCheckRadius, _tileMask);

        if (_tilecollider.gameObject.CompareTag(Tags.Tile))
            transform.position = new Vector2(_tilecollider.gameObject.transform.position.x,
                                             _tilecollider.gameObject.transform.position.y);
    }

    #region Attack

    public void Attack(Unit enemy)
    {
        _enemyUnit = enemy;

        if (enemy.transform.position.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y,
                                               transform.localScale.z);
        else if (enemy.transform.position.x < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y,
                                               transform.localScale.z);

        _healthGameobject.transform.localScale =
            new Vector3(Mathf.Sign(transform.localScale.x) * Mathf.Abs(_healthGameobject.transform.localScale.x),
                        _healthGameobject.transform.localScale.y, 1f);

        if (hasProjectile)
        {
            switch (unitType)
            {
                case UnitType.Archer:
                    AttackWithProjectile();
                    break;
            }
        }

        StartCoroutine(InitializeAttack(enemy));
    }

    private void ReturnToOriginalPosition(Vector2 position)
    {
        ShakeCamera();
        transform.DOMove(position, .15f).SetEase(Ease.Linear);
    }

    private DamageIcon _tempPlayerDamageIcon, _tempEnemyDamageIcon;
    
    private IEnumerator InitializeAttack(Unit enemy)
    {
        if (unitType == UnitType.Spirit)
        {
            Vector2 originalPosition = transform.position;

            transform.DOMove(enemy.transform.position, .15f).SetEase(Ease.Linear)
                     .OnComplete(() => ReturnToOriginalPosition(originalPosition));
        }

        AnimatorParam.SetParams(_animator, AnimatorParams.Attack, AnimatorParams.AnimParamType.Trigger);

        hasAttacked = true;
        GameCanvas.Instance.RestoreFocusToEndTurnButton();

        int enemyDamage = _attackDamage - enemy._armor;
        
        if (enemy.unitType == UnitType.King && enemy.faction == Faction.Evil && enemy._isStationary)
            enemy._isStationary = false;

        if (enemyDamage >= 1)
        {
            enemy._health -= enemyDamage;
            
            _tempEnemyDamageIcon = Instantiate(ObjectHolder.Instance.enemyDamageIcon).GetComponent<DamageIcon>();
            _tempEnemyDamageIcon.Setup(enemyDamage);
            _tempEnemyDamageIcon.transform.position = enemy.transform.position;
        }

        enemy.UpdateHealthText();

        if (_enemyUnit.unitType == UnitType.King) _enemyUnit.UpdateKingHealthUi();

        if (enemy._health <= 0)
        {
            if (enemy.unitType == UnitType.Settlement)
                ObjectHolder.Instance.villages.Remove(enemy.GetComponent<Village>());

            ObjectHolder.Instance.units.Remove(enemy);
            StartCoroutine(DestroyUnit(enemy));
            GetWalkableTiles();
            yield break;
        }

        if (Mathf.Abs(transform.position.x - enemy.transform.position.x) +
            Mathf.Abs(transform.position.y - enemy.transform.position.y) > enemy._attackRange + 1)
            yield break;

        yield return new WaitForSeconds(.5f);

        int myDamage = enemy._defenseDamage - _armor;

        if (myDamage >= 1)
        {
            _health -= myDamage;

            _tempPlayerDamageIcon = Instantiate(ObjectHolder.Instance.playerDamageIcon).GetComponent<DamageIcon>();
            _tempPlayerDamageIcon.Setup(myDamage);
            _tempPlayerDamageIcon.transform.position = transform.position;
        }

        UpdateHealthText();

        if (_health <= 0)
        {
            if (unitType == UnitType.King && faction == Faction.Evil) GameManager.Instance.isDarkKingDead = true;
            
            StartCoroutine(DestroyUnit(this));
            GameManager.Instance.ResetTiles();

            if (unitType == UnitType.Settlement) ObjectHolder.Instance.villages.Remove(GetComponent<Village>());

            ObjectHolder.Instance.units.Remove(this);
        }
    }

    private IEnumerator DestroyUnit(Unit unit)
    {
        if (GameManager.Instance.isGameEnded) yield return null;

        unit._boxCollider2D.enabled = false;

        _bodyPartsSpriteRenderers = unit.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer spriteRenderer in _bodyPartsSpriteRenderers)
            spriteRenderer.sortingLayerName = DeadCharactersSortingLayerName;

        _clips = _animator.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in _clips)
            if (clip.name == AnimatorParams.ClipAttack) { _attackAnimationDuration = clip.length; }

        yield return new WaitForSeconds(_attackAnimationDuration);

        if (AnimatorParam.HasParameter(unit._animator, AnimatorParams.Die))
         AnimatorParam.SetParams(unit._animator, AnimatorParams.Die, AnimatorParams.AnimParamType.Trigger);

        yield return new WaitForSeconds(_attackAnimationDuration + .5f);

        EazySoundManager.PlaySound(AudioController.Instance.poofSfx, .2f);

        unit._deathVfx.transform.parent     = null;
        unit._deathVfx.transform.localScale = Vector3.one;
        unit._deathVfx.SetActive(true);

        if (unit.unitType == UnitType.King)
        {
            if (unit.faction == Faction.Good)
            {
                GameCanvas.Instance.ShowDarkWinPanel();
                EazySoundManager.PlaySound(AudioController.Instance.loseSfx);
            }
            else if (unit.faction == Faction.Evil)
            {
                GameCanvas.Instance.ShowBlueWinPanel();
                EazySoundManager.PlaySound(AudioController.Instance.winSfx);
            }

            GameManager.Instance.isGameEnded = true;
            EazySoundManager.StopAllMusic();
            yield break;
        }

        if (unit.unitType == UnitType.Settlement) unit.GetComponent<Village>().SpawnEnemyVillage(this);

        Destroy(unit.gameObject);
    }

    public void AttackWithProjectile()
    {
        EazySoundManager.PlaySound(AudioController.Instance.arrowShootSfx);

        _arrow.transform.position = transform.position;

        if (transform.localScale.x < 0)
            _arrowSpriteRenderer.flipX = true;
        else
            _arrowSpriteRenderer.flipX = false;

        _arrow.SetActive(true);
        TransformExtensions.LookAt2D(_arrow.transform, _enemyUnit.transform);
        float   distance = Vector2.Distance(_arrow.transform.position, _enemyUnit.transform.position);
        Vector3 offset   = new Vector3(0f, .2f, 0f);
        _arrow.transform.DOMove(_enemyUnit.transform.position + offset, distance / 10f).OnComplete(ShakeCamera);
    }

    public void AttackWithMagic() // called from animation event (from the kings attack animation)
    {
        if (_magic.activeSelf == false) _magic.SetActive(true);

        _magicVfx.Play();
        _magic.transform.parent = null;

        float   distance = Vector2.Distance(_magic.transform.position, _enemyUnit.transform.position);
        Vector3 offset   = new Vector3(0f, .2f, 0f);

        if (unitType == UnitType.King)
        {
            _magicSpriteColor.a = .7f;
            _magicSpriteRenderer.DOColor(_magicSpriteColor, .25f);

            if (faction == Faction.Good)
                EazySoundManager.PlaySound(AudioController.Instance.blueKingMagicAttackSfx);
            else if (faction == Faction.Evil)
                EazySoundManager.PlaySound(AudioController.Instance.darkKingMagicAttackSfx);
        }

        if (hasMagicPath)
        {
            for (int i = 0; i < _magicPath.Length; i++) _magicProjectilePaths[i] = _magicPath[i].position;

            if (faction == Faction.Good)
                _magic.transform.DOPath(_magicProjectilePaths, .4f, PathType.Linear, PathMode.Ignore, 5, Color.red)
                      .OnComplete(() => RotateMagicProjectile(distance));
            else
                _magic.transform.DOPath(_magicProjectilePaths, .4f, PathType.Linear, PathMode.Ignore, 5, Color.red)
                      .OnComplete(() => MoveMagicProjectile(offset, distance));
        }
        else
            _magic.transform.DOMove(_enemyUnit.transform.position + offset, distance / 10f).OnComplete(ShakeCamera);
    }

    private void RotateMagicProjectile(float distance)
    {
        _magic.transform.DOLocalRotate(new Vector3(0f, 0f, Mathf.Sign(transform.localScale.x) * -130f), .2f)
              .OnComplete(ShakeCamera);
    }

    private void MoveMagicProjectile(Vector3 offset, float distance) // if magic path exists
    {
        _magic.transform.DOMove(_enemyUnit.transform.position + offset, distance / 10f).OnComplete(ShakeCamera);
    }

    #endregion

    public void HealUnit(Unit _unitToHeal)
    {
        if (_unitToHeal._health < _unitToHeal._initHealth)
        {
            _unitToHeal._health++;
            _unitToHeal.UpdateHealthText();
            EazySoundManager.PlaySound(AudioController.Instance.magicHealSfx);
        }
    }

    public void ShakeCamera() // called from animation event
    {
        if (unitType == UnitType.King)
        {
            _magicVfx.Stop();
            _magicSpriteColor.a = 0f;
            _magicSpriteRenderer.DOColor(_magicSpriteColor, .25f).OnComplete(ResetMagicPosition);
        }
        else if (unitType == UnitType.Archer) { EazySoundManager.PlaySound(AudioController.Instance.arrowHitSfx, .3f); }
        else { EazySoundManager.PlaySound(AudioController.Instance.gettingHitSfx,                                .2f); }

        if (hasProjectile && _arrow.activeSelf) _arrow.SetActive(false);

        if (_enemyUnit != null)
        {
            if (AnimatorParam.HasParameter(_enemyUnit._animator, AnimatorParams.Hit))
                AnimatorParam.SetParams(_enemyUnit._animator, AnimatorParams.Hit, AnimatorParams.AnimParamType.Trigger);
        }

        CameraShakeManager.Instance.ShakeCameraAttack();

        if (GameManager.Instance.isGameEnded == false && GameManager.Instance.isSinglePlayer &&
            GameManager.Instance.factionTurn == GameManager.FactionTurn.Evil)
        {
            _isAiMoving = false;
            GameManager.Instance.ResumeAiCoroutine(1f);
        }
    }

    private void ResetMagicPosition()
    {
        _magic.transform.parent   = transform;
        _magic.transform.position = _magicProjectilePaths[0];

        if (faction == Faction.Good) _magic.transform.DOLocalRotate(Vector3.zero, 0f);
    }

    public void UpdateHealthText()
    {
        if (_health < 0)
            _healthTmp.text = "0";
        else
            _healthTmp.text = _health.ToString();
    }

    #region AI

    public void GetNearestEnemy()
    {
        if (AttackTargetableBlueUnits(false))
        {
            isAiFinished = true;
            return;
        }

        if (hasMoved || isAiFinished) return;

        float distanceToTile;
        float nearestTileDistance = Mathf.Infinity;
        target     = null;
        targetTile = null;

        foreach (Unit unit in ObjectHolder.Instance.units)
        {
            if (unit.faction == Faction.Good || unit.faction == Faction.Neutral)
            {
                if (unit.unitReach.reachableTiles.Count > 0)
                {
                    foreach (Tile reachTile in unit.unitReach.reachableTiles)
                    {
                        distanceToTile = Vector2.Distance(transform.position, reachTile.transform.position);

                        if (nearestTileDistance > distanceToTile)
                        {
                            nearestTileDistance = distanceToTile;
                            target              = unit;
                            targetTile          = reachTile;
                        }
                    }
                }
            }
        }

        if (targetTile != null && _isStationary == false)
        {
            _isAiMoving = true;
            Move(targetTile.transform.position, true);
        }
        else
        {
            _isAiMoving = false;
            
            _healthGameobject.transform.localScale =
                new Vector3(Mathf.Sign(transform.localScale.x) * Mathf.Abs(_healthGameobject.transform.localScale.x),
                            _healthGameobject.transform.localScale.y, 1f);
            
            GameManager.Instance.ResumeAiCoroutine(0f);
        }
    }
    
    public bool AIArcherCheckForEnemies()
    {
        _blueUnitsToAttack.Clear();

        _blueUnitsColliders = Physics2D.OverlapBoxAll(transform.position, _cubeSizeVertical, 0f, _obstacleLayer);

        foreach (Collider2D col in _blueUnitsColliders)
        {
            if (col.gameObject.CompareTag(Tags.GoodUnit) || col.gameObject.CompareTag(Tags.Settlement))
            {
                _tempBlueUnit = col.GetComponent<Unit>();
                if (_tempBlueUnit.faction == Faction.Good || _tempBlueUnit.faction == Faction.Neutral)
                {
                    _blueUnitsToAttack.Add(_tempBlueUnit);
                    return true;
                }
            }
        }

        _blueUnitsColliders = Physics2D.OverlapBoxAll(transform.position, _cubeSizeHorizontal, 0f, _obstacleLayer);

        foreach (Collider2D col in _blueUnitsColliders)
        {
            if (col.gameObject.CompareTag(Tags.GoodUnit) || col.gameObject.CompareTag(Tags.Settlement))
            {
                _tempBlueUnit = col.GetComponent<Unit>();
                if (_tempBlueUnit.faction == Faction.Good || _tempBlueUnit.faction == Faction.Neutral)
                {
                    _blueUnitsToAttack.Add(_tempBlueUnit);
                    return true;
                }
            }
        }

        return false;
    }

    private bool AttackTargetableBlueUnits(bool changeAiState)
    {
        _blueUnitsToAttack.Clear();

        _blueUnitsColliders = Physics2D.OverlapBoxAll(transform.position, _cubeSizeVertical, 0f, _obstacleLayer);

        foreach (Collider2D col in _blueUnitsColliders)
        {
            if (col.gameObject.CompareTag(Tags.GoodUnit) || col.gameObject.CompareTag(Tags.Settlement))
            {
                _tempBlueUnit = col.GetComponent<Unit>();
                if (_tempBlueUnit.faction == Faction.Good || _tempBlueUnit.faction == Faction.Neutral)
                    _blueUnitsToAttack.Add(_tempBlueUnit);
            }
        }

        _blueUnitsColliders = Physics2D.OverlapBoxAll(transform.position, _cubeSizeHorizontal, 0f, _obstacleLayer);

        foreach (Collider2D col in _blueUnitsColliders)
        {
            if (col.gameObject.CompareTag(Tags.GoodUnit) || col.gameObject.CompareTag(Tags.Settlement))
            {
                _tempBlueUnit = col.GetComponent<Unit>();
                if (_tempBlueUnit.faction == Faction.Good || _tempBlueUnit.faction == Faction.Neutral)
                    _blueUnitsToAttack.Add(_tempBlueUnit);
            }
        }

        if (_blueUnitsToAttack.Count > 0)
        {
            movementActionPoints = 0;
            AttackBlueUnit();
            if (changeAiState) isAiFinished = true;
            return true;
        }

        if (changeAiState) isAiFinished = true;

        if (hasMoved)
        {
            _isAiMoving = false;
            GameManager.Instance.ResumeAiCoroutine(1f);
        }

        return false;
    }

    private void AttackBlueUnit()
    {
        Unit _tempBlueUnit = _blueUnitsToAttack[Random.Range(0, _blueUnitsToAttack.Count)];

        if (hasAttacked == false)
        {
            if (faction  == Faction.Evil &&
                unitType == UnitType.Spirit)
            {
                _magicVfx.transform.position = _tempBlueUnit.transform.position;
                _magicVfx.Play();
                _tempBlueUnit._defenseDamage = 0;
            }

            PlayAttackSound();
            Attack(_tempBlueUnit);
            ResetIcons();
        }
    }
    
    public void AIMove()
    {
        Vector2 tilePosition;

        if (movementActionPoints == 0)
        {
            //DOTween.KillAll();
            SetToIdleAnimation(true);
            return;
        }

        if (AIArcherCheckForEnemies())
        {
            movementActionPoints = 0;
            SetToIdleAnimation(true);
            return;
        }

        if (targetTile.transform.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y,
                                               transform.localScale.z);
        }
        else if (targetTile.transform.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y,
                                               transform.localScale.z);
        }
        
        _healthGameobject.transform.localScale =
            new Vector3(Mathf.Sign(transform.localScale.x) * Mathf.Abs(_healthGameobject.transform.localScale.x),
                        _healthGameobject.transform.localScale.y, 1f);

        if (_aiPathCheck.CheckForTarget())
        {
            SetToIdleAnimation(true);
            return;
        }
        
        _aiPathCheck.CheckForWalkablePath();

        foreach (Tile tile in _aiPathCheck.walkableTiles)
        {
            tilePosition = tile.transform.position;

            if (targetTile.transform.position.x > transform.position.x && tilePosition.x > transform.position.x)
            {
                AiMoveOnX(tilePosition);
                movementActionPoints--;
                return;
            }
            else if (targetTile.transform.position.x < transform.position.x && tilePosition.x < transform.position.x)
            {
                AiMoveOnX(tilePosition);
                movementActionPoints--;
                return;
            }

            if (targetTile.transform.position.y < transform.position.y && tilePosition.y < transform.position.y && _hasMovedUp == false)
            {
                _hasMovedDown = true;
                AiMoveOnY(tilePosition);
                movementActionPoints--;
                return;
            }
            else if (targetTile.transform.position.y > transform.position.y && tilePosition.y > transform.position.y && _hasMovedDown == false)
            {
                _hasMovedUp = true;
                AiMoveOnY(tilePosition);
                movementActionPoints--;
                return;
            }
            else if (targetTile.transform.position == transform.position)
            {
                SetToIdleAnimation(true);
                movementActionPoints = 0;
                return;
            }
        }

        movementActionPoints = 0;
        SetToIdleAnimation(true);
    }

    private void AiMoveOnX(Vector2 destinationTile)
    {
        transform.DOMoveX(destinationTile.x,
                          _moveSpeedPerTile * Mathf.Abs(transform.position.x - destinationTile.x))
                 .SetEase(Ease.Linear).OnComplete(AIMove);
        
        GameManager.Instance.MoveStatsPanel(this);
    }
    
    private void AiMoveOnY(Vector2 destinationTile)
    {
        transform.DOMoveY(destinationTile.y,
                          _moveSpeedPerTile * Mathf.Abs(transform.position.y - destinationTile.y))
                 .SetEase(Ease.Linear).OnComplete(AIMove);
        
        GameManager.Instance.MoveStatsPanel(this);
    }

    #endregion

    public int        GetHealth()           { return _health > 0 ? _health : 0; }
    public int        GetAttackDamage()     { return _attackDamage; }
    public int        GetArmor()            { return _armor; }
    public int        GetDefenseDamage()    { return _defenseDamage; }
    public float      GetMoveSpeedPerTile() { return _moveSpeedPerTile; }
    public Animator   GetAnimator()         { return _animator; }
    public GameObject GetHealthGameObject() { return _healthGameobject; }
    public AIPathCheck GetAIPathCheck()     { return _aiPathCheck; }
    public GameObject GetDeathVFX() { return _deathVfx; }
    public bool GetIsStationary() { return _isStationary; }

    public void SetHealth(int addAmount) { _health = _initHealth + addAmount; }

    public void SetAIPathCheck(AIPathCheck value) { _aiPathCheck = value; }
    public void SetIsStationary(bool value) { _isStationary = value; }

    public Village GetVillage() { return _village; }

    public void ResetDefenseDamage() { _defenseDamage = _initDefenseDamage; }

    public void UpdateKingHealthUi()
    {
        switch (faction)
        {
            case Faction.Good:
                GameCanvas.Instance.UpdateBlueKingHealth(_health);
                break;
            case Faction.Evil:
                GameCanvas.Instance.UpdateDarkKingHealth(_health);
                break;
        }
    }

    public void PlayEmoteSound()
    {
        if (faction == Faction.Good)
        {
            switch (unitType)
            {
                case UnitType.Soldier:
                    EazySoundManager.PlaySound(AudioController.Instance.blueSoldierSfx);
                    break;
                case UnitType.Archer:
                    EazySoundManager.PlaySound(AudioController.Instance.blueArcherSfx);
                    break;
                case UnitType.Spirit:
                    EazySoundManager.PlaySound(AudioController.Instance.blueScoutSfx);
                    break;
                case UnitType.King:
                    EazySoundManager.PlaySound(AudioController.Instance.blueKingSfx);
                    break;
                case UnitType.Settlement:
                    EazySoundManager.PlaySound(AudioController.Instance.blueSettlementSfx, .3f);
                    break;
            }
        }
        else if (faction == Faction.Evil)
        {
            switch (unitType)
            {
                case UnitType.Soldier:
                    EazySoundManager.PlaySound(AudioController.Instance.darkSoldierSfx);
                    break;
                case UnitType.Archer:
                    EazySoundManager.PlaySound(AudioController.Instance.darkArcherSfx);
                    break;
                case UnitType.Spirit:
                    EazySoundManager.PlaySound(AudioController.Instance.darkScoutSfx);
                    break;
                case UnitType.King:
                    EazySoundManager.PlaySound(AudioController.Instance.darkKingSfx);
                    break;
                case UnitType.Settlement:
                    EazySoundManager.PlaySound(AudioController.Instance.darkSettlementSfx);
                    break;
            }
        }
    }

    public void PlayAttackSound()
    {
        if (faction == Faction.Good)
        {
            switch (unitType)
            {
                case UnitType.Soldier:
                    EazySoundManager.PlaySound(AudioController.Instance.blueSoldierAttackSfx);
                    break;
                case UnitType.Spirit:
                    EazySoundManager.PlaySound(AudioController.Instance.blueScoutAttackSfx);
                    break;
                case UnitType.King:
                    EazySoundManager.PlaySound(AudioController.Instance.blueKingAttackSfx);
                    break;
            }
        }
        else if (faction == Faction.Evil)
        {
            switch (unitType)
            {
                case UnitType.Soldier:
                    EazySoundManager.PlaySound(AudioController.Instance.darkSoldierAttackSfx);
                    break;
                case UnitType.Spirit:
                    EazySoundManager.PlaySound(AudioController.Instance.darkScoutAttackSfx);
                    break;
                case UnitType.King:
                    EazySoundManager.PlaySound(AudioController.Instance.darkKingAttackSfx);
                    break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _tileCheckRadius);
        Gizmos.DrawWireCube(transform.position, _cubeSizeVertical);
        Gizmos.DrawWireCube(transform.position, _cubeSizeHorizontal);
    }
}