#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Clicker.Content;

public class CreateAdviceMenuEditor
{
    [MenuItem("Tools/Clicker/Create Advice Menu (UI + sample Advices)")]
    public static void CreateMenu()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Create a Canvas in the scene first.");
            return;
        }

        // Create panel
        var panelGO = new GameObject("AdviceMenuPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelGO.transform.SetParent(canvas.transform, false);
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.1f, 0.1f);
        panelRT.anchorMax = new Vector2(0.9f, 0.9f);
        panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

        var img = panelGO.GetComponent<Image>();
        img.color = new Color(0f,0f,0f,0.6f);

        // Close button
        var closeGO = new GameObject("CloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeGO.transform.SetParent(panelGO.transform, false);
        var closeRT = closeGO.GetComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1f, 1f);
        closeRT.anchorMax = new Vector2(1f, 1f);
        closeRT.sizeDelta = new Vector2(28,28);
        closeRT.anchoredPosition = new Vector2(-18, -18);
        var closeImg = closeGO.GetComponent<Image>();
        closeImg.color = Color.white;
        var closeBtn = closeGO.GetComponent<Button>();

        // Title
        var titleGO = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        titleGO.transform.SetParent(panelGO.transform, false);
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot = new Vector2(0.5f,1f);
        titleRT.sizeDelta = new Vector2(0,40);
        titleRT.anchoredPosition = new Vector2(0,-10);
        var titleText = titleGO.GetComponent<Text>();
        titleText.text = "Советы";
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 24;
        titleText.color = Color.white;

        // Scroll view (basic)
        var scrollGO = new GameObject("ScrollView", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask), typeof(ScrollRect));
        scrollGO.transform.SetParent(panelGO.transform, false);
        var scrollRT = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0.02f, 0.02f);
        scrollRT.anchorMax = new Vector2(0.98f, 0.9f);
        scrollRT.offsetMin = scrollRT.offsetMax = Vector2.zero;
        var scrollImage = scrollGO.GetComponent<Image>();
        scrollImage.color = new Color(1f,1f,1f,0.02f);
        var mask = scrollGO.GetComponent<Mask>();
        mask.showMaskGraphic = false;
        var scroll = scrollGO.GetComponent<ScrollRect>();

        // Content
        var contentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGO.transform.SetParent(scrollGO.transform, false);
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f,1f);
        contentRT.anchorMax = new Vector2(1f,1f);
        contentRT.pivot = new Vector2(0.5f,1f);
        contentRT.anchoredPosition = new Vector2(0,0);
        contentRT.sizeDelta = new Vector2(0,0);

        var vlg = contentGO.GetComponent<VerticalLayoutGroup>();
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = true;
        var csf = contentGO.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRT;

        // Entry prefab
        var entryGO = new GameObject("AdviceEntryPrefab", typeof(RectTransform));
        var entryRT = entryGO.GetComponent<RectTransform>();
        entryRT.sizeDelta = new Vector2(0,80);
        var bg = entryGO.AddComponent<Image>();
        bg.color = new Color(1f,1f,1f,0.03f);
        var titleChild = new GameObject("Title", typeof(RectTransform), typeof(Text));
        titleChild.transform.SetParent(entryGO.transform, false);
        var tRT = titleChild.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0f,0.5f);
        tRT.anchorMax = new Vector2(1f,1f);
        tRT.offsetMin = new Vector2(10,10);
        tRT.offsetMax = new Vector2(-10,-40);
        var ttl = titleChild.GetComponent<Text>();
        ttl.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        ttl.fontSize = 16;
        ttl.alignment = TextAnchor.UpperLeft;
        ttl.color = Color.white;
        ttl.text = "Advice Title";

        var explChild = new GameObject("Explanation", typeof(RectTransform), typeof(Text));
        explChild.transform.SetParent(entryGO.transform, false);
        var eRT = explChild.GetComponent<RectTransform>();
        eRT.anchorMin = new Vector2(0f,0f);
        eRT.anchorMax = new Vector2(1f,0.5f);
        eRT.offsetMin = new Vector2(10,10);
        eRT.offsetMax = new Vector2(-10,20);
        var explTxt = explChild.GetComponent<Text>();
        explTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        explTxt.fontSize = 14;
        explTxt.alignment = TextAnchor.UpperLeft;
        explTxt.color = Color.white;
        explTxt.text = "Explanation...";

        // Lock overlay (simple)
        var lockGO = new GameObject("LockOverlay", typeof(RectTransform), typeof(Image));
        lockGO.transform.SetParent(entryGO.transform, false);
        var lockRT = lockGO.GetComponent<RectTransform>();
        lockRT.anchorMin = Vector2.zero;
        lockRT.anchorMax = Vector2.one;
        lockRT.offsetMin = lockRT.offsetMax = Vector2.zero;
        var lockImg = lockGO.GetComponent<Image>();
        lockImg.color = new Color(0f,0f,0f,0.6f);

        // Add AdviceEntryUI component
        var entryComp = entryGO.AddComponent<AdviceEntryUI>();
        entryComp.titleText = ttl;
        entryComp.explanationText = explTxt;
        entryComp.lockOverlay = lockGO;

        // Save the prefab to Assets/Resources/ClickerPrefabs/AdviceEntryPrefab.prefab
        string dir = "Assets/Resources/ClickerPrefabs";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string prefabPath = dir + "/AdviceEntryPrefab.prefab";
        var prefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(entryGO, prefabPath);

        // Create AdviceMenuController on panel
        var controller = panelGO.AddComponent<AdviceMenuController>();
        controller.entryPrefab = prefab;
        controller.contentParent = contentRT;
        controller.closeButton = closeBtn;

        // Create sample AdviceData assets under Resources/Advices if not exists
        string advDir = "Assets/Resources/Advices";
        if (!Directory.Exists(advDir)) Directory.CreateDirectory(advDir);

        for (int i = 1; i <= 15; i++)
        {
            string path = advDir + "/Advice_" + i + ".asset";
            if (!File.Exists(path))
            {
                var asset = ScriptableObject.CreateInstance<Advice>();
                asset.title = "Совет " + i;
                asset.longExplanation = "Объяснение " + i;
                asset.index = i - 1; // zero-based index
                AssetDatabase.CreateAsset(asset, path);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Advice menu created. Prefab saved to " + prefabPath + " and sample advices created under Resources/Advices.");
    }
}
#endif
