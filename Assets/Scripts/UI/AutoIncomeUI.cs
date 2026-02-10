using UnityEngine;
using UnityEngine.UI;
using Clicker.Core;
using Clicker.Upgrades;
using System;

namespace Clicker.UI
{
    /// <summary>
    /// Displays the current automatic income per second (what auto-clicker adds per second
    /// if the player does nothing). The value mirrors how GameManager.AddCurrency will be called
    /// by AutoClicker, using UpgradeManager.CurrentStats and GameManager multipliers.
    /// </summary>
    public class AutoIncomeUI : MonoBehaviour
    {
        [Header("UI")]
        public Text autoIncomeText;
        public string prefix = "Auto: +";
        public string suffix = " /sec";

        private UpgradeManager _upg;
        private GameManager _gm;
        private float _updateTimer = 0f;
        private float _updateInterval = 0.5f; // update twice per second

        private void Start()
        {
            _gm = GameManager.I;
            if (_gm == null)
            {
                Debug.LogWarning("AutoIncomeUI: GameManager.I is null. Ensure GameManager exists in scene.");
                return;
            }

            _upg = _gm.upgradeManager;
            if (_upg != null) _upg.OnStatsChanged += OnStatsChanged;

            UpdateTextImmediate();
        }

        private void OnDestroy()
        {
            if (_upg != null) _upg.OnStatsChanged -= OnStatsChanged;
        }

        private void OnStatsChanged()
        {
            UpdateTextImmediate();
        }

        private void Update()
        {
            _updateTimer += Time.deltaTime;
            if (_updateTimer >= _updateInterval)
            {
                _updateTimer = 0f;
                UpdateTextImmediate();
            }
        }

        private void UpdateTextImmediate()
        {
            if (autoIncomeText == null) return;
            if (_gm == null)
            {
                autoIncomeText.text = prefix + "0" + suffix;
                return;
            }

            double perSecBase = 0.0;
            if (_gm.upgradeManager != null)
                perSecBase = _gm.upgradeManager.CurrentStats.autoIncomePerSecond;

            // Compute multipliers the same way GameManager.AddCurrency does:
            double mult = (_gm.upgradeManager != null ? _gm.upgradeManager.TotalMultiplier : 1.0) * _gm.prestigeMultiplier;

            double shown = perSecBase * mult;

            autoIncomeText.text = prefix + FormatValue(shown) + suffix;
        }

        private string FormatValue(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) return "0";
            double abs = Math.Abs(v);
            if (abs < 1000) return $"{v:0.##}";
            if (abs < 1_000_000) return $"{v/1000.0:0.##}K";
            if (abs < 1_000_000_000) return $"{v/1_000_000.0:0.##}M";
            return $"{v/1_000_000_000.0:0.##}B";
        }
    }
}
