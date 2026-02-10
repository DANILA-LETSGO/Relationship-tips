using System;
using UnityEngine;

namespace Clicker.Levels
{
    /// <summary>
    /// LevelManager (no sublevels)
    /// - totalLevels: number of advice levels (1..N)
    /// - baseCoinsForLevel, coinsScaling: determines coins required for each level
    /// Progress is based on GameManager.totalEarned (cumulative), so spending does not reduce progress.
    /// When progress for a level reaches 58% the explanation unlocks.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Header("Levels")]
        public int totalLevels = 15;
        public double baseCoinsForLevel = 100.0;
        public double coinsScaling = 2.0; // each next level requires * scaling
        public int startLevel = 1;

        public int currentLevel { get; private set; } = 1;
        public double progressFraction { get; private set; } = 0.0;
        public bool explanationUnlocked { get; private set; } = false;

        /// <summary>
        /// Called when progress changes: (levelIndex (1-based), progressFraction [0..1], explanationUnlocked)
        /// </summary>
        public event Action<int, double, bool> OnProgressChanged;
        /// <summary>
        /// Called once when explanation becomes unlocked for a level (levelIndex 1-based).
        /// </summary>
        public event Action<int> OnExplanationUnlocked;

        private double _totalEarnedObserved = 0.0;

        private void Start()
        {
            currentLevel = Mathf.Clamp(startLevel, 1, totalLevels);
            explanationUnlocked = false;
            progressFraction = 0.0;

            if (Clicker.Core.GameManager.I != null)
            {
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
            // 1-based levelIndex
            return baseCoinsForLevel * Math.Pow(coinsScaling, Math.Max(0, levelIndex - 1));
        }

        private void RecalculateProgress()
        {
            // Fast-forward through fully completed levels if totalEarned surpasses multiple thresholds
            // Compute coins required to reach the start of the current level
            double coinsBefore = 0.0;
            for (int i = 1; i < currentLevel; i++)
                coinsBefore += CoinsForLevel(i);

            // Advance levels while we have enough totalEarned to skip the current level entirely
            while (currentLevel < totalLevels && _totalEarnedObserved >= coinsBefore + CoinsForLevel(currentLevel))
            {
                // move to next level
                coinsBefore += CoinsForLevel(currentLevel);
                currentLevel = Math.Min(totalLevels, currentLevel + 1);
                // reset unlocked flag for new level
                explanationUnlocked = false;
            }

            double coinsThisLevel = CoinsForLevel(currentLevel);
            double coinsInto = Math.Max(0.0, _totalEarnedObserved - coinsBefore);
            double frac = coinsThisLevel > 0 ? Math.Min(1.0, coinsInto / coinsThisLevel) : 0.0;
            progressFraction = frac;

            // Unlock explanation at 58%
            if (!explanationUnlocked && progressFraction >= 0.58)
            {
                explanationUnlocked = true;
                OnExplanationUnlocked?.Invoke(currentLevel);
            }

            OnProgressChanged?.Invoke(currentLevel, progressFraction, explanationUnlocked);
        }

        /// <summary>
        /// Force recalculation (use after loading / inspector changes)
        /// </summary>
        public void ForceRecalculate()
        {
            _totalEarnedObserved = Clicker.Core.GameManager.I != null ? Clicker.Core.GameManager.I.totalEarned : _totalEarnedObserved;
            RecalculateProgress();
        }
    }
}
