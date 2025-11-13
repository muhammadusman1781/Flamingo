 using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
    public NoteUI noteUI;
    [HideInInspector] public GameObject noteSpawned;

    public void createNotification(string title, string message, int numberOfButtons, List<string> buttonsTxt, params Action<string>[] OnClickbutton)
    {
        // Null check for noteUI
        if (noteUI == null)
        {
            Debug.LogError("NotificationManager: noteUI is not assigned! Please assign the NoteUI in the inspector.");
            return;
        }

        // Null checks for required components
        if (noteUI.titleTMP == null)
        {
            Debug.LogError("NotificationManager: titleTMP is not assigned in noteUI!");
            return;
        }

        if (noteUI.messageTMP == null)
        {
            Debug.LogError("NotificationManager: messageTMP is not assigned in noteUI!");
            return;
        }

        if (noteUI.contentParent == null)
        {
            Debug.LogError("NotificationManager: contentParent is not assigned in noteUI!");
            return;
        }

        if (noteUI.button == null)
        {
            Debug.LogError("NotificationManager: button prefab is not assigned in noteUI!");
            return;
        }

        // Instantiate the Note prefab if it exists
        if (noteUI.NotePrefab != null)
        {
            noteSpawned = Instantiate(noteUI.NotePrefab);
            noteSpawned.SetActive(true);
            
            Debug.Log($"NotificationManager: Instantiated NotePrefab: {noteSpawned.name}");
            
            // Use the existing NoteUI references but find matching components in the instantiated prefab
            // This approach uses the existing references as templates to find the right components
            
            TextMeshProUGUI instantiatedTitleTMP = null;
            TextMeshProUGUI instantiatedMessageTMP = null;
            Transform instantiatedContentParent = null;
            GameObject instantiatedButton = null;
            
            // Find components by matching names or hierarchy structure
            if (noteUI.titleTMP != null)
            {
                // Try to find a TMP component with similar name or position
                var allTMPs = noteSpawned.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var tmp in allTMPs)
                {
                    if (tmp.name.ToLower().Contains("title") || tmp.transform.GetSiblingIndex() == 0)
                    {
                        instantiatedTitleTMP = tmp;
                        break;
                    }
                }
                if (instantiatedTitleTMP == null && allTMPs.Length > 0)
                    instantiatedTitleTMP = allTMPs[0]; // Fallback to first TMP
            }
            
            if (noteUI.messageTMP != null)
            {
                var allTMPs = noteSpawned.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var tmp in allTMPs)
                {
                    if (tmp.name.ToLower().Contains("message") || tmp.name.ToLower().Contains("content"))
                    {
                        instantiatedMessageTMP = tmp;
                        break;
                    }
                }
                if (instantiatedMessageTMP == null && allTMPs.Length > 1)
                    instantiatedMessageTMP = allTMPs[1]; // Fallback to second TMP
            }
            
            if (noteUI.contentParent != null)
            {
                // Try to find a RectTransform that could be the content parent
                var allRectTransforms = noteSpawned.GetComponentsInChildren<RectTransform>();
                Debug.Log($"NotificationManager: Found {allRectTransforms.Length} RectTransforms in prefab");
                
                foreach (var rt in allRectTransforms)
                {
                    Debug.Log($"NotificationManager: Checking RectTransform: {rt.name}");
                    if (rt.name.ToLower().Contains("content") || rt.name.ToLower().Contains("button") || rt.name.ToLower().Contains("panel"))
                    {
                        instantiatedContentParent = rt;
                        Debug.Log($"NotificationManager: Selected content parent: {rt.name}");
                        break;
                    }
                }
                
                // If we didn't find a specific content parent, try to find one with layout components
                if (instantiatedContentParent == null)
                {
                    var layoutGroups = noteSpawned.GetComponentsInChildren<UnityEngine.UI.VerticalLayoutGroup>();
                    if (layoutGroups.Length > 0)
                    {
                        instantiatedContentParent = layoutGroups[0].GetComponent<RectTransform>();
                        Debug.Log($"NotificationManager: Using VerticalLayoutGroup as content parent: {instantiatedContentParent.name}");
                    }
                    else
                    {
                        var horizontalLayoutGroups = noteSpawned.GetComponentsInChildren<UnityEngine.UI.HorizontalLayoutGroup>();
                        if (horizontalLayoutGroups.Length > 0)
                        {
                            instantiatedContentParent = horizontalLayoutGroups[0].GetComponent<RectTransform>();
                            Debug.Log($"NotificationManager: Using HorizontalLayoutGroup as content parent: {instantiatedContentParent.name}");
                        }
                    }
                }
            }
            
            // Use the assigned button template from the inspector
            if (noteUI.button != null)
            {
                instantiatedButton = noteUI.button;
                Debug.Log($"NotificationManager: Using assigned button template: {instantiatedButton.name}");
            }
            else
            {
                Debug.LogError("NotificationManager: No button template assigned in the inspector!");
                
                // Try to find any button in the prefab as fallback
                var allButtons = noteSpawned.GetComponentsInChildren<Button>();
                Debug.Log($"NotificationManager: Found {allButtons.Length} Button components in prefab as fallback");
                
                if (allButtons.Length > 0)
                {
                    instantiatedButton = allButtons[0].gameObject;
                    Debug.Log($"NotificationManager: Using fallback button: {instantiatedButton.name}");
                }
                else
                {
                    Debug.LogError("NotificationManager: No buttons found in prefab either!");
                }
            }
            
            // Set the text content
            if (instantiatedTitleTMP != null)
            {
                instantiatedTitleTMP.text = title;
                Debug.Log($"NotificationManager: Set title to '{title}' on {instantiatedTitleTMP.name}");
            }
            
            if (instantiatedMessageTMP != null)
            {
                instantiatedMessageTMP.text = message;
                Debug.Log($"NotificationManager: Set message to '{message}' on {instantiatedMessageTMP.name}");
            }
            
            // Debug: Log what we found
            Debug.Log($"NotificationManager: Found components - Title: {instantiatedTitleTMP?.name}, Message: {instantiatedMessageTMP?.name}, ContentParent: {instantiatedContentParent?.name}, Button: {instantiatedButton?.name}");
            
            // Create buttons if we found the content parent
            if (instantiatedContentParent != null && instantiatedButton != null)
            {
                Debug.Log($"NotificationManager: Creating {numberOfButtons} buttons in content parent: {instantiatedContentParent.name}");
                
                // Clear existing buttons first
                var existingButtons = new List<Transform>();
                foreach (Transform child in instantiatedContentParent)
                {
                    if (child.gameObject != instantiatedButton)
                    {
                        existingButtons.Add(child);
                    }
                }
                
                foreach (var child in existingButtons)
                {
                    DestroyImmediate(child.gameObject);
                }
                
                Debug.Log($"NotificationManager: Cleared {existingButtons.Count} existing buttons");

                // Create buttons
                for (int i = 0; i < numberOfButtons; i++)
                {
                    Debug.Log($"NotificationManager: Creating button {i + 1}/{numberOfButtons}");
                    
                    var btnGO = Instantiate(instantiatedButton, instantiatedContentParent);
                    btnGO.SetActive(true);
                    btnGO.name = $"Button_{i}"; // Give it a unique name

                    var label = btnGO.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (label != null && i < buttonsTxt.Count)
                    {
                        label.text = buttonsTxt[i];
                        Debug.Log($"NotificationManager: Set button {i + 1} text to '{buttonsTxt[i]}'");
                    }

                    var btn = btnGO.GetComponent<Button>();
                    if (btn != null)
                    {
                        int index = i;
                        if (index < OnClickbutton.Length && OnClickbutton[index] != null)
                        {
                            btn.onClick.AddListener(() => 
                            {
                                Debug.Log($"NotificationManager: Button {index + 1} clicked with text: {label.text}");
                                OnClickbutton[index]?.Invoke(label.text);
                            });
                            Debug.Log($"NotificationManager: Added click listener to button {i + 1}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"NotificationManager: Button component not found on instantiated button {i + 1}");
                    }
                }
                
                Debug.Log($"NotificationManager: Successfully created {numberOfButtons} buttons");
            }
            else
            {
                Debug.LogError($"NotificationManager: Could not create buttons - ContentParent: {instantiatedContentParent?.name}, Button: {instantiatedButton?.name}");
                
                // Try alternative approach - create buttons without content parent
                if (instantiatedButton != null)
                {
                    Debug.Log("NotificationManager: Trying alternative button creation approach");
                    
                    for (int i = 0; i < numberOfButtons; i++)
                    {
                        var btnGO = Instantiate(instantiatedButton);
                        btnGO.SetActive(true);
                        btnGO.name = $"Button_{i}";
                        
                        // Position it manually
                        var rectTransform = btnGO.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            rectTransform.SetParent(noteSpawned.transform, false);
                            rectTransform.anchoredPosition = new Vector2(0, -50 * i); // Stack buttons vertically
                        }

                        var label = btnGO.GetComponentInChildren<TextMeshProUGUI>(true);
                        if (label != null && i < buttonsTxt.Count)
                        {
                            label.text = buttonsTxt[i];
                            Debug.Log($"NotificationManager: Set alternative button {i + 1} text to '{buttonsTxt[i]}'");
                        }

                        var btn = btnGO.GetComponent<Button>();
                        if (btn != null)
                        {
                            int index = i;
                            if (index < OnClickbutton.Length && OnClickbutton[index] != null)
                            {
                                btn.onClick.AddListener(() => 
                                {
                                    Debug.Log($"NotificationManager: Alternative button {index + 1} clicked");
                                    OnClickbutton[index]?.Invoke(label.text);
                                });
                            }
                        }
                    }
                    
                    Debug.Log($"NotificationManager: Created {numberOfButtons} alternative buttons");
                }
            }
        }
        else
        {
            // Fallback: Use the existing UI components directly
            Debug.LogWarning("NotificationManager: NotePrefab is null, using existing UI components directly");
            noteSpawned = gameObject;
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
        }
    }

    public void removeNotification()
    {
        if (noteSpawned != null && noteSpawned != gameObject)
        {
            Destroy(noteSpawned);
            noteSpawned = null;
        }
        else if (noteSpawned == gameObject)
        {
            // If using the fallback method, just hide the notification
            if (noteUI != null && noteUI.NotePrefab != null)
            {
                noteUI.NotePrefab.SetActive(false);
            }
        }
    }
    
    private bool IsPersistent(GameObject obj)
    {
        // Check if the object has DontDestroyOnLoad applied
        return obj.hideFlags == HideFlags.DontSaveInEditor || 
               obj.hideFlags == HideFlags.DontSaveInBuild ||
               obj.hideFlags == HideFlags.DontSave;
    }
}

[System.Serializable]
public class NoteUI
{
    public GameObject NotePrefab;
    public TextMeshProUGUI titleTMP;
    public TextMeshProUGUI messageTMP;
    public Transform contentParent;
    public GameObject button;
}