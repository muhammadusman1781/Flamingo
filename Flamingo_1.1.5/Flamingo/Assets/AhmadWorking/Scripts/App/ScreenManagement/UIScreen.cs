using UnityEngine;
// AwaisDev: Base component for UI panels managed by ScreenManager

public class UIScreen : MonoBehaviour
{
    public ScreenId screenId = ScreenId.None;
    public bool IsVisible { get; private set; }
    [Tooltip("Optional hook invoked when screen becomes visible")] public UnityEngine.Events.UnityEvent onShown;
    [Tooltip("Optional hook invoked when screen is hidden")] public UnityEngine.Events.UnityEvent onHidden;

    public virtual void Show()
    {
        if (gameObject != null && !gameObject.activeSelf) gameObject.SetActive(true);
        IsVisible = true;
        OnShown();
        if (onShown != null) onShown.Invoke();
    }

    public virtual void Hide()
    {
        if (gameObject != null && gameObject.activeSelf) gameObject.SetActive(false);
        IsVisible = false;
        OnHidden();
        if (onHidden != null) onHidden.Invoke();
    }

    protected virtual void OnShown() { }
    protected virtual void OnHidden() { }
}


