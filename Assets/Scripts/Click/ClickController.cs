using UnityEngine;
using Clicker.Core;
using Clicker.Upgrades;

namespace Clicker.Click
{
    public class ClickController : MonoBehaviour
    {
        public double baseClickValue = 1.0;

        public void OnClick()
        {
            var gm = GameManager.I;
            var stats = gm.upgradeManager.CurrentStats;
            double value = baseClickValue * (1.0 + stats.clickPowerBonus);
            // Critical?
            if (Random.value < (float)stats.critChance)
            {
                value *= (1.0 + stats.critMultiplier);
            }
            // Multiclick chance
            int extra = 0;
            if (Random.value < (float)stats.multiClickChance) extra = 1;
            gm.AddCurrency(value);
            if (extra > 0) gm.AddCurrency(value * extra);
        }
    }
}
