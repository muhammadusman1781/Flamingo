using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChallengeFriendsScreen : MonoBehaviour
{
    public Button BackButton;
    public RoomSelection roomSelection;
    
    [Header("Friend Entity")]
    public GameObject friendEntityPrefab;
    public Transform friendEntityContainer;
    
    [Header("Data")]
    private FriendsListResponse friendsListResponse;
    private List<FriendEntity> instantiatedFriendEntities = new List<FriendEntity>();

    public GameObject AddFriendsScreen;

    public void Start()
    {
        BackButton.onClick.AddListener(OnBackButtonClicked);
    }
    public void OnBackButtonClicked()
    {
        Debug.Log("BackButtonClicked");
        AddFriendsScreen.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void SetFriendsData(FriendsListResponse response)
    {
        friendsListResponse = response;
        
        if (friendsListResponse != null && friendsListResponse.data != null)
        {
            Debug.Log($"ChallengeFriendsScreen: Received {friendsListResponse.data.Count} friends");
            DisplayFriends();
        }
        else
        {
            Debug.LogError("ChallengeFriendsScreen: Received null or invalid friends data");
        }
    }

    private void OnEnable()
    {
        // If we already have friends data, display it
        if (friendsListResponse != null && friendsListResponse.data != null)
        {
            DisplayFriends();
        }
    }

    private void OnDisable()
    {
        // Clear friend entities when screen is disabled
        ClearFriendEntities();
    }

    private void ClearFriendEntities()
    {
        // Destroy all instantiated friend entities
        foreach (var entity in instantiatedFriendEntities)
        {
            if (entity != null)
            {
                Destroy(entity.gameObject);
            }
        }
        instantiatedFriendEntities.Clear();
    }

    private void DisplayFriends()
    {
        if (friendsListResponse == null || friendsListResponse.data == null || friendsListResponse.data.Count == 0)
        {
            Debug.LogError("No friends to display");
            return;
        }

        if (friendEntityPrefab == null)
        {
            Debug.LogError("FriendEntity prefab is not assigned!");
            return;
        }

        if (friendEntityContainer == null)
        {
            Debug.LogError("FriendEntity container is not assigned!");
            return;
        }

        // Clear existing entities
        ClearFriendEntities();

        // Instantiate friend entities for all friends
        foreach (var friend in friendsListResponse.data)
        {
            GameObject friendEntityObj = Instantiate(friendEntityPrefab, friendEntityContainer);
            FriendEntity friendEntity = friendEntityObj.GetComponent<FriendEntity>();

            if (friendEntity != null)
            {
                friendEntity.Initialize(friend, this);
                instantiatedFriendEntities.Add(friendEntity);
                
                Debug.Log($"Instantiated friend: {friend.first_name} {friend.last_name} (ID: {friend.id})");
            }
            else
            {
                Debug.LogError("FriendEntity component not found on prefab!");
                Destroy(friendEntityObj);
            }
        }

        Debug.Log($"Displayed {instantiatedFriendEntities.Count} friends");
    }

    public void OnFriendChallengeButtonClick(int friendId)
    {
        Debug.Log($"Friend challenge button clicked for friend ID: {friendId}");
        
        if (roomSelection != null)
        {
            // Set to private match mode and open room selection
            roomSelection.SetMatchMode(isPrivateMatch: true);
            roomSelection.gameObject.SetActive(true);
            
            // Optionally hide this screen
            // this.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("RoomSelection reference is not assigned!");
        }
    }
}

