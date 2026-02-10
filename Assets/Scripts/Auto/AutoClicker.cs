using UnityEngine;
using Clicker.Core;
using Clicker.Upgrades;

namespace Clicker.Auto
{
    public class AutoClicker : MonoBehaviour
    {
        private double _timer;
        private double _period = 1.0;

        private double _autoPerTick = 0.0;

        public void ApplyStats()
        {
            var s = GameManager.I.upgradeManager.CurrentStats;
            _period = Mathf.Max(0.05f, (float)(1.0 / s.autoClicksPerSecond));
            _autoPerTick = s.autoIncomePerSecond / (1.0 / _period);
        }

        private void Update()
        {
            var s = GameManager.I.upgradeManager.CurrentStats;
            double pps = s.autoClicksPerSecond;
            if (pps <= 0) return;

            _timer += Time.deltaTime;
            while (_timer >= _period)
            {
                _timer -= _period;
                GameManager.I.AddCurrency(_autoPerTick);
            }
        }
    }
}
