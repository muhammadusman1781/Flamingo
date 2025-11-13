using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerGamesScreen : MonoBehaviour
{
    public Button StartChallengeButton;

    public GameObject MultiplayerScreensPrefab;
    // Start is called before the first frame update
    void Start()
    {
        StartChallengeButton.onClick.AddListener(StartChallenge);
    }
    void StartChallenge()
    {
        Debug.Log("StartChallenge");
        MultiplayerScreensPrefab.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
