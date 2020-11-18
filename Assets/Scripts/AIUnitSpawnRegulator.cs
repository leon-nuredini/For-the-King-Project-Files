using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class AIUnitSpawnRegulator : MonoBehaviour
{
    private static AIUnitSpawnRegulator instance;
    public static  AIUnitSpawnRegulator Instance => instance;

    public bool isArcherUnlocked, isScoutUnlocked;

    public int maxUnits = 3, maxArchers = 1, maxScouts = 1, maxSpawnableUnits = 1;

    [BoxGroup("Units")]
    public GameObject darkSoldier, darkArcher, darkScout;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
}