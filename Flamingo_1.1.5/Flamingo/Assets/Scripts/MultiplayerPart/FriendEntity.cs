using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RTLTMPro;

public class FriendEntity : MonoBehaviour
{
    [Header("UI References")]
    public RTLTextMeshPro nameText;
    public Button challengeButton;

    [Header("Friend Data")]
    private UserData friendData;
    private ChallengeFriendsScreen parentScreen;

    public void Initialize(UserData friend, ChallengeFriendsScreen screen)
    {
        friendData = friend;
        parentScreen = screen;

        // Populate name
        if (nameText != null)
        {
            string displayName = GetDisplayName();
            nameText.text = displayName;
        }

        // Setup challenge button listener
        if (challengeButton != null)
        {
            challengeButton.onClick.RemoveAllListeners();
            challengeButton.onClick.AddListener(OnChallengeButtonClick);
            challengeButton.gameObject.SetActive(true);
        }
    }

    private string GetDisplayName()
    {
        if (friendData == null)
            return "Unknown";

        // Construct full name from first_name and last_name
        string firstName = string.IsNullOrEmpty(friendData.first_name) ? "" : friendData.first_name;
        string lastName = string.IsNullOrEmpty(friendData.last_name) ? "" : friendData.last_name;
        
        string fullName = $"{firstName} {lastName}".Trim();

        // If both names are empty, use email
        if (string.IsNullOrEmpty(fullName))
        {
            fullName = friendData.email;
        }

        return fullName;
    }

    private void OnChallengeButtonClick()
    {
        if (friendData == null)
        {
            Debug.LogError("FriendData is null!");
            return;
        }

        if (parentScreen != null)
        {
            parentScreen.OnFriendChallengeButtonClick(friendData.id);
        }
        else
        {
            Debug.LogError("Parent ChallengeFriendsScreen reference is null!");
        }
    }
}

