#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CreateAdviceOpenerEditor
{
    [MenuItem("Tools/Clicker/Create Advice Opener Button")]
    public static void CreateOpener()
    {
        // Find canvas
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Create a Canvas in the scene first.");
            return;
        }

        // Find AdviceMenuPanel in scene
        var panel = GameObject.Find("AdviceMenuPanel");
        if (panel == null)
        {
            Debug.LogError("AdviceMenuPanel not found in the scene. Create it first using Tools->Clicker->Create Advice Menu.");
            return;
        }

        var controller = panel.GetComponent<AdviceMenuController>();
        if (controller == null)
        {
            Debug.LogError("AdviceMenuPanel does not have AdviceMenuController attached.");
            return;
        }

        // Create button
        var btnGO = new GameObject("AdviceOpenButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(canvas.transform, false);
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(120, 40);
        rt.anchoredPosition = new Vector2(-80, -20);

        var img = btnGO.GetComponent<Image>();
        img.color = new Color(1f,1f,1f,0.9f);

        var textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textGO.transform.SetParent(btnGO.transform, false);
        var trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var txt = textGO.GetComponent<Text>();
        txt.text = "Советы";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.color = Color.black;

        // Add AdviceMenuButton component and wire controller
        var adviceBtn = btnGO.AddComponent<AdviceMenuButton>();
        adviceBtn.controller = controller;

        // Select created object
        Selection.activeGameObject = btnGO;

        Debug.Log("Advice opener button created and linked to AdviceMenuPanel.");
    }
}
#endif
