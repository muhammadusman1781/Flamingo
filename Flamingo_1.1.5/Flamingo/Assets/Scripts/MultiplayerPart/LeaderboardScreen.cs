using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RTLTMPro;

public class LeaderboardScreen : MonoBehaviour
{
    [Header("Player Name Text Fields (Rank 1-10)")]
    public RTLTextMeshPro rank1PlayerNameText;
    public RTLTextMeshPro rank2PlayerNameText;
    public RTLTextMeshPro rank3PlayerNameText;
    public RTLTextMeshPro rank4PlayerNameText;
    public RTLTextMeshPro rank5PlayerNameText;
    public RTLTextMeshPro rank6PlayerNameText;
    public RTLTextMeshPro rank7PlayerNameText;
    public RTLTextMeshPro rank8PlayerNameText;
    public RTLTextMeshPro rank9PlayerNameText;
    public RTLTextMeshPro rank10PlayerNameText;
    
    [Header("Buttons")]
    public Button backButton;
    
    [Header("Loading")]
    public GameObject loadingPanel;
    
    private ServerConstants serverConstants;
    private LeaderboardData currentLeaderboardData;
    
    private void Start()
    {
        // Get server constants reference
        if (NetworkingHandler.instance != null)
        {
            serverConstants = NetworkingHandler.instance.serverConstants;
        }
        else
        {
            Debug.LogError("NetworkingHandler instance not found!");
        }
        
        // Setup back button
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        
        // Load leaderboard data
        LoadLeaderboard();
    }
    
    private void LoadLeaderboard()
    {
        if (serverConstants == null)
        {
            Debug.LogError("ServerConstants is null!");
            return;
        }
        
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        
        string apiUrl = serverConstants.baseUrl + "/auth/leaderboard/";
        
        NetworkingHandler.instance.getMessage(
            apiUrl,
            isTokenNeeded: true,
            onSuccess: OnLeaderboardSuccess,
            onFail: OnLeaderboardFail
        );
    }
    
    private void OnLeaderboardSuccess(string response)
    {
        Debug.Log($"Leaderboard received: {response}");
        
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
        
        try
        {
            LeaderboardResponse leaderboardResponse = JsonUtility.FromJson<LeaderboardResponse>(response);
            
            if (leaderboardResponse != null && leaderboardResponse.data != null)
            {
                currentLeaderboardData = leaderboardResponse.data;
                UpdateUI();
            }
            else
            {
                Debug.LogError("Failed to parse leaderboard response");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing leaderboard: {ex.Message}");
        }
    }
    
    private void OnLeaderboardFail(string error)
    {
        Debug.LogError($"Failed to load leaderboard: {error}");
        
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
    
    private void UpdateUI()
    {
        if (currentLeaderboardData == null)
            return;
        
        // Create a combined list of all players (top 3 + remaining)
        List<PlayerRankInfo> allPlayers = new List<PlayerRankInfo>();
        
        // Add top three players
        if (currentLeaderboardData.top_three != null)
        {
            foreach (var player in currentLeaderboardData.top_three)
            {
                allPlayers.Add(new PlayerRankInfo
                {
                    player_name = player.player_name,
                    rank = player.rank
                });
            }
        }
        
        // Add remaining players
        if (currentLeaderboardData.remaining != null)
        {
            foreach (var player in currentLeaderboardData.remaining)
            {
                allPlayers.Add(new PlayerRankInfo
                {
                    player_name = player.player_name,
                    rank = player.rank
                });
            }
        }
        
        // Sort by rank to ensure correct order
        allPlayers.Sort((a, b) => a.rank.CompareTo(b.rank));
        
        // Update UI text fields for ranks 1-10
        UpdatePlayerNameText(rank1PlayerNameText, allPlayers, 1);
        UpdatePlayerNameText(rank2PlayerNameText, allPlayers, 2);
        UpdatePlayerNameText(rank3PlayerNameText, allPlayers, 3);
        UpdatePlayerNameText(rank4PlayerNameText, allPlayers, 4);
        UpdatePlayerNameText(rank5PlayerNameText, allPlayers, 5);
        UpdatePlayerNameText(rank6PlayerNameText, allPlayers, 6);
        UpdatePlayerNameText(rank7PlayerNameText, allPlayers, 7);
        UpdatePlayerNameText(rank8PlayerNameText, allPlayers, 8);
        UpdatePlayerNameText(rank9PlayerNameText, allPlayers, 9);
        UpdatePlayerNameText(rank10PlayerNameText, allPlayers, 10);
    }
    
    private void UpdatePlayerNameText(RTLTextMeshPro textComponent, List<PlayerRankInfo> players, int rank)
    {
        if (textComponent == null)
            return;
        
        // Find player with this rank
        PlayerRankInfo player = players.Find(p => p.rank == rank);
        
        if (player != null)
        {
            textComponent.text = player.player_name;
        }
        else
        {
            textComponent.text = "-";
        }
    }
    
    private void OnBackButtonClicked()
    {
        // Hide this screen
        gameObject.SetActive(false);
    }
    
    // Helper class to combine player info
    private class PlayerRankInfo
    {
        public string player_name;
        public int rank;
    }
}
