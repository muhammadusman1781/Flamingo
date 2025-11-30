using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;

public class AchievementsDetailsScreen : MonoBehaviour
{
    [Header("Navigation")]
    public Button backButton;
    public GameObject profileScreen;
    
    [Header("Achievement Detail Panel")]
    public GameObject achievementDetailPanel;
    public RTLTextMeshPro achievementNameText;
    public RTLTextMeshPro achievementDescriptionText;
    public RTLTextMeshPro achievementRewardText;
    public Image achievementDetailIcon;
    public Button closeDetailButton;
    
    [Header("Achievement Buttons - All 25 in Order")]
    [Tooltip("Add all 25 achievement buttons in order")]
    public List<Button> achievementButtons = new List<Button>();
    
    [Header("Completed Sprites - All 25 in Order")]
    [Tooltip("Add all 25 completed sprites in same order as buttons")]
    public List<Sprite> completedSprites = new List<Sprite>();
    
    [Header("Not Completed Sprites - All 25 in Order")]
    [Tooltip("Add all 25 not completed sprites in same order as buttons")]
    public List<Sprite> notCompletedSprites = new List<Sprite>();
    
    // Internal data
    private List<string> achievementNames = new List<string>();
    
    void Start()
    {
        InitializeAchievementButtons();
        UpdateAllAchievementStates();
        
        if (achievementDetailPanel != null)
        {
            achievementDetailPanel.SetActive(false);
        }
        
        if (closeDetailButton != null)
        {
            closeDetailButton.onClick.AddListener(CloseDetailPanel);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }
    
    void OnEnable()
    {
        // Update achievement states when screen is opened
        UpdateAllAchievementStates();
    }
    
    void InitializeAchievementButtons()
    {
        if (AchievementsManager.Instance == null)
        {
            Debug.LogError("AchievementsManager instance not found! Make sure it exists in the scene.");
            return;
        }
        
        // Get all achievements from the manager
        List<AchievementData> allAchievements = AchievementsManager.Instance.allAchievements;
        
        // Validation checks
        if (achievementButtons.Count != allAchievements.Count)
        {
            Debug.LogWarning($"Achievement buttons count ({achievementButtons.Count}) does not match achievements count ({allAchievements.Count})");
        }
        
        if (completedSprites.Count != allAchievements.Count)
        {
            Debug.LogWarning($"Completed sprites count ({completedSprites.Count}) does not match achievements count ({allAchievements.Count})");
        }
        
        if (notCompletedSprites.Count != allAchievements.Count)
        {
            Debug.LogWarning($"Not completed sprites count ({notCompletedSprites.Count}) does not match achievements count ({allAchievements.Count})");
        }
        
        // Clear and rebuild achievement names list
        achievementNames.Clear();
        
        // Automatically assign achievement names and add click listeners
        for (int i = 0; i < achievementButtons.Count && i < allAchievements.Count; i++)
        {
            if (achievementButtons[i] != null)
            {
                // Store the achievement name
                achievementNames.Add(allAchievements[i].name);
                
                // Add click listener
                int index = i; // Capture index for lambda
                achievementButtons[i].onClick.AddListener(() => ShowAchievementDetailByIndex(index));
            }
            else
            {
                achievementNames.Add("");
            }
        }
        
        Debug.Log($"Initialized {achievementButtons.Count} achievement buttons");
    }
    
    void UpdateAllAchievementStates()
    {
        if (AchievementsManager.Instance == null)
        {
            Debug.LogWarning("AchievementsManager instance not found!");
            return;
        }
        
        for (int i = 0; i < achievementButtons.Count && i < achievementNames.Count; i++)
        {
            UpdateAchievementButtonState(i);
        }
    }
    
    void UpdateAchievementButtonState(int index)
    {
        if (index < 0 || index >= achievementButtons.Count || 
            index >= achievementNames.Count ||
            achievementButtons[index] == null)
            return;
        
        // Get the image component from the button
        Image buttonImage = achievementButtons[index].GetComponent<Image>();
        if (buttonImage == null)
        {
            // Try to find image in children
            buttonImage = achievementButtons[index].GetComponentInChildren<Image>();
        }
        
        if (buttonImage == null)
            return;
        
        // Check if achievement is completed
        bool isCompleted = AchievementsManager.Instance.IsAchievementCompleted(achievementNames[index]);
        
        // Set appropriate sprite
        if (isCompleted)
        {
            if (index < completedSprites.Count && completedSprites[index] != null)
            {
                buttonImage.sprite = completedSprites[index];
            }
        }
        else
        {
            if (index < notCompletedSprites.Count && notCompletedSprites[index] != null)
            {
                buttonImage.sprite = notCompletedSprites[index];
            }
        }
    }
    
    void ShowAchievementDetailByIndex(int index)
    {
        if (index < 0 || index >= achievementNames.Count)
        {
            Debug.LogError("Invalid achievement index: " + index);
            return;
        }
        
        ShowAchievementDetail(achievementNames[index], index);
    }
    
    void ShowAchievementDetail(string achievementName, int index)
    {
        if (AchievementsManager.Instance == null)
        {
            Debug.LogWarning("AchievementsManager instance not found!");
            return;
        }
        
        AchievementData achievement = AchievementsManager.Instance.GetAchievement(achievementName);
        
        if (achievement == null)
        {
            Debug.LogError("Achievement not found: " + achievementName);
            return;
        }
        
        // Update detail panel text
        if (achievementNameText != null)
        {
            achievementNameText.text = achievement.name;
        }
        
        if (achievementDescriptionText != null)
        {
            achievementDescriptionText.text = achievement.description;
        }
        
        if (achievementRewardText != null)
        {
            achievementRewardText.text = achievement.coinReward + " coins";
        }
        
        // Update detail icon with the specific sprite for this achievement
        if (achievementDetailIcon != null)
        {
            bool isCompleted = AchievementsManager.Instance.IsAchievementCompleted(achievementName);
            
            if (isCompleted && index < completedSprites.Count && completedSprites[index] != null)
            {
                achievementDetailIcon.sprite = completedSprites[index];
                achievementDetailIcon.SetNativeSize();
            }
            else if (!isCompleted && index < notCompletedSprites.Count && notCompletedSprites[index] != null)
            {
                achievementDetailIcon.sprite = notCompletedSprites[index];
                achievementDetailIcon.SetNativeSize();
            }
        }
        
        // Show the detail panel
        if (achievementDetailPanel != null)
        {
            achievementDetailPanel.SetActive(true);
        }
        
        Debug.Log("Showing details for achievement: " + achievementName);
    }
    
    void CloseDetailPanel()
    {
        if (achievementDetailPanel != null)
        {
            achievementDetailPanel.SetActive(false);
        }
    }
    
    void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked - Returning to Profile Screen");
        
        // Close detail panel if open
        if (achievementDetailPanel != null && achievementDetailPanel.activeSelf)
        {
            achievementDetailPanel.SetActive(false);
        }
        
        // Hide achievements screen
        gameObject.SetActive(false);
        
        // Show profile screen
        if (profileScreen != null)
        {
            profileScreen.SetActive(true);
        }
        else
        {
            Debug.LogError("Profile Screen reference not set!");
        }
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        foreach (var button in achievementButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
        
        if (closeDetailButton != null)
        {
            closeDetailButton.onClick.RemoveAllListeners();
        }
        
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
        }
    }
}

    
