using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementItemUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image achievementIcon;
    public TextMeshProUGUI achievementNameText;
    public TextMeshProUGUI achievementDescriptionText;
    public GameObject lockedOverlay;
    public Image progressBar;
    public TextMeshProUGUI progressText;
    public GameObject glowEffect;
    public Button achievementButton;
    
    [Header("Default Sprites")]
    public Sprite defaultAchievementIcon;
    public Sprite lockedAchievementIcon;
    
    [Header("Visual Effects")]
    public Color unlockedColor = Color.white;
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);
    public Color progressBarColor = Color.green;
    
    private Achievement currentAchievement;
    
    private void Start()
    {
        SetupButtonListener();
    }
    
    private void SetupButtonListener()
    {
        if (achievementButton != null)
        {
            achievementButton.onClick.AddListener(OnAchievementClicked);
        }
    }
    
    public void SetupAchievement(Achievement achievement)
    {
        if (achievement == null) return;
        
        currentAchievement = achievement;
        UpdateAchievementDisplay();
    }
    
    private void UpdateAchievementDisplay()
    {
        if (currentAchievement == null) return;
        
        // Set achievement icon
        SetAchievementIcon();
        
        // Set achievement name
        SetAchievementName();
        
        // Set achievement description
        SetAchievementDescription();
        
        // Update progress display
        UpdateProgressDisplay();
        
        // Update visual state
        UpdateVisualState();
    }
    
    private void SetAchievementIcon()
    {
        if (achievementIcon != null)
        {
            if (currentAchievement.isUnlocked)
            {
                achievementIcon.sprite = LoadAchievementIcon(currentAchievement.iconName);
            }
            else
            {
                achievementIcon.sprite = lockedAchievementIcon ?? defaultAchievementIcon;
            }
        }
    }
    
    private void SetAchievementName()
    {
        if (achievementNameText != null)
        {
            achievementNameText.text = currentAchievement.title;
        }
    }
    
    private void SetAchievementDescription()
    {
        if (achievementDescriptionText != null)
        {
            achievementDescriptionText.text = currentAchievement.description;
        }
    }
    
    private void UpdateProgressDisplay()
    {
        if (currentAchievement == null) return;
        
        // Calculate progress percentage
        float progressPercentage = currentAchievement.requirement > 0 ? 
            (float)currentAchievement.currentProgress / currentAchievement.requirement : 0f;
        
        // Update progress bar
        if (progressBar != null)
        {
            progressBar.fillAmount = Mathf.Clamp01(progressPercentage);
            progressBar.color = progressBarColor;
        }
        
        // Update progress text
        if (progressText != null)
        {
            if (currentAchievement.isUnlocked)
            {
                progressText.text = "✓ Complete";
            }
            else
            {
                progressText.text = $"{currentAchievement.currentProgress}/{currentAchievement.requirement}";
            }
        }
    }
    
    private void UpdateVisualState()
    {
        // Show/hide locked overlay
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!currentAchievement.isUnlocked);
        }
        
        // Show glow effect for unlocked achievements
        if (glowEffect != null)
        {
            glowEffect.SetActive(currentAchievement.isUnlocked);
        }
        
        // Apply color based on unlock status
        if (achievementIcon != null)
        {
            achievementIcon.color = currentAchievement.isUnlocked ? unlockedColor : lockedColor;
        }
    }
    
    private Sprite LoadAchievementIcon(string iconName)
    {
        if (string.IsNullOrEmpty(iconName)) return defaultAchievementIcon;
        
        // Try to load sprite from Resources folder
        Sprite icon = Resources.Load<Sprite>($"AchievementIcons/{iconName}");
        
        if (icon == null)
        {
            // Try to load from Sprites folder
            icon = Resources.Load<Sprite>($"Sprites/{iconName}");
        }
        
        return icon ?? defaultAchievementIcon;
    }
    
    private void OnAchievementClicked()
    {
        if (currentAchievement != null)
        {
            ShowAchievementDetails();
        }
    }
    
    private void ShowAchievementDetails()
    {
        Debug.Log($"Achievement Details: {currentAchievement.title}\nDescription: {currentAchievement.description}\nProgress: {currentAchievement.currentProgress}/{currentAchievement.requirement}\nUnlocked: {currentAchievement.isUnlocked}");
        
        // TODO: Implement achievement details popup
    }
}
