using System;
using UnityEngine;
using Clicker.Upgrades;

namespace Clicker.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        [Header("Currency")]
        public Currency softCurrency = new Currency();

        [Header("Managers")]
        public UpgradeManager upgradeManager;
        public Auto.AutoClicker autoClicker;

        [Header("Prestige")]
        public int prestige;
        public double prestigeMultiplier => 1.0 + prestige * 0.5; // +50% per prestige

        // Total currency ever gained (doesn't decrease when spent).
        public double totalEarned;

        public event Action<double> OnCurrencyAdded; // amount added (after multipliers)

        private SaveData _save;

        private void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();
        }

        private void Start()
        {
            if (upgradeManager != null) upgradeManager.RecalculateStats();
            if (autoClicker != null) autoClicker.ApplyStats();
        }

        private void OnApplicationPause(bool pause) { if (pause) SaveGame(); }
        private void OnApplicationQuit() { SaveGame(); }

        public void AddCurrency(double amount)
        {
            double mult = (upgradeManager != null ? upgradeManager.TotalMultiplier : 1.0) * prestigeMultiplier;
            double final = amount * mult;
            softCurrency.Add(final);
            totalEarned += final;
            OnCurrencyAdded?.Invoke(final);
        }

        public void SaveGame()
        {
            _save = _save ?? new SaveData();
            _save.currency = softCurrency.Value;
            _save.upgradeLevels = upgradeManager?.GetLevelsSnapshot();
            _save.prestige = prestige;
            _save.lastQuitUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _save.totalEarned = totalEarned;
            SaveSystem.Save(_save);
        }

        private void LoadGame()
        {
            _save = SaveSystem.Load();
            softCurrency.Set(_save.currency);
            prestige = _save.prestige;
            totalEarned = _save.totalEarned;

            if (upgradeManager != null && _save.upgradeLevels != null)
            {
                upgradeManager.LoadLevelsSnapshot(_save.upgradeLevels);
            }

            // Offline earnings
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long dt = Math.Max(0, now - _save.lastQuitUnixSeconds);
            if (dt > 0 && upgradeManager != null)
            {
                double perSec = upgradeManager.CurrentStats.autoIncomePerSecond;
                double offlineMult = 1.0 + upgradeManager.CurrentStats.offlineEarningsBonus;
                double earned = perSec * dt * offlineMult * prestigeMultiplier;
                AddCurrency(earned); // this will increase totalEarned and currency
            }
        }

        public void DoPrestige()
        {
            // Simple prestige: gain 1 point per 1e6 currency, reset progress
            int gained = (int)(softCurrency.Value / 1_000_000.0);
            if (gained <= 0) return;
            prestige += gained;
            softCurrency.Set(0);
            upgradeManager?.ResetAll();
            SaveGame();
        }
    }
}
