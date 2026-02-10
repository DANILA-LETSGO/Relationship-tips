using UnityEngine;

namespace Clicker.Upgrades
{
    [CreateAssetMenu(menuName = "Clicker/Upgrade", fileName = "Upgrade")]
    public class Upgrade : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public UpgradeType type;
        public double baseCost = 10;
        public double costScaling = 1.15;
        public int maxLevel = 9999;
        public double effectPerLevel = 1.0; // meaning depends on type (additive or multiplicative step)
        public bool isPercent; // if true, treat effect as percentage (e.g., 0.05 => +5% per level)
    }
}
