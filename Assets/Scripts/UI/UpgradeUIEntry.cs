using UnityEngine;
using UnityEngine.UI;
using Clicker.Upgrades;
using Clicker.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Clicker.UI
{
    /// <summary>
    /// Upgrade UI entry that creates a visual clone on the root Canvas (not inside the ScrollRect)
    /// so the animation isn't clipped by masks. Only Graphics listed in flashGraphics are flashed.
    /// The animation can be interrupted: if a new purchase happens while animation plays,
    /// the running animation is stopped, the clone is destroyed and a new animation starts.
    /// Also makes the BuyButton image fully transparent on Bind().
    /// Fix v5: ensure the clone does NOT block pointer events (so original button remains clickable)
    /// and keep original canvas group blocksRaycasts = true so it can receive clicks while hidden.
    /// </summary>
    public class UpgradeUIEntry : MonoBehaviour
    {
        [Header("UI Elements")]
        public Text title;
        public Text desc;
        public Text cost;
        public Text level;
        public Button buyButton;

        [Header("Upgrade (bound at runtime)")]
        [HideInInspector] public Upgrade upgrade;
        private UpgradeManager _mgr;

        [Header("Animation (purchase)")]
        public RectTransform rootTransform; // if null, will use this RectTransform
        [Tooltip("Graphics on the original that will flash on the clone (optional).")]
        public Graphic[] flashGraphics;
        public float purchaseScale = 1.12f;
        public float scaleUpDuration = 0.09f;
        public float scaleDownDuration = 0.28f;
        public Color flashColor = new Color(1f, 0.9f, 0.4f, 1f);
        public float flashDuration = 0.25f;
        public int overlaySortingOrder = 10000;

        // Internal
        private Coroutine _animCoroutine;
        private GameObject _currentClone;
        private CanvasGroup _originalCanvasGroup;

        public void Bind(Upgrade upg, UpgradeManager mgr)
        {
            upgrade = upg;
            _mgr = mgr;
            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(Buy);

                // MAKE BUY BUTTON IMAGE FULLY TRANSPARENT (alpha = 0)
                var img = buyButton.GetComponent<Image>();
                if (img != null)
                {
                    Color c = img.color;
                    c.a = 0f;
                    img.color = c;
                }
            }

            if (_mgr != null) _mgr.OnStatsChanged += Refresh;
            if (GameManager.I != null) GameManager.I.softCurrency.OnChanged += OnCurrencyChanged;

            // ensure we have a root transform
            if (rootTransform == null)
            {
                rootTransform = GetComponent<RectTransform>();
            }

            // cache or add canvas group to original to hide while animating
            _originalCanvasGroup = GetComponent<CanvasGroup>();
            if (_originalCanvasGroup == null)
                _originalCanvasGroup = gameObject.AddComponent<CanvasGroup>();

            Refresh();
        }

        private void OnDestroy()
        {
            if (_mgr != null) _mgr.OnStatsChanged -= Refresh;
            if (GameManager.I != null) GameManager.I.softCurrency.OnChanged -= OnCurrencyChanged;

            // cleanup clone if any
            if (_currentClone != null)
            {
                Destroy(_currentClone);
                _currentClone = null;
            }
        }

        private void OnCurrencyChanged(double v)
        {
            Refresh();
        }

        public void Refresh()
        {
            if (upgrade == null || _mgr == null)
                return;

            int lv = _mgr.GetLevel(upgrade);
            bool isMax = lv >= upgrade.maxLevel;
            double upgradeCost = isMax ? 0.0 : _mgr.GetCost(upgrade);

            // Determine index to know whether this is one of the first two upgrades
            int idx = -1;
            if (_mgr.upgrades != null)
            {
                for (int i = 0; i < _mgr.upgrades.Length; i++)
                {
                    if (_mgr.upgrades[i] == upgrade) { idx = i; break; }
                }
            }

            bool canAfford = (GameManager.I != null) && (GameManager.I.softCurrency.Value + 1e-9 >= upgradeCost);
            bool alwaysReveal = (idx >= 0 && idx < 2);
            bool reveal = alwaysReveal || lv > 0 || canAfford;

            if (!reveal && lv == 0)
            {
                if (title != null) title.text = "??";
                if (desc != null) desc.text = "??";
            }
            else
            {
                if (title != null) title.text = upgrade.displayName;
                if (desc != null) desc.text = upgrade.description;
            }

            if (level != null) level.text = "Lv " + lv;
            if (cost != null) cost.text = isMax ? "MAX" : $"{upgradeCost:0}";

            if (buyButton != null)
            {
                buyButton.interactable = !isMax && canAfford;
            }
        }

        private void Buy()
        {
            if (_mgr == null || upgrade == null) return;

            bool success = _mgr.TryBuy(upgrade);
            // If success is true, we animate. If not, we do nothing (you might still want feedback).
            if (success)
            {
                // Interrupt current animation if running and clean up clone/restore original
                InterruptAnimationIfRunning();

                // play animation on overlay clone (parented to root Canvas)
                StartPurchaseAnimationOverlay();
                Refresh();
            }
        }

        // Interrupt running animation: stop coroutine, destroy clone, restore original
        private void InterruptAnimationIfRunning()
        {
            if (_animCoroutine != null)
            {
                try { StopCoroutine(_animCoroutine); } catch { }
                _animCoroutine = null;
            }

            if (_currentClone != null)
            {
                // destroy clone immediately
                Destroy(_currentClone);
                _currentClone = null;
            }

            if (_originalCanvasGroup != null)
            {
                // Keep blocksRaycasts true so original remains clickable while animation runs
                _originalCanvasGroup.alpha = 1f;
                _originalCanvasGroup.blocksRaycasts = true;
                _originalCanvasGroup.interactable = true;
            }
        }

        //----------------------------------------------------
        // Overlay clone animation helpers
        //----------------------------------------------------

        // Find the root Canvas the original is under (walk up parents)
        private Canvas FindRootCanvas()
        {
            if (rootTransform == null) return null;
            Transform t = rootTransform.transform;
            while (t != null)
            {
                var c = t.GetComponent<Canvas>();
                if (c != null && c.isRootCanvas)
                    return c;
                t = t.parent;
            }

            // fallback: scene's first Canvas
            return GameObject.FindObjectOfType<Canvas>();
        }

        // Get a relative path from ancestor to child (excluding ancestor)
        private string GetPathRelative(Transform ancestor, Transform child)
        {
            if (ancestor == null || child == null) return null;
            List<string> parts = new List<string>();
            Transform cur = child;
            while (cur != null && cur != ancestor && cur != cur.root)
            {
                parts.Add(cur.name);
                cur = cur.parent;
            }
            if (cur != ancestor) return null; // not a descendant
            parts.Reverse();
            return string.Join("/", parts);
        }

        public void StartPurchaseAnimationOverlay()
        {
            if (_animCoroutine != null)
            {
                // Ensure any previous animation is cleaned up before starting a new one
                InterruptAnimationIfRunning();
            }
            _animCoroutine = StartCoroutine(PurchaseAnimationOverlayCoroutine());
        }

        private IEnumerator PurchaseAnimationOverlayCoroutine()
        {
            // find root canvas for original element (so clone remains in same canvas)
            Canvas rootCanvas = FindRootCanvas();
            if (rootCanvas == null)
            {
                yield break;
            }

            // instantiate clone under rootCanvas.transform (so it's not inside the ScrollRect)
            var originalRT = rootTransform;
            if (originalRT == null) yield break;

            GameObject cloneGO = GameObject.Instantiate(originalRT.gameObject, rootCanvas.transform);
            cloneGO.name = originalRT.gameObject.name + "_AnimClone";
            _currentClone = cloneGO;

            // Add CanvasGroup to clone to make sure it does NOT block raycasts (so original button remains clickable)
            var cloneCanvasGroup = cloneGO.GetComponent<CanvasGroup>();
            if (cloneCanvasGroup == null) cloneCanvasGroup = cloneGO.AddComponent<CanvasGroup>();
            cloneCanvasGroup.blocksRaycasts = false;
            cloneCanvasGroup.interactable = false;

            // Also ensure any Graphic on clone does not block raycasts as extra safety
            var allGraphics = cloneGO.GetComponentsInChildren<Graphic>(true);
            foreach (var g in allGraphics) if (g != null) g.raycastTarget = false;

            // Remove interactive components on clone (Button, UpgradeUIEntry etc.)
            var buttons = cloneGO.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons) Destroy(b);
            var upgradeEntries = cloneGO.GetComponentsInChildren<UpgradeUIEntry>(true);
            foreach (var ue in upgradeEntries) Destroy(ue);

            // Remove Layout / Mask components from clone so it renders freely
            var masks = cloneGO.GetComponentsInChildren<Mask>(true);
            foreach (var m in masks) Destroy(m);
            var layoutGroups = cloneGO.GetComponentsInChildren<UnityEngine.UI.LayoutGroup>(true);
            foreach (var lg in layoutGroups) Destroy(lg);
            var layoutElements = cloneGO.GetComponentsInChildren<UnityEngine.UI.LayoutElement>(true);
            foreach (var le in layoutElements) Destroy(le);

            // compute world center of original to position the clone
            Vector3[] worldCorners = new Vector3[4];
            originalRT.GetWorldCorners(worldCorners);
            Vector3 worldCenter = (worldCorners[0] + worldCorners[2]) * 0.5f;

            // convert to canvas local point
            RectTransform canvasRT = rootCanvas.GetComponent<RectTransform>();
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldCenter);
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenPoint, null, out localPoint);

            RectTransform cloneRT = cloneGO.GetComponent<RectTransform>();
            cloneRT.SetParent(rootCanvas.transform, false);
            cloneRT.anchorMin = cloneRT.anchorMax = new Vector2(0.5f, 0.5f);
            cloneRT.pivot = new Vector2(0.5f, 0.5f);
            cloneRT.anchoredPosition = localPoint;
            // match size
            Vector2 size = new Vector2(originalRT.rect.width, originalRT.rect.height);
            cloneRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            cloneRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

            // Build list of corresponding clone graphics that match original flashGraphics by relative path
            List<Graphic> cloneFlashGraphics = new List<Graphic>();
            if (flashGraphics != null && flashGraphics.Length > 0)
            {
                foreach (var orig in flashGraphics)
                {
                    if (orig == null) continue;
                    string path = GetPathRelative(originalRT.transform, orig.transform);
                    Graphic matched = null;
                    if (!string.IsNullOrEmpty(path))
                    {
                        Transform found = cloneGO.transform.Find(path);
                        if (found != null) matched = found.GetComponent<Graphic>();
                    }
                    // fallback: try to find by name in clone children
                    if (matched == null)
                    {
                        var byName = cloneGO.GetComponentsInChildren<Graphic>(true);
                        foreach (var g in byName)
                        {
                            if (g != null && g.name == orig.name) { matched = g; break; }
                        }
                    }
                    if (matched != null) cloneFlashGraphics.Add(matched);
                }
            }

            // cache original colors for cloneFlashGraphics
            Color[] originalColors = new Color[cloneFlashGraphics.Count];
            for (int i = 0; i < cloneFlashGraphics.Count; i++)
                originalColors[i] = cloneFlashGraphics[i].color;

            // hide original (use CanvasGroup so layout doesn't change)
            if (_originalCanvasGroup != null)
            {
                _originalCanvasGroup.alpha = 0f;
                // IMPORTANT: leave blocksRaycasts = true so original button continues to receive clicks
                _originalCanvasGroup.blocksRaycasts = true;
                _originalCanvasGroup.interactable = true;
            }

            // animate clone: scale up -> flash -> scale down -> destroy clone -> restore original
            Vector3 originalScale = cloneRT.localScale;
            Vector3 targetScale = originalScale * purchaseScale;

            // scale up
            float t = 0f;
            while (t < scaleUpDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / scaleUpDuration);
                float s = Mathf.SmoothStep(0f, 1f, k);
                cloneRT.localScale = Vector3.LerpUnclamped(originalScale, targetScale, s);
                yield return null;
            }
            cloneRT.localScale = targetScale;

            // flash start: set to flashColor only for selected cloneFlashGraphics
            for (int i = 0; i < cloneFlashGraphics.Count; i++)
            {
                if (cloneFlashGraphics[i] != null)
                    cloneFlashGraphics[i].color = Color.Lerp(originalColors[i], flashColor, 0.9f);
            }

            // scale down with slight overshoot
            t = 0f;
            while (t < scaleDownDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / scaleDownDuration);
                float s = 1f - Mathf.Pow(1f - k, 3f);
                float overshoot = Mathf.Sin(k * Mathf.PI) * 0.08f;
                cloneRT.localScale = Vector3.LerpUnclamped(targetScale, originalScale * (1f + overshoot), s);
                yield return null;
            }
            cloneRT.localScale = originalScale;

            // restore colors over flashDuration for selected cloneFlashGraphics
            float tt = 0f;
            while (tt < flashDuration)
            {
                tt += Time.unscaledDeltaTime;
                float kk = Mathf.Clamp01(tt / flashDuration);
                for (int i = 0; i < cloneFlashGraphics.Count; i++)
                {
                    if (cloneFlashGraphics[i] != null)
                        cloneFlashGraphics[i].color = Color.Lerp(Color.Lerp(originalColors[i], flashColor, 0.9f), originalColors[i], kk);
                }
                yield return null;
            }
            // ensure colors restored exactly
            for (int i = 0; i < cloneFlashGraphics.Count; i++)
            {
                if (cloneFlashGraphics[i] != null) cloneFlashGraphics[i].color = originalColors[i];
            }

            // destroy clone and restore original
            if (_currentClone == cloneGO) _currentClone = null;
            Destroy(cloneGO);
            if (_originalCanvasGroup != null)
            {
                _originalCanvasGroup.alpha = 1f;
                _originalCanvasGroup.blocksRaycasts = true;
                _originalCanvasGroup.interactable = true;
            }

            _animCoroutine = null;
        }
    }
}
