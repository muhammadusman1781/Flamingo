using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AddFriendsScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ServerConstants serverConstants;
    public ChallengeFriendsScreen challengeFriendsScreen;
    public FriendRequestsScreen friendRequestsScreen;
    public Button ChallengeFriendsButton;
    public Button FriendRequestsButton;
    public Button BackButton;
    
    [Header("User Entity")]
    public GameObject userEntityPrefab;
    public Transform userEntityContainer;
    
    [Header("Data")]
    private UsersListResponse usersListResponse;
    private FriendsListResponse friendsListResponse;
    private FriendRequestsResponse friendRequestsResponse;
    private List<UserEntity> instantiatedUserEntities = new List<UserEntity>();
    private bool usersListFetched = false;
    private bool friendsListFetched = false;
    private bool friendRequestsFetched = false;

    public GameObject MultiplayerGamesScreen;

    private void OnEnable()
    {
        // Get server constants reference
        if (serverConstants == null && NetworkingHandler.instance != null)
        {
            serverConstants = NetworkingHandler.instance.serverConstants;
        }

        // Clear any existing user entities
        ClearUserEntities();

        // Reset fetch flags
        usersListFetched = false;
        friendsListFetched = false;
        friendRequestsFetched = false;

        // Fetch all data when screen is enabled
        FetchUsersList();
        FetchFriendsList();
        FetchFriendRequests();
    }

    void Start()
    {
        ChallengeFriendsButton.onClick.AddListener(OnChallengeFriendButtonClicked);
        FriendRequestsButton.onClick.AddListener(OnFriendRequestsButtonClicked);
        BackButton.onClick.AddListener(OnBackButtonClicked);
    }

    public void OnBackButtonClicked()
    {
        Debug.Log("BackButtonClicked");
        MultiplayerGamesScreen.SetActive(true);
        this.gameObject.SetActive(false);
    }

    private void ClearUserEntities()
    {
        // Destroy all instantiated user entities
        foreach (var entity in instantiatedUserEntities)
        {
            if (entity != null)
            {
                Destroy(entity.gameObject);
            }
        }
        instantiatedUserEntities.Clear();
    }

    private void FetchUsersList()
    {
        if (serverConstants == null)
        {
            Debug.LogError("ServerConstants is not assigned in AddFriendsScreen!");
            return;
        }

        if (NetworkingHandler.instance == null)
        {
            Debug.LogError("NetworkingHandler instance is not available!");
            return;
        }

        string apiUrl = serverConstants.baseUrl + "/auth/users/list/";
        Debug.Log($"Fetching users list from: {apiUrl}");

        NetworkingHandler.instance.getMessage(
            apiUrl,
            isTokenNeeded: true,
            onSuccess: OnUsersListFetchSuccess,
            onFail: OnUsersListFetchFail
        );
    }

    private void OnUsersListFetchSuccess(string response)
    {
        Debug.Log($"Users list fetched successfully: {response}");

        try
        {
            usersListResponse = JsonUtility.FromJson<UsersListResponse>(response);

            if (usersListResponse != null && usersListResponse.data != null && usersListResponse.data.Count > 0)
            {
                Debug.Log($"Successfully parsed {usersListResponse.data.Count} users");
                usersListFetched = true;

                // Display users only after both lists are fetched
                TryDisplayUsers();
            }
            else
            {
                Debug.LogError("Failed to parse users list response or data is empty");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing users list response: {ex.Message}");
        }
    }

    private void OnUsersListFetchFail(string error)
    {
        Debug.LogError($"Failed to fetch users list: {error}");
    }

    private void FetchFriendsList()
    {
        if (serverConstants == null)
        {
            Debug.LogError("ServerConstants is not assigned in AddFriendsScreen!");
            return;
        }

        if (NetworkingHandler.instance == null)
        {
            Debug.LogError("NetworkingHandler instance is not available!");
            return;
        }

        string apiUrl = serverConstants.baseUrl + "/auth/friends/list/";
        Debug.Log($"Fetching friends list from: {apiUrl}");

        NetworkingHandler.instance.getMessage(
            apiUrl,
            isTokenNeeded: true,
            onSuccess: OnFriendsListFetchSuccess,
            onFail: OnFriendsListFetchFail
        );
    }

    private void OnFriendsListFetchSuccess(string response)
    {
        Debug.Log($"Friends list fetched successfully: {response}");

        try
        {
            friendsListResponse = JsonUtility.FromJson<FriendsListResponse>(response);

            if (friendsListResponse != null && friendsListResponse.data != null)
            {
                Debug.Log($"Successfully parsed {friendsListResponse.data.Count} friends");
                friendsListFetched = true;

                // Display users only after both lists are fetched
                TryDisplayUsers();
            }
            else
            {
                Debug.LogError("Failed to parse friends list response or data is null");
                friendsListFetched = true; // Still set to true to allow display with empty friends list
                TryDisplayUsers();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing friends list response: {ex.Message}");
            friendsListFetched = true; // Still set to true to allow display
            TryDisplayUsers();
        }
    }

    private void OnFriendsListFetchFail(string error)
    {
        Debug.LogError($"Failed to fetch friends list: {error}");
        friendsListFetched = true; // Still set to true to allow display with empty friends list
        TryDisplayUsers();
    }

    private void FetchFriendRequests()
    {
        if (serverConstants == null)
        {
            Debug.LogError("ServerConstants is not assigned in AddFriendsScreen!");
            return;
        }

        if (NetworkingHandler.instance == null)
        {
            Debug.LogError("NetworkingHandler instance is not available!");
            return;
        }

        string apiUrl = serverConstants.baseUrl + "/auth/friends/requests/";
        Debug.Log($"Fetching friend requests from: {apiUrl}");

        NetworkingHandler.instance.getMessage(
            apiUrl,
            isTokenNeeded: true,
            onSuccess: OnFriendRequestsFetchSuccess,
            onFail: OnFriendRequestsFetchFail
        );
    }

    private void OnFriendRequestsFetchSuccess(string response)
    {
        Debug.Log($"Friend requests fetched successfully: {response}");

        try
        {
            friendRequestsResponse = JsonUtility.FromJson<FriendRequestsResponse>(response);

            if (friendRequestsResponse != null && friendRequestsResponse.data != null)
            {
                int receivedCount = friendRequestsResponse.data.received_requests?.Count ?? 0;
                int sentCount = friendRequestsResponse.data.sent_requests?.Count ?? 0;
                Debug.Log($"Successfully parsed {receivedCount} received and {sentCount} sent friend requests");
                friendRequestsFetched = true;

                // Display users only after all lists are fetched
                TryDisplayUsers();
            }
            else
            {
                Debug.LogError("Failed to parse friend requests response or data is null");
                friendRequestsFetched = true;
                TryDisplayUsers();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing friend requests response: {ex.Message}");
            friendRequestsFetched = true;
            TryDisplayUsers();
        }
    }

    private void OnFriendRequestsFetchFail(string error)
    {
        Debug.LogError($"Failed to fetch friend requests: {error}");
        friendRequestsFetched = true;
        TryDisplayUsers();
    }

    private void TryDisplayUsers()
    {
        // Only display users when all API calls have completed
        if (usersListFetched && friendsListFetched && friendRequestsFetched)
        {
            DisplayUsers();
        }
    }

    private void DisplayUsers()
    {
        if (usersListResponse == null || usersListResponse.data == null || usersListResponse.data.Count == 0)
        {
            Debug.LogError("No users to display");
            return;
        }

        if (userEntityPrefab == null)
        {
            Debug.LogError("UserEntity prefab is not assigned!");
            return;
        }

        if (userEntityContainer == null)
        {
            Debug.LogError("UserEntity container is not assigned!");
            return;
        }

        // Clear existing entities
        ClearUserEntities();

        // Create HashSets for quick lookup
        HashSet<int> friendIds = new HashSet<int>();
        if (friendsListResponse != null && friendsListResponse.data != null)
        {
            foreach (var friend in friendsListResponse.data)
            {
                friendIds.Add(friend.id);
            }
        }

        // Create HashSet of user IDs to whom we've sent friend requests
        HashSet<int> sentRequestUserIds = new HashSet<int>();
        if (friendRequestsResponse != null && friendRequestsResponse.data != null && friendRequestsResponse.data.sent_requests != null)
        {
            foreach (var request in friendRequestsResponse.data.sent_requests)
            {
                if (request.receiver != null)
                {
                    sentRequestUserIds.Add(request.receiver.id);
                }
            }
        }

        // Filter users: exclude friends and only include those with first and last names
        List<UserData> filteredUsers = usersListResponse.data.FindAll(u => 
            !friendIds.Contains(u.id) &&
            !string.IsNullOrEmpty(u.first_name) && 
            !string.IsNullOrEmpty(u.last_name)
        );

        Debug.Log($"Filtered users (non-friends with names): {filteredUsers.Count}");
        
        // Check if there's a user named "Usman" or contains "usman" (case-insensitive) in filtered list
        UserData usmanUser = filteredUsers.Find(u => 
            (!string.IsNullOrEmpty(u.first_name) && u.first_name.ToLower().Contains("usman")) ||
            (!string.IsNullOrEmpty(u.last_name) && u.last_name.ToLower().Contains("usman"))
        );

        List<UserData> usersToShow = new List<UserData>();

        // If Usman exists, add him first
        if (usmanUser != null)
        {
            usersToShow.Add(usmanUser);
            Debug.Log($"Found Usman: {usmanUser.first_name} {usmanUser.last_name} ({usmanUser.email})");
        }

        // Add other users until we have 4 total
        foreach (var user in filteredUsers)
        {
            if (usersToShow.Count >= 4)
                break;

            // Skip if this is the Usman user we already added
            if (usmanUser != null && user.id == usmanUser.id)
                continue;

            usersToShow.Add(user);
        }

        // Instantiate user entities
        foreach (var user in usersToShow)
        {
            GameObject userEntityObj = Instantiate(userEntityPrefab, userEntityContainer);
            UserEntity userEntity = userEntityObj.GetComponent<UserEntity>();

            if (userEntity != null)
            {
                bool requestAlreadySent = sentRequestUserIds.Contains(user.id);
                userEntity.Initialize(user, this, requestAlreadySent);
                instantiatedUserEntities.Add(userEntity);
                
                Debug.Log($"Instantiated user: {user.first_name} {user.last_name} (ID: {user.id}, Request sent: {requestAlreadySent})");
            }
            else
            {
                Debug.LogError("UserEntity component not found on prefab!");
                Destroy(userEntityObj);
            }
        }

        Debug.Log($"Displayed {instantiatedUserEntities.Count} users");
    }

    public void SendFriendRequest(int receiverId, UserEntity userEntity)
    {
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

        // Create request object
        SendFriendRequest request = new SendFriendRequest
        {
            receiver_id = receiverId
        };

        // Convert to JSON
        string jsonToSend = JsonUtility.ToJson(request);
        
        // API URL
        string apiUrl = serverConstants.baseUrl + "/auth/friends/send-request/";
        
        Debug.Log($"Sending friend request to user ID: {receiverId}");
        Debug.Log($"Request JSON: {jsonToSend}");
        Debug.Log($"API URL: {apiUrl}");

        // Call POST API
        NetworkingHandler.instance.postMessage(
            apiUrl,
            jsonToSend,
            isTokenNeeded: true,
            onSuccess: (response) => OnSendFriendRequestSuccess(response, userEntity),
            onFail: OnSendFriendRequestFail
        );
    }

    private void OnSendFriendRequestSuccess(string response, UserEntity userEntity)
    {
        Debug.Log($"Friend request sent successfully: {response}");

        try
        {
            SendFriendRequestResponse sendFriendRequestResponse = JsonUtility.FromJson<SendFriendRequestResponse>(response);

            if (sendFriendRequestResponse != null && sendFriendRequestResponse.status == "success")
            {
                Debug.Log($"Friend request successful: {sendFriendRequestResponse.message}");
                
                // Disable the add friend button on this user entity
                if (userEntity != null)
                {
                    userEntity.DisableAddFriendButton();
                    Debug.Log("Add friend button disabled");
                }
            }
            else
            {
                Debug.LogError("Failed to send friend request or invalid response");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing friend request response: {ex.Message}");
        }
    }

    private void OnSendFriendRequestFail(string error)
    {
        Debug.LogError($"Failed to send friend request: {error}");
    }

    public void OnChallengeFriendButtonClicked()
    {
        if (challengeFriendsScreen != null)
        {
            // Pass friends data to the challenge friends screen
            if (friendsListResponse != null && friendsListResponse.data != null)
            {
                challengeFriendsScreen.SetFriendsData(friendsListResponse);
                challengeFriendsScreen.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("No friends data available to pass to ChallengeFriendsScreen");
            }
        }
        else
        {
            Debug.LogError("ChallengeFriendsScreen reference is not assigned!");
        }
    }

    public void OnFriendRequestsButtonClicked()
    {
        if (friendRequestsScreen != null)
        {
            // Pass friend requests data to the friend requests screen
            if (friendRequestsResponse != null && friendRequestsResponse.data != null)
            {
                friendRequestsScreen.SetFriendRequestsData(friendRequestsResponse, this);
                friendRequestsScreen.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("No friend requests data available to pass to FriendRequestsScreen");
            }
        }
        else
        {
            Debug.LogError("FriendRequestsScreen reference is not assigned!");
        }
    }

    public void RefreshFriendsAndRequests()
    {
        // Called after accepting/rejecting a friend request
        Debug.Log("Refreshing friends list and friend requests...");
        
        // Reset flags
        friendsListFetched = false;
        friendRequestsFetched = false;
        
        // Fetch updated data
        FetchFriendsList();
        FetchFriendRequests();
    }

    public FriendsListResponse GetFriendsListResponse()
    {
        return friendsListResponse;
    }
}
