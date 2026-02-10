using UnityEngine;
using UnityEngine.UI;
using Clicker.Core;

namespace Clicker.UI
{
    public class CurrencyUI : MonoBehaviour
    {
        public Text currencyText;

        private void Start()
        {
            GameManager.I.softCurrency.OnChanged += OnChanged;
            OnChanged(GameManager.I.softCurrency.Value);
        }

        private void OnDestroy()
        {
            if (GameManager.I != null)
                GameManager.I.softCurrency.OnChanged -= OnChanged;
        }

        private void OnChanged(double v)
        {
            currencyText.text = $"{v:0}";
        }
    }
}
