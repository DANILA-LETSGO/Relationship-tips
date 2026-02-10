using UnityEngine;

namespace Clicker.Upgrades
{
    public struct UpgradeStats
    {
        public double clickPowerBonus;
        public double autoIncomePerSecond;
        public double autoClicksPerSecond;
        public double critChance;
        public double critMultiplier;
        public double multiClickChance;
        public double offlineEarningsBonus;
        public double goldenClickChance;
        public double goldenClickValue;
        public double comboTime;
        public double comboMultiplier;
        public double overclockDuration;
        public double overclockMultiplier;
        public double costReduction; // as fraction, e.g., 0.1 => 10%
        public int autoClickerCount;
        public double autoEfficiency; // multiplier to auto income
        public double spawnFeverChance;
        public double feverMultiplier;
        public double globalMultiplier;
        public double prestigeMultiplier;

        public static UpgradeStats Default()
        {
            var s = new UpgradeStats();
            s.autoClicksPerSecond = 1;
            s.autoIncomePerSecond = 0;
            return s;
        }
    }
}
