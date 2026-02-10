#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Clicker.Core;
using Clicker.Click;
using Clicker.Auto;
using Clicker.Upgrades;
using Clicker.UI;
using System.IO;
using System.Linq;

public static class ClickerAutoSetup
{
    [MenuItem("Tools/Clicker/Build Scene & Prefabs")]
    public static void BuildEverything()
    {
        // Ensure textures are sprites
        string[] artPaths = AssetDatabase.FindAssets("", new[] {"Assets/Art"})
            .Select(AssetDatabase.GUIDToAssetPath).ToArray();
        foreach (var p in artPaths)
        {
            var ti = AssetImporter.GetAtPath(p) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spritePixelsPerUnit = 100;
                ti.SaveAndReimport();
            }
        }

        // Create Game Root
        var root = new GameObject("GameRoot");
        var gm = root.AddComponent<GameManager>();
        var upgMgr = root.AddComponent<UpgradeManager>();
        var auto = root.AddComponent<AutoClicker>();
        gm.upgradeManager = upgMgr;
        gm.autoClicker = auto;

        // Create Upgrades ScriptableObjects if absent
        var upgradeAssets = CreateOrLoadUpgrades();
        upgMgr.upgrades = upgradeAssets;

        // Canvas UI
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        // Currency Text
        var currencyGO = CreateText("CurrencyText", canvas.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -80), "0", 64, TextAnchor.MiddleCenter);
        var currencyUI = currencyGO.AddComponent<CurrencyUI>();
        currencyUI.currencyText = currencyGO.GetComponent<Text>();

        // Click button
        var clickBtn = CreateButton("ClickButton", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 200), "CLICK");
        var clickCtrl = clickBtn.AddComponent<ClickController>();
        clickBtn.GetComponent<Button>().onClick.AddListener(clickCtrl.OnClick);

        // Scroll view for upgrades
        var scrollRoot = new GameObject("UpgradesScroll", typeof(Image));
        scrollRoot.transform.SetParent(canvas.transform, false);
        var sr = scrollRoot.GetComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.1f, 0.05f);
        sr.anchorMax = new Vector2(0.9f, 0.45f);
        sr.offsetMin = sr.offsetMax = Vector2.zero;

        var scroll = CreateScroll(scrollRoot.transform);
        var list = scroll.gameObject.AddComponent<UpgradeUIList>();
        list.upgradeManager = upgMgr;

        // Entry prefab
        var entry = CreateEntryPrefab();
        list.entryPrefab = entry;
        list.content = scroll.content;

        // Assign sprites if exist
        var btnTex = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/button.png");
        var bgTex = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/background.png");
        if (btnTex != null) clickBtn.GetComponent<Image>().sprite = btnTex;
        if (bgTex != null) canvasGO.AddComponent<Image>().sprite = bgTex;

        // Save Scene
        var scenePath = "Assets/Clicker_AutoScene.unity";
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(), scenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Clicker scene built at " + scenePath);
    }

    private static ScrollRect CreateScroll(Transform parent)
    {
        var go = new GameObject("ScrollView", typeof(Image), typeof(Mask), typeof(ScrollRect));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>(); img.color = new Color(1,1,1,0.1f);
        var mask = go.GetComponent<Mask>(); mask.showMaskGraphic = false;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0,0);
        rect.anchorMax = new Vector2(1,1);
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(go.transform, false);
        var cRect = content.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0,1);
        cRect.anchorMax = new Vector2(1,1);
        cRect.pivot = new Vector2(0.5f, 1);
        cRect.sizeDelta = new Vector2(0, 1000);

        var layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.spacing = 8;
        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sr = go.GetComponent<ScrollRect>();
        sr.content = cRect;
        sr.horizontal = false;

        return sr;
    }

    private static GameObject CreateEntryPrefab()
    {
        var go = new GameObject("UpgradeEntry", typeof(Image));
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 160);

        var title = CreateText("Title", go.transform, new Vector2(0,1), new Vector2(1,1), new Vector2(0,-24), "Upgrade", 32, TextAnchor.UpperLeft);
        var desc = CreateText("Desc", go.transform, new Vector2(0,1), new Vector2(1,1), new Vector2(0,-68), "Description", 20, TextAnchor.UpperLeft);
        var cost = CreateText("Cost", go.transform, new Vector2(0,0), new Vector2(0,0), new Vector2(16,16), "0", 24, TextAnchor.LowerLeft);
        var lvl = CreateText("Level", go.transform, new Vector2(1,0), new Vector2(1,0), new Vector2(-16,16), "Lv 0", 24, TextAnchor.LowerRight);

        var btnGO = CreateButton("BuyButton", go.transform, new Vector2(1,0.5f), new Vector2(1,0.5f), new Vector2(-120,0), "BUY");
        var entry = go.AddComponent<Clicker.UI.UpgradeUIEntry>();
        entry.title = title.GetComponent<Text>();
        entry.desc = desc.GetComponent<Text>();
        entry.cost = cost.GetComponent<Text>();
        entry.level = lvl.GetComponent<Text>();
        entry.buyButton = btnGO.GetComponent<Button>();

        // Save as prefab
        var path = "Assets/Prefabs";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, "UpgradeEntry.prefab"));
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, assetPathAndName);
        GameObject.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, string text)
    {
        var go = new GameObject(name, typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 200);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPos;
        var t = CreateText("Text", go.transform, new Vector2(0,0), new Vector2(1,1), Vector2.zero, text, 36, TextAnchor.MiddleCenter);
        return go;
    }

    private static GameObject CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, string text, int size, TextAnchor anchor)
    {
        var go = new GameObject(name, typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPos;
        rect.offsetMin = new Vector2(16, 16);
        rect.offsetMax = new Vector2(-16, -16);
        var label = go.GetComponent<Text>();
        label.text = text;
        label.fontSize = size;
        label.alignment = anchor;
        label.color = Color.white;
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return go;
    }

    private static Upgrade[] CreateOrLoadUpgrades()
    {
        var dir = "Assets/Resources/Upgrades";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var list = new System.Collections.Generic.List<Upgrade>();

        void Add(string id, string name, string desc, UpgradeType type, double baseCost, double scaling, int maxLv, double eff, bool pct=false)
        {
            var path = Path.Combine(dir, id + ".asset");
            var asset = AssetDatabase.LoadAssetAtPath<Upgrade>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<Upgrade>();
                asset.id = id;
                asset.displayName = name;
                asset.description = desc;
                asset.type = type;
                asset.baseCost = baseCost;
                asset.costScaling = scaling;
                asset.maxLevel = maxLv;
                asset.effectPerLevel = eff;
                asset.isPercent = pct;
                AssetDatabase.CreateAsset(asset, path);
            }
            list.Add(asset);
        }

        // 20+ Upgrades (includes multiple auto-click variants)
        Add("click_power_1", "Сильный клик", "+1 к клику за уровень", UpgradeType.ClickPower, 10, 1.15, 999, 1);
        Add("click_power_2", "Мощный палец", "+5 к клику за уровень", UpgradeType.ClickPower, 250, 1.18, 999, 5);

        Add("auto_income_1", "Автокликер", "+0.5 в секунду", UpgradeType.AutoIncome, 50, 1.16, 999, 0.5);
        Add("auto_income_2", "Смазать шестерёнки", "+2 в секунду", UpgradeType.AutoIncome, 500, 1.18, 999, 2);
        Add("auto_rate_1", "Ускоритель кликов", "+0.5 клика/сек", UpgradeType.AutoRate, 200, 1.17, 200, 0.5);

        Add("crit_chance", "Крит-шанс", "+2% за уровень", UpgradeType.CritChance, 400, 1.2, 25, 0.02, true);
        Add("crit_mult", "Крит-множитель", "+50% за уровень", UpgradeType.CritMultiplier, 800, 1.22, 20, 0.5, true);
        Add("multi_click", "Двойной клик", "+3% шанс", UpgradeType.MultiClickChance, 600, 1.2, 50, 0.03, true);

        Add("offline", "Оффлайн-доход", "+10% к оффлайну", UpgradeType.OfflineEarnings, 1000, 1.24, 50, 0.10, true);

        Add("golden_chance", "Золотой клик (шанс)", "+1% шанс", UpgradeType.GoldenClickChance, 1500, 1.26, 50, 0.01, true);
        Add("golden_value", "Золотой клик (ценность)", "+100 за уровень", UpgradeType.GoldenClickValue, 2000, 1.28, 100, 100);

        Add("combo_time", "Комбо-время", "+0.5с", UpgradeType.ComboTime, 1800, 1.23, 50, 0.5);
        Add("combo_mult", "Комбо-множитель", "+5%/уровень", UpgradeType.ComboMultiplier, 2200, 1.24, 50, 0.05, true);

        Add("oc_time", "Оверклок (время)", "+1с", UpgradeType.OverclockDuration, 2500, 1.25, 30, 1.0);
        Add("oc_mult", "Оверклок (множ.)", "+10%/уровень", UpgradeType.OverclockMultiplier, 2600, 1.3, 30, 0.10, true);

        Add("cost_reduce", "Скидка мастера", "-1% цена/уровень", UpgradeType.CostReduction, 3000, 1.28, 50, 0.01, true);

        Add("auto_count", "Мульти-автокликер", "+1 автокликер", UpgradeType.AutoClickerCount, 4000, 1.35, 100, 1);
        Add("auto_eff", "Эффективность авто", "+10% доход авто", UpgradeType.AutoEfficiency, 4500, 1.32, 100, 0.10, true);

        Add("fever_chance", "Шанс лихорадки", "+1% шанс", UpgradeType.SpawnFeverChance, 5000, 1.33, 50, 0.01, true);
        Add("fever_mult", "Множитель лихорадки", "+10% множитель", UpgradeType.FeverMultiplier, 5200, 1.33, 50, 0.10, true);

        Add("global_mult", "Глобальный множитель", "+5% ко всему", UpgradeType.GlobalMultiplier, 6000, 1.36, 100, 0.05, true);
        Add("prestige_mult", "Престижный бонус", "+10%/уровень", UpgradeType.PrestigeMultiplier, 10000, 1.4, 50, 0.10, true);

        AssetDatabase.SaveAssets();
        return list.ToArray();
    }
}
#endif
