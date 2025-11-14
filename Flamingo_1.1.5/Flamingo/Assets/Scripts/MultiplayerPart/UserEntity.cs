using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RTLTMPro;
public class UserEntity : MonoBehaviour
{
    [Header("UI References")]
    public RTLTextMeshPro nameText; // Use Text if you're using legacy UI
    public Button addFriendButton;

    [Header("User Data")]
    private UserData userData;
    private AddFriendsScreen parentScreen;

    public void Initialize(UserData user, AddFriendsScreen screen, bool requestAlreadySent = false)
    {
        userData = user;
        parentScreen = screen;

        // Populate name
        if (nameText != null)
        {
            string displayName = GetDisplayName();
            nameText.text = displayName;
        }

        // Setup button listener
        if (addFriendButton != null)
        {
            addFriendButton.onClick.RemoveAllListeners();
            addFriendButton.onClick.AddListener(OnAddFriendButtonClick);
            
            // If request already sent, disable the button
            if (requestAlreadySent)
            {
                addFriendButton.interactable = false;
                addFriendButton.gameObject.SetActive(false);
            }
            else
            {
                addFriendButton.gameObject.SetActive(true);
            }
        }

    }

    private string GetDisplayName()
    {
        if (userData == null)
            return "Unknown";

        // Construct full name from first_name and last_name
        string firstName = string.IsNullOrEmpty(userData.first_name) ? "" : userData.first_name;
        string lastName = string.IsNullOrEmpty(userData.last_name) ? "" : userData.last_name;
        
        string fullName = $"{firstName} {lastName}".Trim();

        // If both names are empty, use email
        if (string.IsNullOrEmpty(fullName))
        {
            fullName = userData.email;
        }

        return fullName;
    }

    private void OnAddFriendButtonClick()
    {
        if (userData == null)
        {
            Debug.LogError("UserData is null!");
            return;
        }

        if (parentScreen != null)
        {
            parentScreen.SendFriendRequest(userData.id, this);
        }
        else
        {
            Debug.LogError("Parent AddFriendsScreen reference is null!");
        }
    }



    public void DisableAddFriendButton()
    {
        if (addFriendButton != null)
        {
            addFriendButton.interactable = false;
            addFriendButton.gameObject.SetActive(false);
        }
    }
}

