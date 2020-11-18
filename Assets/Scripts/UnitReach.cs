using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitReach : MonoBehaviour
{
    [SerializeField] private Vector3 _cubeSizeVertical, _cubeSizeHorizontal;

    private Unit _unit;
    
    [SerializeField] private Collider2D[] _colliders;
    public List<Tile> reachableTiles = new List<Tile>();

    [SerializeField] private LayerMask _tileLayer;

    private Tile _tempTile;

    private void Start()
    {
        _unit = GetComponentInParent<Unit>();
        _unit.unitReach = this;
    }

    public void GetNearbyReachableTiles()
    {
        reachableTiles.Clear();
        _colliders = Physics2D.OverlapBoxAll(transform.position, _cubeSizeVertical, 0f, _tileLayer);

        foreach (Collider2D col in _colliders)
        {
            _tempTile = col.gameObject.GetComponent<Tile>();
            if (_tempTile.IsClear()) reachableTiles.Add(_tempTile);
        }
        
        _colliders = Physics2D.OverlapBoxAll(transform.position, _cubeSizeHorizontal, 0f, _tileLayer);

        foreach (Collider2D col in _colliders)
        {
            _tempTile = col.gameObject.GetComponent<Tile>();
            if (_tempTile.IsClear()) reachableTiles.Add(_tempTile);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, _cubeSizeVertical);
        Gizmos.DrawWireCube(transform.position, _cubeSizeHorizontal);
    }
}
