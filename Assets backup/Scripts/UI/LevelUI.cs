using UnityEngine;
using UnityEngine.UI;
using Clicker.Levels;
using Clicker.Content;

namespace Clicker.UI
{
    public class LevelUI : MonoBehaviour
    {
        public LevelManager levelManager;

        public Text levelText;
        public Text sublevelText;
        public Image progressFill;
        public Text progressPercentText;
        public GameObject explanationPanel;
        public Text explanationTitle;
        public Text explanationBody;

        // Reference to Advice DB (will be loaded from Resources if null)
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

            // Ensure the progressFill is configured correctly so it visually fills
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

            // Make sure the image uses Filled mode so fillAmount works
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = 0;
            progressFill.fillAmount = 0f;

            // Make sure rect transforms occupy the full area of their parent so fill is visible
            var rt = progressFill.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 1);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }

        private void OnProgressChanged(int lvl, int sub, double frac, bool unlocked)
        {
            if (levelText != null) levelText.text = $"Совет {lvl}";
            if (sublevelText != null) sublevelText.text = $"Подуровень {sub+1}/{levelManager.sublevelsPerLevel}";
            if (progressFill != null) progressFill.fillAmount = (float)frac;
            if (progressPercentText != null) progressPercentText.text = $"{(int)(frac*100)}%";
            if (explanationPanel != null && !explanationPanel.activeSelf) explanationPanel.SetActive(false);

            // If explanation already unlocked, show content from AdviceDB
            if (unlocked)
            {
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
            }
        }

        private void OnExplanationUnlocked(int lvl)
        {
            // Load advice (if not already)
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
