#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Clicker.UI;

public static class AddAutoIncomeUIText
{
    [MenuItem("Tools/Clicker/Add Auto Income UI")]
    public static void AddAutoIncome()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found. Create UI Canvas first (Tools -> Clicker -> Build Scene & Prefabs).");
            return;
        }

        var go = new GameObject("AutoIncomeText", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(canvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.02f, 0.95f);
        rt.anchorMax = new Vector2(0.4f, 0.99f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var txt = go.GetComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 22;
        txt.alignment = TextAnchor.MiddleLeft;
        txt.color = Color.white;
        txt.text = "Auto: +0 /sec";

        var comp = go.AddComponent<AutoIncomeUI>();
        comp.autoIncomeText = txt;

        EditorUtility.SetDirty(go);
        Debug.Log("AutoIncomeText created and AutoIncomeUI attached.");
    }
}
#endif
