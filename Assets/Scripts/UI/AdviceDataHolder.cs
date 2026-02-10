using UnityEngine;
using Clicker.Content; // чтобы инспектор показывал правильный тип

/// <summary>
/// Хранит ссылку на Advice ассет (тип Advice используется напрямую, чтобы не ломать ссылки в инспекторе).
/// </summary>
public class AdviceDataHolder : MonoBehaviour
{
    public Advice data;
}
