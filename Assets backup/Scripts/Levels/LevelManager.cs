using System;
using UnityEngine;

namespace Clicker.Levels
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Levels")]
        public int totalLevels = 15;
        public int sublevelsPerLevel = 3; // Подуровней на уровень
        public double baseCoinsForLevel = 100.0;
        public double coinsScaling = 2.0; // each next level requires * scaling
        public int startLevel = 1;

        public int currentLevel { get; private set; }
        public int currentSublevel { get; private set; }
        public double progressFraction { get; private set; }
        public bool explanationUnlocked { get; private set; }

        public event Action<int,int,double,bool> OnProgressChanged; // level, sublevel, progress, explanationUnlocked
        public event Action<int> OnExplanationUnlocked; // level index (1-based)

        private double _totalEarnedObserved = 0.0;

        private void Start()
        {
            currentLevel = Mathf.Clamp(startLevel, 1, totalLevels);
            currentSublevel = 0;
            explanationUnlocked = false;

            if (Clicker.Core.GameManager.I != null)
            {
                // Initialize observed total earned from gamemanager (load)
                _totalEarnedObserved = Clicker.Core.GameManager.I.totalEarned;
                Clicker.Core.GameManager.I.OnCurrencyAdded += OnCurrencyAdded;
                RecalculateProgress();
            }
        }

        private void OnDestroy()
        {
            if (Clicker.Core.GameManager.I != null)
                Clicker.Core.GameManager.I.OnCurrencyAdded -= OnCurrencyAdded;
        }

        private void OnCurrencyAdded(double amount)
        {
            _totalEarnedObserved += amount;
            RecalculateProgress();
        }

        private double CoinsForLevel(int levelIndex)
        {
            // levelIndex is 1-based
            return baseCoinsForLevel * Math.Pow(coinsScaling, Math.Max(0, levelIndex - 1));
        }

        private double CoinsForSublevel(int levelIndex)
        {
            return CoinsForLevel(levelIndex) / Math.Max(1, sublevelsPerLevel);
        }

        private void RecalculateProgress()
        {
            // compute how many coins needed to reach current level start
            double coinsBefore = 0.0;
            for (int i = 1; i < currentLevel; i++)
                coinsBefore += CoinsForLevel(i);

            // plus coins for completed sublevels within current level
            coinsBefore += currentSublevel * CoinsForSublevel(currentLevel);

            double coinsThisSublevel = CoinsForSublevel(currentLevel);
            double coinsInto = Math.Max(0.0, _totalEarnedObserved - coinsBefore);
            double frac = coinsThisSublevel > 0 ? Math.Min(1.0, coinsInto / coinsThisSublevel) : 0.0;
            progressFraction = frac;

            // Unlock explanation at 58%
            if (!explanationUnlocked && progressFraction >= 0.58)
            {
                explanationUnlocked = true;
                OnExplanationUnlocked?.Invoke(currentLevel);
            }

            // Level up if progress reached 100%
            if (progressFraction >= 1.0)
            {
                progressFraction = 0.0;
                currentSublevel++;
                explanationUnlocked = false;
                if (currentSublevel >= sublevelsPerLevel)
                {
                    currentSublevel = 0;
                    currentLevel = Math.Min(totalLevels, currentLevel + 1);
                }
            }

            OnProgressChanged?.Invoke(currentLevel, currentSublevel, progressFraction, explanationUnlocked);
        }

        // For UI or debug: force recalculation (e.g., after load)
        public void ForceRecalculate()
        {
            _totalEarnedObserved = Clicker.Core.GameManager.I != null ? Clicker.Core.GameManager.I.totalEarned : _totalEarnedObserved;
            RecalculateProgress();
        }
    }
}
