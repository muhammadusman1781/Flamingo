using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerGamesScreen : MonoBehaviour
{
    public Button StartChallengeButton;
    public Button AddFriendsButton;

    public GameObject MultiplayerScreensPrefab;
    public GameObject AddFriendsScreen;
    // Start is called before the first frame update
    void Start()
    {
        StartChallengeButton.onClick.AddListener(StartChallenge);
        AddFriendsButton.onClick.AddListener(AddFriends);
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
    // Update is called once per frame
    void Update()
    {
        
    }
}
