using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class AIPathCheck : MonoBehaviour
{
    [BoxGroup("Check Size")]
    [SerializeField] private Vector3 _cubeVerticalSize   = new Vector3(.5f, 3f,  0f),
                                     _cubeHorizontalSize = new Vector3(3f,  .5f, 0f);

    [BoxGroup("Check Layer")]
    [SerializeField] private LayerMask _tileLayer, _obstacleLayer;

    [BoxGroup("Tiles")]
    [SerializeField] private Collider2D[] _tileColliders;
    
    [BoxGroup("Tiles")]
    public List<Tile> walkableTiles = new List<Tile>();
    
    [BoxGroup("Targets")]
    [SerializeField] private Collider2D[] _enemyColliders;

    private Unit _unit, _targetUnit;
    private Tile _tempTile;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
        _unit.SetAIPathCheck(this);
    }

    public void CheckForWalkablePath()
    {
        CheckForTarget();
        
        if (walkableTiles.Count > 0)
            walkableTiles.Clear();

        _tileColliders = Physics2D.OverlapBoxAll(transform.position, _cubeVerticalSize, 0f, _tileLayer);

        foreach (Collider2D col in _tileColliders)
        {
            _tempTile = col.gameObject.GetComponent<Tile>();
            if (_tempTile.IsClear()) walkableTiles.Add(_tempTile);
        }
        
        _tileColliders = Physics2D.OverlapBoxAll(transform.position, _cubeHorizontalSize, 0f, _tileLayer);

        foreach (Collider2D col in _tileColliders)
        {
            _tempTile = col.gameObject.GetComponent<Tile>();
            if (_tempTile.IsClear()) walkableTiles.Add(_tempTile);
        }
    }

    public bool CheckForTarget()
    {
        _enemyColliders = Physics2D.OverlapBoxAll(transform.position, _cubeVerticalSize, 0f, _obstacleLayer);
        
        foreach (Collider2D col in _enemyColliders)
        {
            if (col.gameObject.GetComponent<Unit>())
            {
                _targetUnit = col.gameObject.GetComponent<Unit>();
                if (_targetUnit.faction == Unit.Faction.Good || _targetUnit.faction == Unit.Faction.Neutral)
                    if (_targetUnit.Equals(_unit.target))
                        return true;
            }
        }
        
        _enemyColliders = Physics2D.OverlapBoxAll(transform.position, _cubeHorizontalSize, 0f, _obstacleLayer);

        foreach (Collider2D col in _enemyColliders)
        {
            if (col.gameObject.GetComponent<Unit>())
            {
                _targetUnit = col.gameObject.GetComponent<Unit>();
                if (_targetUnit.faction == Unit.Faction.Good || _targetUnit.faction == Unit.Faction.Neutral)
                    if (_targetUnit.Equals(_unit.target))
                        return true;
            }
        }
        
        return false;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, _cubeVerticalSize);
        Gizmos.DrawWireCube(transform.position, _cubeHorizontalSize);
    }
}