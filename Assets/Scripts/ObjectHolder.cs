using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class ObjectHolder : MonoBehaviour
{
    private static ObjectHolder _instance;
    public static  ObjectHolder Instance => _instance;

    public List<Tile>    tiles    = new List<Tile>();
    public List<Unit>    units    = new List<Unit>();
    public List<Village> villages = new List<Village>();

    public Camera     mainCamera;
    public GameObject statsPanel, villageStatsPanel;

    [BoxGroup("Damage Icons")]
    public GameObject playerDamageIcon, enemyDamageIcon;

    [BoxGroup("Unit Spawning Particles")]
    public GameObject blueSpawnParticles, darkSpawnParticles;
    
    [BoxGroup("Unit Ghosts")]
    public GameObject blueSoldierGhost, blueArcherGhost, blueScoutGhost;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);

        mainCamera = Camera.main;
    }
}