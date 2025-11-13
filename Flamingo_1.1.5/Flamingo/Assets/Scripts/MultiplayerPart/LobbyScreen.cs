using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScreen : MonoBehaviour
{
    [Header("References")]
    public GameObject gameplayScreen;
    private ServerConstants serverConstants;

    [Header("Room Data")]
    public JoinRoomResponse joinRoomResponse;
    public RoomData CurrentRoomData => joinRoomResponse?.data;

    [Header("Polling Settings")]
    public float pollInterval = 3f;
    private Coroutine pollingCoroutine;
    private bool isPolling = false;

    // Method to set room data from RoomSelection
    public void SetRoomData(JoinRoomResponse response)
    {
        joinRoomResponse = response;
        
        if (joinRoomResponse != null && joinRoomResponse.data != null)
        {
            Debug.Log($"LobbyScreen: Room data set successfully");
            Debug.Log($"Room Slug: {joinRoomResponse.data.slug}");
            Debug.Log($"Game Mode: {joinRoomResponse.data.game_mode_name}");
            Debug.Log($"Room Type: {joinRoomResponse.data.room_type}");
            Debug.Log($"Level: {joinRoomResponse.data.level}");
            Debug.Log($"Player 1: {joinRoomResponse.data.player1_name} (ID: {joinRoomResponse.data.player1})");
            
            // Debug initial player2 data
            Debug.Log($"[SetRoomData] Initial Player2 Value: {joinRoomResponse.data.player2}");
            Debug.Log($"[SetRoomData] Initial Player2 Name: '{joinRoomResponse.data.player2_name}'");
            
            if (joinRoomResponse.data.player2 > 0 && !string.IsNullOrEmpty(joinRoomResponse.data.player2_name))
            {
                Debug.Log($"Player 2: {joinRoomResponse.data.player2_name} (ID: {joinRoomResponse.data.player2})");
                Debug.Log("Both players already in room - going to gameplay immediately!");
                // Both players are already in the room, go to gameplay immediately
                StartGameplay();
            }
            else
            {
                Debug.Log("Player 2: Waiting for player...");
                // Start polling for second player
                StartCoroutine(StartPolling());
            }

            // Call method to update UI with room data
            UpdateLobbyUI();
        }
        else
        {
            Debug.LogError("LobbyScreen: Received null or invalid room data");
        }
    }

    private void UpdateLobbyUI()
    {
        // This method will be called when room data is set
        // You can populate your UI elements here with the room data
        // Example:
        // roomNameText.text = CurrentRoomData.game_mode_name;
        // player1NameText.text = CurrentRoomData.player1_name;
        // levelText.text = $"Level: {CurrentRoomData.level}";
        
        Debug.Log("LobbyScreen: UI updated with room data");
    }

    private void OnEnable()
    {
        // Get server constants reference
        if (serverConstants == null && NetworkingHandler.instance != null)
        {
            serverConstants = NetworkingHandler.instance.serverConstants;
        }

        // Called when lobby screen is activated
        if (joinRoomResponse != null && joinRoomResponse.data != null)
        {
            UpdateLobbyUI();
            
            // Check if we need to start polling
            if (!IsRoomFull())
            {
                StartCoroutine(StartPolling());
            }
        }
    }

    private void OnDisable()
    {
        // Stop polling when screen is disabled
        StopPolling();
    }

    private IEnumerator StartPolling()
    {
        if (isPolling)
        {
            Debug.LogWarning("Polling is already active");
            yield break;
        }

        if (CurrentRoomData == null)
        {
            Debug.LogError("Cannot start polling: No room data available");
            yield break;
        }

        Debug.Log("Starting to poll for second player...");
        isPolling = true;
        
        // Start the actual polling loop
        pollingCoroutine = StartCoroutine(PollForSecondPlayer());
    }

    private void StopPolling()
    {
        if (pollingCoroutine != null)
        {
            StopCoroutine(pollingCoroutine);
            pollingCoroutine = null;
        }
        isPolling = false;
        Debug.Log("Stopped polling for second player");
    }

    private IEnumerator PollForSecondPlayer()
    {
        Debug.Log($"PollForSecondPlayer coroutine started. Will check every {pollInterval} seconds.");
        
        while (isPolling)
        {
            if (CurrentRoomData == null)
            {
                Debug.LogError("Room data is null, stopping polling");
                StopPolling();
                yield break;
            }

            Debug.Log($"Polling room {CurrentRoomData.slug} for second player...");
            CheckRoomStatus();
            
            // Wait for the specified interval before next poll
            yield return new WaitForSeconds(pollInterval);
        }
    }

    private void CheckRoomStatus()
    {
        Debug.Log("[CheckRoomStatus] Called");
        
        if (serverConstants == null)
        {
            Debug.LogError("ServerConstants is not assigned!");
            return;
        }

        if (NetworkingHandler.instance == null)
        {
            Debug.LogError("NetworkingHandler instance is not available!");
            return;
        }

        if (CurrentRoomData == null)
        {
            Debug.LogError("Current room data is null!");
            return;
        }

        // Create the join request again to check room status
        JoinRoomRequest request = new JoinRoomRequest
        {
            game_mode = CurrentRoomData.game_mode
        };

        string jsonToSend = JsonUtility.ToJson(request);
        string apiUrl = serverConstants.baseUrl + "/multiplayer/rooms/join/";

        Debug.Log($"[CheckRoomStatus] Calling join API to check room status");
        Debug.Log($"[CheckRoomStatus] API URL: {apiUrl}");
        Debug.Log($"[CheckRoomStatus] Request JSON: {jsonToSend}");

        NetworkingHandler.instance.postMessage(
            apiUrl,
            jsonToSend,
            isTokenNeeded: true,
            onSuccess: OnRoomStatusCheckSuccess,
            onFail: OnRoomStatusCheckFail
        );
    }

    private void OnRoomStatusCheckSuccess(string response)
    {
        Debug.Log($"Room status check response: {response}");

        try
        {
            JoinRoomResponse updatedResponse = JsonUtility.FromJson<JoinRoomResponse>(response);

            if (updatedResponse != null && updatedResponse.data != null)
            {
                // Update the room data
                joinRoomResponse = updatedResponse;

                // Debug player2 data
                Debug.Log($"[OnRoomStatusCheckSuccess] Player2 Value: {updatedResponse.data.player2}");
                Debug.Log($"[OnRoomStatusCheckSuccess] Player2 Name: '{updatedResponse.data.player2_name}'");
                Debug.Log($"[OnRoomStatusCheckSuccess] Player2 Name IsNullOrEmpty: {string.IsNullOrEmpty(updatedResponse.data.player2_name)}");
                Debug.Log($"[OnRoomStatusCheckSuccess] Condition check: player2 > 0? {updatedResponse.data.player2 > 0} && name not empty? {!string.IsNullOrEmpty(updatedResponse.data.player2_name)}");

                // Check if player 2 has joined
                if (updatedResponse.data.player2 > 0 && !string.IsNullOrEmpty(updatedResponse.data.player2_name))
                {
                    Debug.Log($"✓ Player 2 joined: {updatedResponse.data.player2_name} (ID: {updatedResponse.data.player2})");
                    
                    // Update UI to show both players
                    UpdateLobbyUI();
                    
                    // Stop polling
                    StopPolling();
                    
                    // Start gameplay
                    Debug.Log("Calling StartGameplay()...");
                    StartGameplay();
                }
                else
                {
                    Debug.Log("Still waiting for player 2...");
                    // Update UI to refresh waiting status
                    UpdateLobbyUI();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing room status response: {ex.Message}");
        }
    }

    private void OnRoomStatusCheckFail(string error)
    {
        Debug.LogError($"Failed to check room status: {error}");
        // Continue polling despite the error
    }

    private void StartGameplay()
    {
        Debug.Log("========================");
        Debug.Log("Both players ready! Starting gameplay...");
        Debug.Log($"GameplayScreen reference is null: {gameplayScreen == null}");
        
        if (gameplayScreen != null)
        {
            Debug.Log($"GameplayScreen GameObject: {gameplayScreen.name}");
            Debug.Log("Activating gameplay screen...");
            
            // Activate gameplay screen
            gameplayScreen.GetComponent<QuizScreenMultiplayer>().roomSlug = CurrentRoomData.slug;
            gameplayScreen.SetActive(true);
            Debug.Log("Gameplay screen activated!");
            
            // Optionally, pass room data to gameplay screen if it has a script
            // For example:
            // var gameplayScript = gameplayScreen.GetComponent<GameplayScreen>();
            // if (gameplayScript != null)
            // {
            //     gameplayScript.SetRoomData(joinRoomResponse);
            // }
            
            // Hide lobby screen
            Debug.Log("Hiding lobby screen...");
            gameObject.SetActive(false);
            Debug.Log("Lobby screen hidden!");
        }
        else
        {
            Debug.LogError("GameplayScreen reference is not assigned in LobbyScreen Inspector!");
        }
        Debug.Log("========================");
    }

    // Helper method to check if room is full
    public bool IsRoomFull()
    {
        return CurrentRoomData != null && 
               CurrentRoomData.player2 > 0 && 
               !string.IsNullOrEmpty(CurrentRoomData.player2_name);
    }

    // Helper method to check if current user is player 1
    public bool IsPlayer1()
    {
        if (CurrentRoomData == null) return false;
        
        // You can compare with user ID from ServerConstants if needed
        // For now, we assume if player2 is 0 or negative, current user is player1
        return CurrentRoomData.player2 <= 0;
    }

    // Helper method to get room status message
    public string GetRoomStatusMessage()
    {
        if (joinRoomResponse == null) return "No room data";
        return joinRoomResponse.message;
    }
}
