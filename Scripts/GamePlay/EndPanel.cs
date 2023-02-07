using System;
using System.Collections;
using System.Collections.Generic;
using _Main.Scripts.Managers;
using TMPro;
using UnityEngine;

public class EndPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI earnedMoneyText;

    private EnemyManager _enemyManager;
    private EconomyManager _economyManager;

    void Awake()
    {
        _enemyManager = FindObjectOfType<EnemyManager>();
        _economyManager = FindObjectOfType<EconomyManager>();
    }

    void OnEnable()
    {
        killCountText.text = $"{_enemyManager.KillCountPerTurn}";
        earnedMoneyText.text = $"{_economyManager.EarnedMoneyPerTurn}";
    }
}
