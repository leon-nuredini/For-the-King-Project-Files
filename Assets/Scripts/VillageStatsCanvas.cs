using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class VillageStatsCanvas : MonoBehaviour
{
    private static VillageStatsCanvas _instance;
    public static  VillageStatsCanvas Instance => _instance;

    [BoxGroup("Text Elements")]
    [SerializeField] private TextMeshProUGUI _healthTmp, _goldPerTurnTmp, _levelTmp;

    [HideInInspector] public CanvasGroup canvasGroup;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);

        canvasGroup                             = GetComponent<CanvasGroup>();
        ObjectHolder.Instance.villageStatsPanel = gameObject;
    }

    public void UpdateVillageStatsPanel(Unit unit, Village village)
    {
        if (unit != null)
        {
            _healthTmp.text      = unit.GetHealth().ToString();
            _goldPerTurnTmp.text = village.GetGoldPerTurn().ToString();
            _levelTmp.text       = village.currentLevel.ToString();
        }
    }
}