using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScreensManager : MonoBehaviour
{
    public GameObject HomeScreen;
    public GameObject ProfileScreen;
    public GameObject AddFriendsScreen;
    public GameObject friendrequestsScreen;
    public GameObject challengeFriendsScreen;
    public GameObject MultiplayerGamesScreen;
    public GameObject LobbyScreen;
    public GameObject GameEndScreen;
    public GameObject LeaderboardScreen;    
    public GameObject ChallengeOtherPlayersScreen;
    public List<GameObject> Screens;

    public static UIScreensManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void HideAllScreens()
    {
        foreach (GameObject screen in Screens)
        {
            screen.SetActive(false);
        }
    }

    public void SwitchToMultiplayerGamesScreen()
    {
        HideAllScreens();
        if (MultiplayerGamesScreen != null)
        {
            MultiplayerGamesScreen.SetActive(true);
        }
    }
}
