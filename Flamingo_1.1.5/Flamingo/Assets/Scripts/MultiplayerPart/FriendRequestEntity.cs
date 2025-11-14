using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RTLTMPro;

public class FriendRequestEntity : MonoBehaviour
{
    [Header("UI References")]
    public RTLTextMeshPro nameText;
    public Button acceptButton;
    public Button rejectButton;

    [Header("Request Data")]
    private FriendRequestData requestData;
    private FriendRequestsScreen parentScreen;

    public void Initialize(FriendRequestData request, FriendRequestsScreen screen)
    {
        requestData = request;
        parentScreen = screen;

        // Populate name (from sender)
        if (nameText != null && requestData.sender != null)
        {
            string displayName = GetDisplayName();
            nameText.text = displayName;
        }

        // Setup accept button listener
        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(OnAcceptButtonClick);
            acceptButton.gameObject.SetActive(true);
        }

        // Setup reject button listener
        if (rejectButton != null)
        {
            rejectButton.onClick.RemoveAllListeners();
            rejectButton.onClick.AddListener(OnRejectButtonClick);
            rejectButton.gameObject.SetActive(true);
        }
    }

    private string GetDisplayName()
    {
        if (requestData == null || requestData.sender == null)
            return "Unknown";

        UserData sender = requestData.sender;

        // Construct full name from first_name and last_name
        string firstName = string.IsNullOrEmpty(sender.first_name) ? "" : sender.first_name;
        string lastName = string.IsNullOrEmpty(sender.last_name) ? "" : sender.last_name;
        
        string fullName = $"{firstName} {lastName}".Trim();

        // If both names are empty, use email
        if (string.IsNullOrEmpty(fullName))
        {
            fullName = sender.email;
        }

        return fullName;
    }

    private void OnAcceptButtonClick()
    {
        if (requestData == null)
        {
            Debug.LogError("RequestData is null!");
            return;
        }

        if (parentScreen != null)
        {
            // Disable buttons immediately to prevent multiple clicks
            if (acceptButton != null) acceptButton.interactable = false;
            if (rejectButton != null) rejectButton.interactable = false;

            parentScreen.AcceptFriendRequest(requestData.id, this);
        }
        else
        {
            Debug.LogError("Parent FriendRequestsScreen reference is null!");
        }
    }

    private void OnRejectButtonClick()
    {
        if (requestData == null)
        {
            Debug.LogError("RequestData is null!");
            return;
        }

        if (parentScreen != null)
        {
            // Disable buttons immediately to prevent multiple clicks
            if (acceptButton != null) acceptButton.interactable = false;
            if (rejectButton != null) rejectButton.interactable = false;

            parentScreen.RejectFriendRequest(requestData.id, this);
        }
        else
        {
            Debug.LogError("Parent FriendRequestsScreen reference is null!");
        }
    }
}

