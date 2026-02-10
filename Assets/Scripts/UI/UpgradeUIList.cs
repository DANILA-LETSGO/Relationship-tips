using UnityEngine;
using UnityEngine.UI;
using Clicker.Upgrades;
using System.Collections.Generic;

namespace Clicker.UI
{
    public class UpgradeUIList : MonoBehaviour
    {
        public UpgradeManager upgradeManager;
        public RectTransform content;
        public GameObject entryPrefab;

        private List<UpgradeUIEntry> _entries = new List<UpgradeUIEntry>();

        private void Start()
        {
            Populate();
            upgradeManager.OnStatsChanged += RefreshAll;
        }

        private void OnDestroy()
        {
            if (upgradeManager != null)
                upgradeManager.OnStatsChanged -= RefreshAll;
        }

        public void Populate()
        {
            foreach (Transform t in content) Destroy(t.gameObject);
            _entries.Clear();
            foreach (var upg in upgradeManager.upgrades)
            {
                var go = Instantiate(entryPrefab, content);
                var e = go.GetComponent<UpgradeUIEntry>();
                e.Bind(upg, upgradeManager);
                _entries.Add(e);
            }
        }

        public void RefreshAll()
        {
            foreach (var e in _entries) e.Refresh();
        }
    }
}
