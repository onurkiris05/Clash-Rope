using System;
using UnityEngine;
using VP.Nest.Analytics;
using VP.Nest.Haptic;
using VP.Nest.UI.Currency;
using VPNest.UI.Scripts.IncrementalUI;

namespace _Main.Scripts.Managers
{
    public class EconomyManager : MonoBehaviour
    {
        [Header("General Components")] [SerializeField]
        private SoldierManager soldierManager;

        [SerializeField] private RopeManager ropeManager;
        [SerializeField] private CurrencyUI currencyUI;

        [Header("General Settings")] public int EarnedMoneyPerTurn;
        [SerializeField] private float rateIncrementValue;

        public float incomeRate = 1;

        void Start()
        {
            if (PlayerPrefs.GetInt("IsSaved") > 0)
            {
                incomeRate = PlayerPrefs.GetFloat("IncomeRate");
            }
            else
            {
                PlayerPrefs.SetFloat("IncomeRate", incomeRate);
            }

            IncrementalManager.Instance.GetUpgradeCard(UpgradeType.Add).OnCurrencyPurchase += soldierManager.AddSoldier;
            IncrementalManager.Instance.GetUpgradeCard(UpgradeType.Income).OnCurrencyPurchase += IncreaseIncome;
            IncrementalManager.Instance.GetUpgradeCard(UpgradeType.Length).OnCurrencyPurchase +=
                ropeManager.IncreaseRopeLength;
        }

        public void IncreaseWallet(int value)
        {
            currencyUI.AddMoney((int)(value * incomeRate), false);
            EarnedMoneyPerTurn += (int)(value * incomeRate);
        }

        public void Set()
        {
            EarnedMoneyPerTurn = 0;
        }

        void IncreaseIncome()
        {
            incomeRate += rateIncrementValue;
            PlayerPrefs.SetFloat("IncomeRate", incomeRate);
            HapticManager.Haptic(HapticType.MediumImpact);
            AnalyticsManager.CustomEvent("income_upgraded",0);
        }
    }
}