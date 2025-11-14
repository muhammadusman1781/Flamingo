using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestsScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ServerConstants serverConstants;
    public Button BackButton;
    
    [Header("Friend Request Entity")]
    public GameObject friendRequestEntityPrefab;
    public Transform friendRequestEntityContainer;
    
    [Header("Data")]
    private FriendRequestsResponse friendRequestsResponse;
    private List<FriendRequestEntity> instantiatedRequestEntities = new List<FriendRequestEntity>();
    private AddFriendsScreen addFriendsScreen;

    public GameObject AddFriendsScreen;

    private void Start()
    {
        if (BackButton != null)
        {
            BackButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    public void OnBackButtonClicked()
    {
        Debug.Log("BackButtonClicked");
        AddFriendsScreen.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void SetFriendRequestsData(FriendRequestsResponse response, AddFriendsScreen screen)
    {
        friendRequestsResponse = response;
        addFriendsScreen = screen;
        
        if (serverConstants == null && NetworkingHandler.instance != null)
        {
            serverConstants = NetworkingHandler.instance.serverConstants;
        }
        
        if (friendRequestsResponse != null && friendRequestsResponse.data != null)
        {
            int receivedCount = friendRequestsResponse.data.received_requests?.Count ?? 0;
            Debug.Log($"FriendRequestsScreen: Received {receivedCount} friend requests");
            DisplayFriendRequests();
        }
        else
        {
            Debug.LogError("FriendRequestsScreen: Received null or invalid friend requests data");
        }
    }

    private void OnEnable()
    {
        // If we already have friend requests data, display it
        if (friendRequestsResponse != null && friendRequestsResponse.data != null)
        {
            DisplayFriendRequests();
        }
    }

    private void OnDisable()
    {
        // Clear friend request entities when screen is disabled
        ClearFriendRequestEntities();
    }

    private void ClearFriendRequestEntities()
    {
        // Destroy all instantiated friend request entities
        foreach (var entity in instantiatedRequestEntities)
        {
            if (entity != null)
            {
                Destroy(entity.gameObject);
            }
        }
        instantiatedRequestEntities.Clear();
    }

    private void DisplayFriendRequests()
    {
        if (friendRequestsResponse == null || friendRequestsResponse.data == null || 
            friendRequestsResponse.data.received_requests == null || friendRequestsResponse.data.received_requests.Count == 0)
        {
            Debug.LogWarning("No friend requests to display");
            return;
        }

        if (friendRequestEntityPrefab == null)
        {
            Debug.LogError("FriendRequestEntity prefab is not assigned!");
            return;
        }

        if (friendRequestEntityContainer == null)
        {
            Debug.LogError("FriendRequestEntity container is not assigned!");
            return;
        }

        // Clear existing entities
        ClearFriendRequestEntities();

        // Instantiate friend request entities for all received requests
        foreach (var request in friendRequestsResponse.data.received_requests)
        {
            GameObject requestEntityObj = Instantiate(friendRequestEntityPrefab, friendRequestEntityContainer);
            FriendRequestEntity requestEntity = requestEntityObj.GetComponent<FriendRequestEntity>();

            if (requestEntity != null)
            {
                requestEntity.Initialize(request, this);
                instantiatedRequestEntities.Add(requestEntity);
                
                Debug.Log($"Instantiated friend request from: {request.sender.first_name} {request.sender.last_name} (ID: {request.id})");
            }
            else
            {
                Debug.LogError("FriendRequestEntity component not found on prefab!");
                Destroy(requestEntityObj);
            }
        }

        Debug.Log($"Displayed {instantiatedRequestEntities.Count} friend requests");
    }

    public void AcceptFriendRequest(int requestId, FriendRequestEntity entity)
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

        string apiUrl = serverConstants.baseUrl + $"/auth/friends/accept-request/{requestId}/";
        
        Debug.Log($"Accepting friend request ID: {requestId}");
        Debug.Log($"API URL: {apiUrl}");

        // Call POST API
        NetworkingHandler.instance.postMessage(
            apiUrl,
            "", // Empty body for this endpoint
            isTokenNeeded: true,
            onSuccess: (response) => OnAcceptFriendRequestSuccess(response, entity),
            onFail: OnAcceptFriendRequestFail
        );
    }

    private void OnAcceptFriendRequestSuccess(string response, FriendRequestEntity entity)
    {
        Debug.Log($"Friend request accepted successfully: {response}");

        try
        {
            FriendRequestActionResponse actionResponse = JsonUtility.FromJson<FriendRequestActionResponse>(response);

            if (actionResponse != null && actionResponse.status == "success")
            {
                Debug.Log($"Friend request accepted: {actionResponse.message}");
                
                // Remove the entity from UI
                if (entity != null)
                {
                    instantiatedRequestEntities.Remove(entity);
                    Destroy(entity.gameObject);
                    Debug.Log("Friend request entity removed from UI");
                }

                // Refresh friends list and requests in AddFriendsScreen
                if (addFriendsScreen != null)
                {
                    addFriendsScreen.RefreshFriendsAndRequests();
                    
                    // Update ChallengeFriendsScreen if it's active
                    var challengeScreen = addFriendsScreen.challengeFriendsScreen;
                    if (challengeScreen != null && challengeScreen.gameObject.activeSelf)
                    {
                        var updatedFriendsList = addFriendsScreen.GetFriendsListResponse();
                        if (updatedFriendsList != null)
                        {
                            challengeScreen.SetFriendsData(updatedFriendsList);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to accept friend request or invalid response");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing accept friend request response: {ex.Message}");
        }
    }

    private void OnAcceptFriendRequestFail(string error)
    {
        Debug.LogError($"Failed to accept friend request: {error}");
    }

    public void RejectFriendRequest(int requestId, FriendRequestEntity entity)
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

        string apiUrl = serverConstants.baseUrl + $"/auth/friends/reject-request/{requestId}/";
        
        Debug.Log($"Rejecting friend request ID: {requestId}");
        Debug.Log($"API URL: {apiUrl}");

        // Call POST API
        NetworkingHandler.instance.postMessage(
            apiUrl,
            "", // Empty body for this endpoint
            isTokenNeeded: true,
            onSuccess: (response) => OnRejectFriendRequestSuccess(response, entity),
            onFail: OnRejectFriendRequestFail
        );
    }

    private void OnRejectFriendRequestSuccess(string response, FriendRequestEntity entity)
    {
        Debug.Log($"Friend request rejected successfully: {response}");

        try
        {
            FriendRequestActionResponse actionResponse = JsonUtility.FromJson<FriendRequestActionResponse>(response);

            if (actionResponse != null && actionResponse.status == "success")
            {
                Debug.Log($"Friend request rejected: {actionResponse.message}");
                
                // Remove the entity from UI
                if (entity != null)
                {
                    instantiatedRequestEntities.Remove(entity);
                    Destroy(entity.gameObject);
                    Debug.Log("Friend request entity removed from UI");
                }

                // Refresh friend requests in AddFriendsScreen
                if (addFriendsScreen != null)
                {
                    addFriendsScreen.RefreshFriendsAndRequests();
                }
            }
            else
            {
                Debug.LogError("Failed to reject friend request or invalid response");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing reject friend request response: {ex.Message}");
        }
    }

    private void OnRejectFriendRequestFail(string error)
    {
        Debug.LogError($"Failed to reject friend request: {error}");
    }
}

