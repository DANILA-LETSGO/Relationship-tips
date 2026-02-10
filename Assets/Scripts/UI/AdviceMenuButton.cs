using Clicker.UI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AdviceMenuButton : MonoBehaviour
{
    public AdviceMenuController controller;

    void Reset()
    {
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClick);
        }
    }

    void Awake()
    {
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClick);
        }
    }

    public void OnClick()
    {
        if (controller != null)
            controller.OpenMenu();
    }
}
