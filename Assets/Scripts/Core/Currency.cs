using System;
using UnityEngine;

namespace Clicker.Core
{
    [Serializable]
    public class Currency
    {
        [SerializeField] private double _value = 0;
        public event Action<double> OnChanged;

        public double Value => _value;

        public void Add(double amount)
        {
            _value += Math.Max(0, amount);
            OnChanged?.Invoke(_value);
        }

        public bool Spend(double amount)
        {
            if (_value + 1e-9 >= amount)
            {
                _value -= amount;
                OnChanged?.Invoke(_value);
                return true;
            }
            return false;
        }

        public void Set(double amount)
        {
            _value = Math.Max(0, amount);
            OnChanged?.Invoke(_value);
        }
    }
}
