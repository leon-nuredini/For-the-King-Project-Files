using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitStatsCanvas : MonoBehaviour
{
    private static UnitStatsCanvas _instance;
    public static  UnitStatsCanvas Instance => _instance;

    [BoxGroup("Text Elements")]
    [SerializeField] private TextMeshProUGUI _healthTmp, _attackDamageTmp, _armorTmp, _defenseDamageTmp;

    [HideInInspector] public CanvasGroup canvasGroup;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);

        canvasGroup                      = GetComponent<CanvasGroup>();
        ObjectHolder.Instance.statsPanel = gameObject;
    }

    public void UpdateStatsPanel(Unit unit)
    {
        if (unit != null)
        {
            _healthTmp.text        = unit.GetHealth().ToString();
            _attackDamageTmp.text  = unit.GetAttackDamage().ToString();
            _armorTmp.text         = unit.GetArmor().ToString();
            _defenseDamageTmp.text = unit.GetDefenseDamage().ToString();
        }
    }
}