using UnityEngine;

public class BackAction : MonoBehaviour
{
    public void Back()
    {
        if (ScreenManager.Instance == null) return;
        ScreenManager.Instance.Back();
    }
}


