#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Clicker.UI;

public static class CreateLockedUpgradeHintPopupPrefabEditor
{
    [MenuItem("Tools/Clicker/Create Locked Upgrade Hint Popup Prefab")]
    public static void CreatePrefab()
    {
        const string resourcesDir = "Assets/Resources";
        const string prefabsDir = "Assets/Resources/ClickerPrefabs";
        const string prefabPath = "Assets/Resources/ClickerPrefabs/LockedUpgradeHintPopup.prefab";

        if (!Directory.Exists(resourcesDir)) Directory.CreateDirectory(resourcesDir);
        if (!Directory.Exists(prefabsDir)) Directory.CreateDirectory(prefabsDir);

        var root = new GameObject("LockedUpgradeHintPopup", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(LockedUpgradeHintPopup));
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.18f);
        rect.anchorMax = new Vector2(0.5f, 0.18f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(760f, 120f);

        var bg = root.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.78f);
        bg.raycastTarget = false;

        var group = root.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textGO.transform.SetParent(root.transform, false);
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20f, 14f);
        textRect.offsetMax = new Vector2(-20f, -14f);

        var text = textGO.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontSize = 24;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.text = "Подсказка о скрытом улучшении";

        var popup = root.GetComponent<LockedUpgradeHintPopup>();
        var so = new SerializedObject(popup);
        so.FindProperty("_group").objectReferenceValue = group;
        so.FindProperty("_text").objectReferenceValue = text;
        so.ApplyModifiedPropertiesWithoutUndo();

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (prefab != null)
        {
            Selection.activeObject = prefab;
            Debug.Log("[Clicker] Created popup prefab: " + prefabPath);
        }
        else
        {
            Debug.LogError("[Clicker] Failed to create popup prefab.");
        }
    }
}
#endif
