#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Clicker.Levels;
using Clicker.UI;

public static class ClickerLevelUIBuilder
{
    [MenuItem("Tools/Clicker/Build Level UI")]
    public static void BuildLevelUI()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene. Run Build Scene & Prefabs first.");
            return;
        }

        // Create Level Panel
        var panelGO = new GameObject("LevelPanel", typeof(Image));
        panelGO.transform.SetParent(canvas.transform, false);
        var rect = panelGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.5f);
        rect.anchorMax = new Vector2(0.9f, 0.8f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        var img = panelGO.GetComponent<Image>(); img.color = new Color(0,0,0,0.4f);

        // Level Text
        var level = CreateText("LevelText", panelGO.transform, new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(0,-24), "Совет 1", 36, TextAnchor.UpperCenter);

        // Sublevel Text
        var sub = CreateText("SublevelText", panelGO.transform, new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(0,-68), "Подуровень 1", 24, TextAnchor.UpperCenter);

        // Progress Bar background
        var progBg = new GameObject("ProgressBG", typeof(Image));
        progBg.transform.SetParent(panelGO.transform, false);
        var pRect = progBg.GetComponent<RectTransform>();
        pRect.anchorMin = new Vector2(0.1f, 0.5f);
        pRect.anchorMax = new Vector2(0.9f, 0.6f);
        pRect.offsetMin = pRect.offsetMax = Vector2.zero;
        var pImg = progBg.GetComponent<Image>(); pImg.color = new Color(0.2f,0.2f,0.2f,0.8f);

        var fillGO = new GameObject("Fill", typeof(Image));
        fillGO.transform.SetParent(progBg.transform, false);
        var fRect = fillGO.GetComponent<RectTransform>();
        fRect.anchorMin = new Vector2(0,0);
        fRect.anchorMax = new Vector2(0,1);
        fRect.offsetMin = fRect.offsetMax = Vector2.zero;
        var fImg = fillGO.GetComponent<Image>(); fImg.type = Image.Type.Filled; fImg.fillMethod = Image.FillMethod.Horizontal;
        fImg.fillAmount = 0.0f;
        fImg.color = new Color(0.8f,0.6f,0.1f,1f);

        var perc = CreateText("ProgressPercent", panelGO.transform, new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0,0), "0%", 28, TextAnchor.MiddleCenter);

        // Explanation panel (hidden by default)
        var expl = new GameObject("ExplanationPanel", typeof(Image));
        expl.transform.SetParent(panelGO.transform, false);
        var eRect = expl.GetComponent<RectTransform>();
        eRect.anchorMin = new Vector2(0.05f, 0.05f);
        eRect.anchorMax = new Vector2(0.95f, 0.45f);
        eRect.offsetMin = eRect.offsetMax = Vector2.zero;
        var eImg = expl.GetComponent<Image>(); eImg.color = new Color(0,0,0,0.6f);
        expl.SetActive(false);

        var expTitle = CreateText("ExplTitle", expl.transform, new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(0,-8), "Объяснение 1", 26, TextAnchor.UpperCenter);
        var expBody = CreateText("ExplBody", expl.transform, new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0,0), "Объяснение (подробно)...", 18, TextAnchor.UpperLeft);
        expBody.GetComponent<RectTransform>().offsetMin = new UnityEngine.Vector2(16,16);
        expBody.GetComponent<RectTransform>().offsetMax = new UnityEngine.Vector2(-16,-16);

        // Attach LevelManager to GameRoot (create if missing)
        var gmObj = GameObject.Find("GameRoot");
        if (gmObj == null)
        {
            gmObj = new GameObject("GameRoot");
            gmObj.AddComponent<Clicker.Core.GameManager>();
        }

        var lm = gmObj.GetComponent<LevelManager>();
        if (lm == null) lm = gmObj.AddComponent<LevelManager>();

        // Attach LevelUI component to panel and wire references
        var levelUI = panelGO.AddComponent<LevelUI>();
        levelUI.levelManager = lm;
        levelUI.levelText = level.GetComponent<UnityEngine.UI.Text>();
        levelUI.sublevelText = sub.GetComponent<UnityEngine.UI.Text>();
        levelUI.progressFill = fImg;
        levelUI.progressPercentText = perc.GetComponent<UnityEngine.UI.Text>();
        levelUI.explanationPanel = expl;
        levelUI.explanationTitle = expTitle.GetComponent<UnityEngine.UI.Text>();
        levelUI.explanationBody = expBody.GetComponent<UnityEngine.UI.Text>();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("Level UI created and wired. Attach LevelManager settings in GameRoot inspector if needed.");
    }

    private static GameObject CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, string text, int size, TextAnchor anchor)
    {
        var go = new GameObject(name, typeof(UnityEngine.UI.Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new UnityEngine.Vector2(0, 64);
        var label = go.GetComponent<UnityEngine.UI.Text>();
        label.text = text;
        label.fontSize = size;
        label.alignment = anchor;
        label.color = UnityEngine.Color.white;
        label.font = UnityEngine.Resources.GetBuiltinResource<UnityEngine.Font>("Arial.ttf");
        return go;
    }
}
#endif
