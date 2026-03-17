using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Clicker.UI
{
    /// <summary>
    /// Minimal runtime popup used to explain why hidden upgrades are still locked.
    /// </summary>
    public class LockedUpgradeHintPopup : MonoBehaviour
    {
        private static LockedUpgradeHintPopup _instance;
        private const string PopupPrefabPath = "ClickerPrefabs/LockedUpgradeHintPopup";

        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Text _text;
        private Coroutine _routine;

        public static void Show(string message, float seconds = 2f)
        {
            if (_instance == null)
            {
                _instance = CreateInstance();
            }

            if (_instance == null) return;
            _instance.ShowInternal(message, seconds);
        }

        private static LockedUpgradeHintPopup CreateInstance()
        {
            Canvas targetCanvas = FindObjectOfType<Canvas>();
            if (targetCanvas == null) return null;

            var prefab = Resources.Load<GameObject>(PopupPrefabPath);
            if (prefab != null)
            {
                var instance = Instantiate(prefab, targetCanvas.transform, false);
                var popupFromPrefab = instance.GetComponent<LockedUpgradeHintPopup>();
                if (popupFromPrefab != null)
                {
                    popupFromPrefab.EnsureReferences();
                    return popupFromPrefab;
                }
            }

            GameObject root = new GameObject("LockedUpgradeHintPopup", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            root.transform.SetParent(targetCanvas.transform, false);

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

            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(root.transform, false);
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(20f, 14f);
            textRect.offsetMax = new Vector2(-20f, -14f);

            var txt = textGO.GetComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontSize = 24;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;

            var popup = root.AddComponent<LockedUpgradeHintPopup>();
            popup._group = group;
            popup._text = txt;
            return popup;
        }

        private void EnsureReferences()
        {
            if (_group == null) _group = GetComponent<CanvasGroup>();
            if (_text == null) _text = GetComponentInChildren<Text>(true);
        }

        private void ShowInternal(string message, float seconds)
        {
            if (_text != null) _text.text = message;

            if (_routine != null)
            {
                StopCoroutine(_routine);
            }
            _routine = StartCoroutine(ShowRoutine(seconds));
        }

        private IEnumerator ShowRoutine(float seconds)
        {
            if (_group == null) yield break;
            _group.alpha = 1f;
            yield return new WaitForSeconds(Mathf.Max(0.5f, seconds));
            _group.alpha = 0f;
            _routine = null;
        }
    }
}
