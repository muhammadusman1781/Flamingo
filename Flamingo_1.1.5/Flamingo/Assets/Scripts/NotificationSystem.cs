using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton notification system that handles all notifications in the game
/// </summary>
public class NotificationSystem : MonoBehaviour
{
    public static NotificationSystem Instance { get; private set; }
    
    [Header("Notification UI Components")]
    public NoteUI noteUI;
    
    [Header("Notification Settings")]
    public float autoHideDelay = 5f;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Show a notification with the given parameters
    /// </summary>
    /// <param name="title">Title of the notification</param>
    /// <param name="message">Message content</param>
    /// <param name="numberOfButtons">Number of buttons to show</param>
    /// <param name="buttonsTxt">Text for each button</param>
    /// <param name="OnClickbutton">Actions for each button</param>
    public void ShowNotification(string title, string message, int numberOfButtons, List<string> buttonsTxt, params Action<string>[] OnClickbutton)
    {
        if (noteUI == null)
        {
            Debug.LogError("NotificationSystem: noteUI is not assigned! Please assign the NoteUI in the inspector.");
            return;
        }

        // Null checks for required components
        if (noteUI.titleTMP == null)
        {
            Debug.LogError("NotificationSystem: titleTMP is not assigned in noteUI!");
            return;
        }

        if (noteUI.messageTMP == null)
        {
            Debug.LogError("NotificationSystem: messageTMP is not assigned in noteUI!");
            return;
        }

        if (noteUI.contentParent == null)
        {
            Debug.LogError("NotificationSystem: contentParent is not assigned in noteUI!");
            return;
        }

        if (noteUI.button == null)
        {
            Debug.LogError("NotificationSystem: button prefab is not assigned in noteUI!");
            return;
        }

        // Set the notification content
        noteUI.titleTMP.text = title;
        noteUI.messageTMP.text = message;

        // Clear existing buttons first
        foreach (Transform child in noteUI.contentParent)
        {
            if (child.gameObject != noteUI.button) // Don't destroy the template button
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // Create buttons
        for (int i = 0; i < numberOfButtons; i++)
        {
            var btnGO = Instantiate(noteUI.button, noteUI.contentParent);
            btnGO.SetActive(true);

            var label = btnGO.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null && i < buttonsTxt.Count)
                label.text = buttonsTxt[i];

            var btn = btnGO.GetComponent<Button>();
            int index = i;
            if (index < OnClickbutton.Length && OnClickbutton[index] != null)
                btn.onClick.AddListener(() => OnClickbutton[index]?.Invoke(label.text));
        }
        
        // Show the notification
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Show a simple notification with just an OK button
    /// </summary>
    public void ShowSimpleNotification(string title, string message)
    {
        ShowNotification(title, message, 1, new List<string> { "OK" }, 
            msg => HideNotification());
    }
    
    /// <summary>
    /// Show an error notification
    /// </summary>
    public void ShowErrorNotification(string message)
    {
        ShowSimpleNotification("Error", message);
    }
    
    /// <summary>
    /// Show a warning notification
    /// </summary>
    public void ShowWarningNotification(string message)
    {
        ShowSimpleNotification("Warning", message);
    }
    
    /// <summary>
    /// Show a success notification
    /// </summary>
    public void ShowSuccessNotification(string message)
    {
        ShowSimpleNotification("Success", message);
    }
    
    /// <summary>
    /// Hide the current notification
    /// </summary>
    public void HideNotification()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Check if notification is currently visible
    /// </summary>
    public bool IsNotificationVisible()
    {
        return gameObject.activeInHierarchy;
    }
}
