using UnityEngine;
using UnityEngine.UI;
using Clicker.Upgrades;

namespace Clicker.UI
{
    public class UpgradeUIEntry : MonoBehaviour
    {
        public Text title;
        public Text desc;
        public Text cost;
        public Text level;
        public Button buyButton;
        [HideInInspector] public Upgrade upgrade;
        private UpgradeManager _mgr;

        public void Bind(Upgrade upg, UpgradeManager mgr)
        {
            upgrade = upg;
            _mgr = mgr;
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(Buy);
            Refresh();
        }

        public void Refresh()
        {
            if (upgrade == null || _mgr == null) return;
            title.text = upgrade.displayName;
            desc.text = upgrade.description;
            level.text = "Lv " + _mgr.GetLevel(upgrade);
            cost.text = _mgr.GetLevel(upgrade) >= upgrade.maxLevel ? "MAX" : $"{_mgr.GetCost(upgrade):0}";
        }

        private void Buy()
        {
            if (_mgr.TryBuy(upgrade)) Refresh();
        }
    }
}
