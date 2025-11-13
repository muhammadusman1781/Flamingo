using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomSelection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ServerConstants serverConstants;
    public LobbyScreen lobbyScreen;

    // Stored game modes data
    public GameModesResponse gameModesResponse;
    public List<GameMode> GameModes => gameModesResponse?.data;

    public Button BackButton;

    // Each room now has a button and a name (RoomEntry pattern)
    [System.Serializable]
    public class RoomEntry
    {
        public Button roomButton;
        public string roomName;
    }

    // All room entries, assign in inspector or construct at run-time
    public List<RoomEntry> Rooms = new List<RoomEntry>();

    // Navigation buttons for rooms
    public Button NextRoomButton;
    public Button PrevRoomButton;

    // Index tracking current room
    private int currentRoomIndex = 0;

    public void OnBackButtonClick()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        serverConstants = NetworkingHandler.instance.serverConstants;
        FetchGameModes();

        // Reset and disable all room UIs, then show first
        SetAllRoomsInactive();
        currentRoomIndex = 0;
        SetRoomActive(currentRoomIndex);

        // Set up listeners for navigation buttons
        if (NextRoomButton != null)
        {
            NextRoomButton.onClick.RemoveAllListeners();
            NextRoomButton.onClick.AddListener(OnNextRoom);
        }
        if (PrevRoomButton != null)
        {
            PrevRoomButton.onClick.RemoveAllListeners();
            PrevRoomButton.onClick.AddListener(OnPrevRoom);
        }

        // Set up listeners for each roomButton in Rooms (and only if not null)
        for (int i = 0; i < Rooms.Count; i++)
        {
            int index = i;
            if (Rooms[i].roomButton != null)
            {
                Rooms[i].roomButton.onClick.RemoveAllListeners();
                Rooms[i].roomButton.onClick.AddListener(() => OnRoomButtonClicked(index));
            }
        }
    }

    void Start()
    {
        if (BackButton != null)
            BackButton.onClick.AddListener(OnBackButtonClick);

        if (NextRoomButton != null)
        {
            NextRoomButton.onClick.RemoveAllListeners();
            NextRoomButton.onClick.AddListener(OnNextRoom);
        }

        if (PrevRoomButton != null)
        {
            PrevRoomButton.onClick.RemoveAllListeners();
            PrevRoomButton.onClick.AddListener(OnPrevRoom);
        }

        SetAllRoomsInactive();
        if (Rooms != null && Rooms.Count > 0)
        {
            currentRoomIndex = 0;
            SetRoomActive(currentRoomIndex);
        }

        // Add listeners to room buttons
        for (int i = 0; i < Rooms.Count; i++)
        {
            int index = i;
            if (Rooms[i].roomButton != null)
            {
                Rooms[i].roomButton.onClick.RemoveAllListeners();
                Rooms[i].roomButton.onClick.AddListener(() => OnRoomButtonClicked(index));
            }
        }
    }

    private void SetAllRoomsInactive()
    {
        if (Rooms == null) return;
        foreach (var entry in Rooms)
        {
            if (entry != null && entry.roomButton != null)
                entry.roomButton.gameObject.SetActive(false);
        }
    }

    private void SetRoomActive(int index)
    {
        if (Rooms == null || Rooms.Count == 0) return;

        for (int i = 0; i < Rooms.Count; i++)
        {
            if (Rooms[i] != null && Rooms[i].roomButton != null)
                Rooms[i].roomButton.gameObject.SetActive(i == index);
        }

        // Enable/disable nav buttons based on position
        if (PrevRoomButton != null)
            PrevRoomButton.interactable = index > 0;
        if (NextRoomButton != null)
            NextRoomButton.interactable = index < Rooms.Count - 1;
    }

    private void OnNextRoom()
    {
        if (Rooms == null || Rooms.Count == 0) return;
        if (currentRoomIndex < Rooms.Count - 1)
        {
            currentRoomIndex++;
            SetRoomActive(currentRoomIndex);
        }
    }

    private void OnPrevRoom()
    {
        if (Rooms == null || Rooms.Count == 0) return;
        if (currentRoomIndex > 0)
        {
            currentRoomIndex--;
            SetRoomActive(currentRoomIndex);
        }
    }

    private void OnRoomButtonClicked(int index)
    {
        if (index < 0 || index >= Rooms.Count)
        {
            Debug.LogError($"Invalid room index: {index}");
            return;
        }

        var roomName = Rooms[index].roomName;
        Debug.Log($"Room button '{roomName}' (index {index}) clicked.");

        // Check if game modes are loaded
        if (gameModesResponse == null || gameModesResponse.data == null || gameModesResponse.data.Count == 0)
        {
            Debug.LogError("Game modes not loaded yet!");
            return;
        }

        // Find matching game mode by name
        GameMode matchedGameMode = GetGameModeByName(roomName);
        
        if (matchedGameMode == null)
        {
            Debug.LogError($"No game mode found matching room name: {roomName}");
            return;
        }

        Debug.Log($"Matched game mode: {matchedGameMode.game_name} with ID: {matchedGameMode.game_id}");
        
        // Call join room API with the matched game_id
        JoinRoom(matchedGameMode.game_id);
    }

    private void JoinRoom(string gameModeId)
    {
        if (string.IsNullOrEmpty(gameModeId))
        {
            Debug.LogError("Game mode ID is null or empty!");
            return;
        }

        if (NetworkingHandler.instance == null)
        {
            Debug.LogError("NetworkingHandler instance is not available!");
            return;
        }

        if (serverConstants == null)
        {
            Debug.LogError("ServerConstants is not assigned!");
            return;
        }

        // Create request object
        JoinRoomRequest request = new JoinRoomRequest
        {
            game_mode = gameModeId
        };

        // Convert to JSON
        string jsonToSend = JsonUtility.ToJson(request);
        
        // API URL
        string apiUrl = serverConstants.baseUrl + "/multiplayer/rooms/join/";
        
        Debug.Log($"Joining room with game_mode: {gameModeId}");
        Debug.Log($"Request JSON: {jsonToSend}");
        Debug.Log($"API URL: {apiUrl}");

        // Call POST API
        NetworkingHandler.instance.postMessage(
            apiUrl,
            jsonToSend,
            isTokenNeeded: true,
            onSuccess: OnJoinRoomSuccess,
            onFail: OnJoinRoomFail
        );
    }

    private void OnJoinRoomSuccess(string response)
    {
        Debug.Log($"Join room successful: {response}");

        try
        {
            JoinRoomResponse joinRoomResponse = JsonUtility.FromJson<JoinRoomResponse>(response);

            if (joinRoomResponse != null && joinRoomResponse.data != null)
            {
                Debug.Log($"Room joined successfully: {joinRoomResponse.message}");
                Debug.Log($"Room slug: {joinRoomResponse.data.slug}");
                Debug.Log($"Game mode: {joinRoomResponse.data.game_mode_name}");
                Debug.Log($"Player 1: {joinRoomResponse.data.player1_name}");

                // Pass the response to LobbyScreen
                if (lobbyScreen != null)
                {
                    lobbyScreen.SetRoomData(joinRoomResponse);
                    lobbyScreen.gameObject.SetActive(true);
                    
                    // Optionally hide this screen
                    // gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogError("LobbyScreen reference is not assigned!");
                }
            }
            else
            {
                Debug.LogError("Failed to parse join room response or data is null");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing join room response: {ex.Message}");
        }
    }

    private void OnJoinRoomFail(string error)
    {
        Debug.LogError($"Failed to join room: {error}");
        // You can show an error UI here
    }

    private void FetchGameModes()
    {
        if (serverConstants == null)
        {
            Debug.LogError("ServerConstants is not assigned in RoomSelection!");
            return;
        }
        if (NetworkingHandler.instance == null)
        {
            Debug.LogError("NetworkingHandler instance is not available!");
            return;
        }

        string apiUrl = serverConstants.baseUrl + "/multiplayer/game-modes/";
        Debug.Log($"Fetching game modes from: {apiUrl}");

        NetworkingHandler.instance.getMessage(
            apiUrl,
            isTokenNeeded: true,
            onSuccess: OnGameModesFetchSuccess,
            onFail: OnGameModesFetchFail
        );
    }

    private void OnGameModesFetchSuccess(string response)
    {
        Debug.Log($"Game modes fetched successfully: {response}");

        try
        {
            gameModesResponse = JsonUtility.FromJson<GameModesResponse>(response);

            if (gameModesResponse != null && gameModesResponse.data != null)
            {
                Debug.Log($"Successfully parsed {gameModesResponse.data.Count} game modes");

                foreach (var mode in gameModesResponse.data)
                {
                    Debug.Log($"Game Mode: {mode.game_name}, Price: {mode.price}, Feather Type: {mode.feather_type}");
                }

                OnGameModesLoaded();
            }
            else
            {
                Debug.LogError("Failed to parse game modes response or data is null");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing game modes response: {ex.Message}");
        }
    }

    private void OnGameModesFetchFail(string error)
    {
        Debug.LogError($"Failed to fetch game modes: {error}");
        // You can show an error UI or retry logic here
    }

    private void OnGameModesLoaded()
    {
        Debug.Log("Game modes loaded and ready to use");
    }

    // Public method to get a specific game mode by ID
    public GameMode GetGameModeById(string gameId)
    {
        if (GameModes == null) return null;
        return GameModes.Find(mode => mode.game_id == gameId);
    }

    // Public method to get a specific game mode by name
    public GameMode GetGameModeByName(string gameName)
    {
        if (GameModes == null) return null;
        return GameModes.Find(mode => mode.game_name == gameName);
    }
}
