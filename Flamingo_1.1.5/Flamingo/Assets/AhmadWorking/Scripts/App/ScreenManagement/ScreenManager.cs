using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    // AwaisDev: Centralized UI screen navigation with stack behavior

    public static ScreenManager Instance { get; private set; }

    [Tooltip("Assign all UIScreen components that belong to this manager")] public List<UIScreen> screens = new List<UIScreen>();
    [Tooltip("Initial screen to show on start")] public ScreenId initialScreen = ScreenId.Login;

    private readonly Dictionary<ScreenId, UIScreen> idToScreen = new Dictionary<ScreenId, UIScreen>();
    private readonly Stack<UIScreen> history = new Stack<UIScreen>();
    private UIScreen current;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        idToScreen.Clear();
        foreach (var s in screens)
        {
            if (s == null || s.screenId == ScreenId.None) continue;
            if (!idToScreen.ContainsKey(s.screenId)) idToScreen.Add(s.screenId, s);
        }
    }

    void Start()
    {
        foreach (var kv in idToScreen)
        {
            if (kv.Value != null) kv.Value.Hide();
        }
        if (initialScreen != ScreenId.None)
        {
            Show(initialScreen, false);
        }
    }

    public void Show(ScreenId id, bool addToHistory = true)
    {
        if (!idToScreen.TryGetValue(id, out var target) || target == null)
        {
            Debug.LogWarning($"ScreenManager: Screen not found {id}");
            return;
        }

        if (current == target) return;

        if (current != null)
        {
            if (addToHistory) history.Push(current);
            current.Hide();
        }

        current = target;
        current.Show();
    }

    public void SetAsRoot(ScreenId id)
    {
        history.Clear();
        Show(id, false);
    }

    public void Back()
    {
        if (history.Count == 0) return;
        var prev = history.Pop();
        if (current != null) current.Hide();
        current = prev;
        current.Show();
    }
}


