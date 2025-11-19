using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BottomBar : MonoBehaviour
{
    public Button HomeButton;
    public Button ProfileButton;
    public Button LeaderboardButton;
    public Button SettingsButton;
    // Start is called before the first frame update
    void Start()
    {
        HomeButton.onClick.AddListener(Home);
        ProfileButton.onClick.AddListener(Profile);
        LeaderboardButton.onClick.AddListener(Leaderboard);
        SettingsButton.onClick.AddListener(Settings);
    }
    void Home()
    {
        Debug.Log("Home");
        UIScreensManager.Instance.HideAllScreens();
        UIScreensManager.Instance.HomeScreen.SetActive(true);
    }
    void Profile()
    {
        Debug.Log("Profile");
        UIScreensManager.Instance.HideAllScreens();
        UIScreensManager.Instance.ProfileScreen.SetActive(true);
    }
    void Leaderboard()
    {
        Debug.Log("Leaderboard");
        UIScreensManager.Instance.HideAllScreens();
        UIScreensManager.Instance.LeaderboardScreen.SetActive(true);
    }
    void Settings()
    {
        Debug.Log("Settings");
    }
}
