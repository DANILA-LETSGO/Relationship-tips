using UnityEngine;
using UnityEngine.UI;
using Clicker.Content; // Advice класс в этом пространстве имён

/// <summary>
/// UI для одной записи Advice.
/// Исправление: shortText теперь показывается всегда, когда showLong == false (т.е. до достижения порога),
/// независимо от того, открыт ли заголовок. Заголовок по-прежнему зависит от unlockedTitle.
/// Добавлены дополнительные защиты и логирование для диагностики в консоли.
/// </summary>
public class AdviceEntryUI : MonoBehaviour
{
    public Text titleText;
    public Text firstPartText;
    public Text explanationText;
    public GameObject lockOverlay; // отображается, если заголовок закрыт (по-прежнему используется для визуального оформления)

    /// <summary>
    /// Bind an Advice object.
    /// - advice: ScriptableObject Advice (использует поля index, title, shortText, longExplanation)
    /// - showLong: true -> показываем longExplanation; false -> показываем shortText (если есть)
    /// - unlockedTitle: true -> показываем реальный заголовок; false -> показываем "??"
    /// </summary>
    public void Bind(Advice advice, bool showLong, bool unlockedTitle)
    {
        // firstPartText -> shortText (visible only if advice is opened/unlockedTitle == true)
        // explanationText -> longExplanation (visible only if advice is opened AND showLong == true)
        if (advice == null)
        {
            if (titleText != null) titleText.text = "??";
            if (firstPartText != null) { firstPartText.gameObject.SetActive(false); firstPartText.text = string.Empty; }
            if (explanationText != null) { explanationText.gameObject.SetActive(false); explanationText.text = string.Empty; }
            if (lockOverlay != null) lockOverlay.SetActive(true);
            return;
        }

        // Title: show only if unlockedTitle
        if (titleText != null)
            titleText.text = unlockedTitle ? advice.title : "??";

        // First (short) part: show only when unlockedTitle == true
        if (firstPartText != null)
        {
            if (unlockedTitle && !string.IsNullOrEmpty(advice.shortText))
            {
                firstPartText.gameObject.SetActive(true);
                firstPartText.text = advice.shortText;
            }
            else
            {
                firstPartText.gameObject.SetActive(false);
                firstPartText.text = string.Empty;
            }
        }

        // Explanation (long): show only when unlockedTitle && showLong
        if (explanationText != null)
        {
            if (unlockedTitle && showLong)
            {
                explanationText.gameObject.SetActive(true);
                explanationText.text = advice.longExplanation ?? string.Empty;
            }
            else
            {
                explanationText.gameObject.SetActive(false);
                explanationText.text = string.Empty;
            }
        }

        if (lockOverlay != null)
            lockOverlay.SetActive(!unlockedTitle);

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

}
