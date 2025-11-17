using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerGamesScreen : MonoBehaviour
{
    public Button StartChallengeButton;
    public Button AddFriendsButton;
    public Button LeaderboardButton;

    public GameObject MultiplayerScreensPrefab;
    public GameObject AddFriendsScreen;
    public GameObject LeaderboardScreen;
    // Start is called before the first frame update
    void Start()
    {
        StartChallengeButton.onClick.AddListener(StartChallenge);
        AddFriendsButton.onClick.AddListener(AddFriends);
        LeaderboardButton.onClick.AddListener(Leaderboard);
    }
    void StartChallenge()
    {
        Debug.Log("StartChallenge");
        MultiplayerScreensPrefab.SetActive(true);
    }
    void AddFriends()
    {
        Debug.Log("AddFriends");
        AddFriendsScreen.SetActive(true);
    }
    void Leaderboard()
    {
        Debug.Log("Leaderboard");
        LeaderboardScreen.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
