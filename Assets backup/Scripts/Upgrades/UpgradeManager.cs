using System;
using System.Collections.Generic;
using UnityEngine;
using Clicker.Core;

namespace Clicker.Upgrades
{
    public class UpgradeManager : MonoBehaviour
    {
        [Header("Database")]
        public Upgrade[] upgrades;

        private Dictionary<string, int> _levels = new Dictionary<string, int>();
        public UpgradeStats CurrentStats { get; private set; }

        public double TotalMultiplier => 1.0 + CurrentStats.globalMultiplier + CurrentStats.prestigeMultiplier;

        public event Action OnStatsChanged;

        private void Awake()
        {
            RecalculateStats();
        }

        public void ResetAll()
        {
            _levels.Clear();
            RecalculateStats();
            OnStatsChanged?.Invoke();
        }

        public Dictionary<string, int> GetLevelsSnapshot()
        {
            return new Dictionary<string, int>(_levels);
        }

        public void LoadLevelsSnapshot(Dictionary<string, int> snapshot)
        {
            _levels = new Dictionary<string, int>(snapshot);
            RecalculateStats();
        }

        public int GetLevel(Upgrade upg)
        {
            if (upg == null) return 0;
            if (_levels.TryGetValue(upg.id, out int lv)) return lv;
            return 0;
        }

        public double GetCost(Upgrade upg)
        {
            int lv = GetLevel(upg);
            double cost = upg.baseCost * Math.Pow(upg.costScaling, lv);
            cost *= Math.Max(0.01, 1.0 - CurrentStats.costReduction);
            return Math.Ceiling(cost);
        }

        public bool TryBuy(Upgrade upg)
        {
            int lv = GetLevel(upg);
            if (lv >= upg.maxLevel) return false;
            double cost = GetCost(upg);
            if (!GameManager.I.softCurrency.Spend(cost)) return false;
            _levels[upg.id] = lv + 1;
            RecalculateStats();
            GameManager.I.autoClicker?.ApplyStats();
            OnStatsChanged?.Invoke();
            return true;
        }

        public void RecalculateStats()
        {
            var s = UpgradeStats.Default();
            foreach (var upg in upgrades)
            {
                int lv = GetLevel(upg);
                if (lv <= 0) continue;
                double eff = upg.effectPerLevel * lv;
                if (upg.isPercent) { /* remains as fraction */ }
                switch (upg.type)
                {
                    case UpgradeType.ClickPower: s.clickPowerBonus += eff; break;
                    case UpgradeType.AutoIncome: s.autoIncomePerSecond += eff * (1.0 + s.autoEfficiency) * Math.Max(1, s.autoClickerCount); break;
                    case UpgradeType.AutoRate: s.autoClicksPerSecond += eff; break;
                    case UpgradeType.CritChance: s.critChance += eff; break;
                    case UpgradeType.CritMultiplier: s.critMultiplier += eff; break;
                    case UpgradeType.MultiClickChance: s.multiClickChance += eff; break;
                    case UpgradeType.OfflineEarnings: s.offlineEarningsBonus += eff; break;
                    case UpgradeType.GoldenClickChance: s.goldenClickChance += eff; break;
                    case UpgradeType.GoldenClickValue: s.goldenClickValue += eff; break;
                    case UpgradeType.ComboTime: s.comboTime += eff; break;
                    case UpgradeType.ComboMultiplier: s.comboMultiplier += eff; break;
                    case UpgradeType.OverclockDuration: s.overclockDuration += eff; break;
                    case UpgradeType.OverclockMultiplier: s.overclockMultiplier += eff; break;
                    case UpgradeType.CostReduction: s.costReduction += eff; break;
                    case UpgradeType.AutoClickerCount: s.autoClickerCount += (int)eff; break;
                    case UpgradeType.AutoEfficiency: s.autoEfficiency += eff; break;
                    case UpgradeType.SpawnFeverChance: s.spawnFeverChance += eff; break;
                    case UpgradeType.FeverMultiplier: s.feverMultiplier += eff; break;
                    case UpgradeType.GlobalMultiplier: s.globalMultiplier += eff; break;
                    case UpgradeType.PrestigeMultiplier: s.prestigeMultiplier += eff; break;
                }
            }
            // Baselines
            s.autoClicksPerSecond = Math.Max(0, s.autoClicksPerSecond);
            CurrentStats = s;
        }
    }
}
