using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private static AIController instance;
    public static  AIController Instance => instance;

    private Unit  _targetUnit, _currentDarkUnit;
    private Tile  _targetTile;
    private float _shortestDistanceToEnemyUnit, _distanceBetweenUnits, _shortestReachableTileDistance, _distanceToTile, _unitExecutionDuration;

    private List<Unit> _enemyUnits = new List<Unit>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void StartAiTurn() { StartCoroutine(ExecuteActions()); }

    private IEnumerator ExecuteActions()
    {
        GetAllEnemyUnits();
        
        _shortestDistanceToEnemyUnit   = Mathf.Infinity;
        _shortestReachableTileDistance = Mathf.Infinity;

        foreach (Unit enemyUnit in _enemyUnits)
        { 
            _currentDarkUnit = enemyUnit;
            
            foreach (Unit unit in ObjectHolder.Instance.units)
            {
                if (unit.faction == Unit.Faction.Good)
                {
                    _distanceBetweenUnits = Vector2.Distance(_currentDarkUnit.transform.position, unit.transform.position);

                    if (_distanceBetweenUnits < _shortestDistanceToEnemyUnit)
                    {
                        if (unit.unitReach.reachableTiles.Count > 0)
                        {
                            _targetUnit                  = unit;
                            _shortestDistanceToEnemyUnit = _distanceBetweenUnits;
                            GetNearestReachableTile();
                            _currentDarkUnit.target = _targetUnit;
                        }
                    }
                }
                
                yield return new WaitForSeconds(0f);
            }
        }
    }

    private void GetAllEnemyUnits()
    {
        _enemyUnits.Clear();
        
        foreach (Unit unit in ObjectHolder.Instance.units)
        {
            if (unit.faction == Unit.Faction.Evil && unit.unitType != Unit.UnitType.Settlement)
                _enemyUnits.Add(unit);
        }
    }

    private void GetNearestReachableTile()
    {
        foreach (Tile tile in _targetUnit.unitReach.reachableTiles)
        {
            _distanceToTile = Vector2.Distance(_currentDarkUnit.transform.position, tile.transform.position);

            if (_distanceToTile < _shortestReachableTileDistance)
            {
                _shortestReachableTileDistance = _distanceToTile;
                _targetTile = tile;
            }
        }
    }
    
//    public void Move(Vector2 destination)
//    {
//        _currentDarkUnit.PlayEmoteSound();
//        AnimatorParam.SetParams(_currentDarkUnit.GetAnimator(), AnimatorParams.Walk, AnimatorParams.AnimParamType.Trigger);
//
//        _currentDarkUnit.hasMoved = true;
//        GameManager.Instance.ResetTiles();
//
//        if (destination.x > _currentDarkUnit.transform.position.x)
//            _currentDarkUnit.transform.localScale = new Vector3(Mathf.Abs(_currentDarkUnit.transform.localScale.x), _currentDarkUnit.transform.localScale.y,
//                                               _currentDarkUnit.transform.localScale.z);
//        else if (destination.x < _currentDarkUnit.transform.position.x)
//            _currentDarkUnit.transform.localScale = new Vector3(-Mathf.Abs(_currentDarkUnit.transform.localScale.x), _currentDarkUnit.transform.localScale.y,
//                                               _currentDarkUnit.transform.localScale.z);
//
//        _currentDarkUnit.GetHealthGameObject().transform.localScale =
//            new Vector3(Mathf.Sign(transform.localScale.x) * Mathf.Abs(_currentDarkUnit.GetHealthGameObject().transform.localScale.x),
//                        _currentDarkUnit.GetHealthGameObject().transform.localScale.y, 1f);
//
//        _currentDarkUnit.transform.DOMoveX(destination.x,
//                          _currentDarkUnit.GetMoveSpeedPerTile() * Mathf.Abs(transform.position.x - destination.x))
//                 .SetEase(Ease.Linear).OnComplete(() => _currentDarkUnit.UpdatePositionY(destination));
//
//        GameManager.Instance.MoveStatsPanel(_currentDarkUnit);
//    }
}