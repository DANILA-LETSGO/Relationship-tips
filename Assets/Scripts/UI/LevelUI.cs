using UnityEngine;
using UnityEngine.UI;
using Clicker.Levels;
using Clicker.Content;

namespace Clicker.UI
{
    /// <summary>
    /// LevelUI for LevelManager without sublevels.
    /// Shows level number, progress bar and explanation when unlocked.
    /// </summary>
    public class LevelUI : MonoBehaviour
    {
        [Header("References")]
        public LevelManager levelManager;

        [Header("UI Elements")]
        public Text levelText;
        public Text sublevelText; // optional: we'll hide it when sublevels are not used
        public Image progressFill;
        public Text progressPercentText;

        [Header("Explanation Panel")]
        public GameObject explanationPanel;
        public Text explanationTitle;
        public Text explanationBody;

        [Header("Content")]
        public AdviceDatabase adviceDB;

        private void Start()
        {
            if (levelManager == null)
            {
                Debug.LogWarning("LevelUI: no LevelManager assigned.");
                return;
            }

            if (adviceDB == null)
            {
                adviceDB = Resources.Load<AdviceDatabase>("AdviceDB");
                if (adviceDB == null)
                {
                    Debug.LogWarning("LevelUI: AdviceDB not found in Resources/AdviceDB. Use Tools->Clicker->Create 15 Advice assets to generate placeholders.");
                }
            }

            // Hide sublevel text if present (no sublevels anymore)
            if (sublevelText != null) sublevelText.gameObject.SetActive(false);

            EnsureProgressFillSetup();

            levelManager.OnProgressChanged += OnProgressChanged;
            levelManager.OnExplanationUnlocked += OnExplanationUnlocked;
            levelManager.ForceRecalculate();
        }

        private void OnDestroy()
        {
            if (levelManager != null)
            {
                levelManager.OnProgressChanged -= OnProgressChanged;
                levelManager.OnExplanationUnlocked -= OnExplanationUnlocked;
            }
        }

        private void EnsureProgressFillSetup()
        {
            if (progressFill == null) return;

            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = 0;
            progressFill.fillAmount = 0f;

            var rt = progressFill.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 1);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }


        private void OnProgressChanged(int lvl, double frac, bool unlocked)
        {
            // Patch: restore LevelText behaviour while keeping shortText and progress updates intact.
            try
            {
                // --- (1) Обновление прогресса — НЕ МЕНЯЕМ, это уже работает ---
                try
                {
                    if (progressFill != null)
                        progressFill.fillAmount = (float)frac;
                }
                catch { }

                try
                {
                    if (progressPercentText != null)
                        progressPercentText.text = string.Format("{0:P0}", (float)frac);
                }
                catch { }

                // --- (2) Найдём Advice ассет для уровня lvl (если он есть) ---
                Advice advLocal = null;
                try
                {
                    var arr = Resources.LoadAll<Advice>("Advices");
                    if (arr != null)
                    {
                        foreach (var a in arr)
                        {
                            if (a != null && a.index == lvl)
                            {
                                advLocal = a;
                                break;
                            }
                        }
                    }
                }
                catch { advLocal = null; }

                // --- (3) Решение показывать long или short (оставляем как есть) ---
                bool showLong = false;
                int advIndex = lvl;
                int current = (levelManager != null) ? levelManager.currentLevel : advIndex;
                if (current > advIndex) showLong = true;
                else if (current == advIndex)
                {
                    if (frac >= 0.58 || (levelManager != null && levelManager.explanationUnlocked)) showLong = true;
                }

                // --- (4) Восстановление LevelText: если есть заголовок совета — показываем его,
                // иначе показываем "Level {current}" как fallback. ---
                try
                {
                    if (levelText != null)
                    {
                        if (advLocal != null)
                        {
                            // Показываем заголовок совета (раньше именно так работал LevelText)
                            levelText.text = $"Уровень {advLocal.index}";
                        }
                        else
                        {
                            // Fallback — показываем номер уровня
                            levelText.text = "Level " + current.ToString();
                        }
                    }
                }
                catch { /* не ломаем UI из-за LevelText */ }

                // --- (5) Отображение title/body объяснения — не трогаем логику shortText/longExplanation ---
                if (advLocal != null)
                {
                    if (explanationTitle != null) explanationTitle.text = advLocal.title ?? string.Empty;

                    if (explanationBody != null)
                    {
                        if (showLong)
                        {
                            explanationBody.text = advLocal.longExplanation ?? string.Empty;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(advLocal.shortText))
                                explanationBody.text = advLocal.shortText;
                            else
                                explanationBody.text = (levelManager != null && levelManager.explanationUnlocked) ? (advLocal.longExplanation ?? string.Empty) : string.Empty;
                        }
                    }
                }
                else
                {
                    if (explanationTitle != null) explanationTitle.text = string.Empty;
                    if (explanationBody != null) explanationBody.text = string.Empty;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[LevelUI Patch] OnProgressChanged exception: " + ex.Message);
            }
        }



        private void OnExplanationUnlocked(int lvl)
        {
            if (adviceDB == null) adviceDB = Resources.Load<AdviceDatabase>("AdviceDB");

            var adv = adviceDB != null ? adviceDB.GetAdvice(lvl) : null;
            if (adv != null)
            {
                if (explanationTitle != null) explanationTitle.text = adv.title;
                if (explanationBody != null) explanationBody.text = adv.longExplanation;
            }
            else
            {
                if (explanationTitle != null) explanationTitle.text = $"Объяснение {lvl}";
                if (explanationBody != null) explanationBody.text = $"Здесь будет подробное объяснение для совета {lvl} (плейсхолдер).";
            }

            if (explanationPanel != null) explanationPanel.SetActive(true);
        }
    }
}
