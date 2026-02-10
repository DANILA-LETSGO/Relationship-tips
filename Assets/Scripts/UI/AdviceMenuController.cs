using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Clicker.Content;
using Clicker.Levels;


/// <summary>
/// Populates the Advice Menu (ScrollView content) with Advice entries.
/// Uses AdviceDatabase (if assigned) or Resources/Advices fallback.
/// Uses LevelManager state to decide whether to show shortText or longExplanation and whether title is unlocked.
/// </summary>
public class AdviceMenuController : MonoBehaviour
{
    [Header("Sources")]
    [Tooltip("Optional. If assigned, will query adviceDB.GetAdvice(index) for each index from 1..adviceCount.")]
    public AdviceDatabase adviceDB;

    [Tooltip("How many advice items to try to show (default 15).")]
    public int adviceCount = 15;

    [Header("UI")]
    public GameObject entryPrefab; // prefab with AdviceEntryUI component
    public Transform contentParent; // Scroll View content
    public Button closeButton;
    public bool startClosed = true;

    [Header("References")]
    public LevelManager levelManager;

    [Header("Options")]
    [Tooltip("0..1 threshold after which the long explanation is shown")]
    public double progressThreshold = 0.58;

    private List<GameObject> spawned = new List<GameObject>();

    void Reset()
    {
        if (levelManager == null) levelManager = FindObjectOfType<LevelManager>();
    }

    void Start()
    {
        if (adviceDB == null)
        {
            // try to auto-load AdviceDB asset placed in Resources/AdviceDB (optional)
            adviceDB = Resources.Load<AdviceDatabase>("AdviceDB");
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseMenu);
        }

        if (startClosed) gameObject.SetActive(false);

        Populate();
    }

    public void OpenMenu()
    {
        gameObject.SetActive(true);
        Populate();
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }

    public void Populate()
    {
        Clear();

        if (entryPrefab == null || contentParent == null) return;

        // Try to get advs from database or resources
        for (int i = 1; i <= adviceCount; i++)
        {
            Advice adv = null;
            if (adviceDB != null)
            {
                try { adv = adviceDB.GetAdvice(i); } catch { adv = null; }
            }

            if (adv == null)
            {
                // fallback: search in Resources/Advices for Advice.index == i
                var arr = Resources.LoadAll<Advice>("Advices");
                if (arr != null)
                {
                    foreach (var a in arr)
                    {
                        if (a != null && a.index == i) { adv = a; break; }
                    }
                }
            }

            if (adv == null) continue; // skip missing slots

            var go = Instantiate(entryPrefab, contentParent);
            spawned.Add(go);

            var entryUI = go.GetComponent<AdviceEntryUI>();
            var holder = go.GetComponent<AdviceDataHolder>();
            if (holder != null) holder.data = adv;

            bool showLong = ShouldShowLongExplanation(adv);
            bool unlockedTitle = showLong;

            if (entryUI != null)
            {
                try
                {
                    int advIndex = adv.index;
                    int current = (levelManager != null) ? levelManager.currentLevel : advIndex;

                    // advice is considered opened if level passed or explanationUnlocked for current level
                    if (current > advIndex) unlockedTitle = true;
                    else if (current == advIndex && levelManager != null && levelManager.explanationUnlocked) unlockedTitle = true;

                    // show long explanation only if opened and threshold reached (or level passed)
                    if (current > advIndex) showLong = true;
                    else if (current == advIndex && levelManager != null && levelManager.progressFraction >= progressThreshold) showLong = true;
                }
                catch { }
            }
            entryUI.Bind(adv, showLong, unlockedTitle);
        }
    }

    public void Clear()
    {
        foreach (var g in spawned) { if (g != null) Destroy(g); }
        spawned.Clear();
    }

    /// <summary>
    /// Decide whether to show long explanation for the given advice.
    /// Uses LevelManager.currentLevel, progressFraction and explanationUnlocked if available.
    /// </summary>
    public bool ShouldShowLongExplanation(Advice adv)
    {
        if (adv == null) return false;
        if (levelManager == null) return false;

        int advIndex = adv.index; // 1-based
        int current = levelManager.currentLevel;

        if (current > advIndex) return true;
        if (current < advIndex) return false;

        // current == advIndex
        if (levelManager.explanationUnlocked) return true;
        if (levelManager.progressFraction >= progressThreshold) return true;
        return false;
    }
}

