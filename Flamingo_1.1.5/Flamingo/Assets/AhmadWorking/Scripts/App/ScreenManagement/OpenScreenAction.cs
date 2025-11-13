using UnityEngine;
// AwaisDev: Button helper to open a ScreenId via ScreenManager

public class OpenScreenAction : MonoBehaviour
{
    public ScreenId target = ScreenId.None;
    public bool setAsRoot = false;
    public bool autoBindButtonOnEnable = true;

    void OnEnable()
    {
        if (!autoBindButtonOnEnable) return;
        var btn = GetComponent<UnityEngine.UI.Button>();
        if (btn != null)
        {
            btn.onClick.RemoveListener(Open);
            btn.onClick.AddListener(Open);
        }
    }

    public void Open()
    {
        Debug.Log($"OpenScreenAction.Open invoked on {name} → {target} (setAsRoot={setAsRoot})");
        if (ScreenManager.Instance == null)
        {
            Debug.LogWarning("OpenScreenAction: ScreenManager.Instance is null in scene");
            return;
        }
        if (target == ScreenId.None)
        {
            Debug.LogWarning("OpenScreenAction: target ScreenId is None");
            return;
        }
        if (setAsRoot)
        {
            ScreenManager.Instance.SetAsRoot(target);
        }
        else
        {
            ScreenManager.Instance.Show(target);
        }
    }
}


