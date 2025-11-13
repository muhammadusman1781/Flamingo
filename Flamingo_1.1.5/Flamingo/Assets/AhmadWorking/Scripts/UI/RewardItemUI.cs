using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardItemUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image rewardIcon;
    public Image displayCaseImage;
    public TextMeshProUGUI rewardNameText;
    public TextMeshProUGUI rewardDescriptionText;
    public TextMeshProUGUI countText; // New count text component
    public GameObject glowEffect;
    public Button rewardButton;
    
    [Header("Display Case Sprites")]
    public Sprite bronzeCaseSprite;
    public Sprite silverCaseSprite;
    public Sprite goldCaseSprite;
    public Sprite platinumCaseSprite;
    public Sprite titaniumCaseSprite;
    public Sprite diamondCaseSprite;
    public Sprite mercuryCaseSprite;
    public Sprite rubyCaseSprite;
    public Sprite emeraldCaseSprite;
    public Sprite legendaryCaseSprite;
    
    [Header("Default Sprites")]
    public Sprite defaultRewardIcon;
    
    [Header("Visual Effects")]
    public Color unlockedColor = Color.white;
    public Color countTextColor = Color.white;
    
    [Header("Layout Settings")]
    public Vector2 preferredSize = new Vector2(200f, 250f);
    
    [Header("Profile Display Settings")]
    public bool isProfileDisplay = false; // Set to true when used in profile screen
    
    private Reward currentReward;
    
    private void Start()
    {
        SetupButtonListener();
    }
    
    private void SetupButtonListener()
    {
        if (rewardButton != null)
        {
            rewardButton.onClick.AddListener(OnRewardClicked);
        }
    }
    
    public void SetupReward(Reward reward, bool profileDisplay = false)
    {
        if (reward == null) return;
        
        currentReward = reward;
        isProfileDisplay = profileDisplay;
        SetupLayout();
        UpdateRewardDisplay();
    }
    
    private void SetupLayout()
    {
        // Set preferred size for grid layout
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = preferredSize;
        }
    }
    
    private void UpdateRewardDisplay()
    {
        if (currentReward == null) return;
        
        // Set reward icon
        SetRewardIcon();
        
        // Set display case
        SetDisplayCase();
        
        // Set reward name
        SetRewardName();
        
        // Set reward description
        SetRewardDescription();
        
        // Set count text
        SetCountText();
        
        // Update visual state
        UpdateVisualState();
        
        // Apply rarity effects
        ApplyRarityEffects();
    }
    
    private void SetRewardIcon()
    {
        if (rewardIcon != null)
        {
            // Always show the feather icon, regardless of count
            rewardIcon.sprite = LoadFeatherIcon(currentReward.iconName);
        }
    }
    
    private void SetDisplayCase()
    {
        if (displayCaseImage != null)
        {
            // Always show the proper display case, regardless of count
            displayCaseImage.sprite = GetDisplayCaseSprite(currentReward.caseName);
        }
    }
    
    private void SetRewardName()
    {
        if (rewardNameText != null)
        {
            // Only show reward name if not in profile display mode
            rewardNameText.text = isProfileDisplay ? "" : currentReward.name;
        }
    }
    
    private void SetRewardDescription()
    {
        if (rewardDescriptionText != null)
        {
            // Only show reward description if not in profile display mode
            rewardDescriptionText.text = isProfileDisplay ? "" : currentReward.description;
        }
    }
    
    private void SetCountText()
    {
        if (countText != null)
        {
            // Only show count text if not in profile display mode
            if (isProfileDisplay)
            {
                countText.gameObject.SetActive(false);
            }
            else
            {
                countText.text = currentReward.count.ToString();
                countText.color = countTextColor;
                countText.gameObject.SetActive(true);
            }
        }
    }
    
    private void UpdateVisualState()
    {
        if (currentReward == null) return;
        
        // Always show glow effect and full color for all rewards
        if (glowEffect != null)
        {
            glowEffect.SetActive(true);
        }
        
        // Always show full color for reward icon
        if (rewardIcon != null)
        {
            rewardIcon.color = unlockedColor;
        }
    }
    
    private Sprite GetDisplayCaseSprite(string caseName)
    {
        switch (caseName)
        {
            case "bronze_case": return bronzeCaseSprite;
            case "silver_case": return silverCaseSprite;
            case "gold_case": return goldCaseSprite;
            case "platinum_case": return platinumCaseSprite;
            case "titanium_case": return titaniumCaseSprite;
            case "diamond_case": return diamondCaseSprite;
            case "mercury_case": return mercuryCaseSprite;
            case "ruby_case": return rubyCaseSprite;
            case "emerald_case": return emeraldCaseSprite;
            case "legendary_case": return legendaryCaseSprite;
            default: return bronzeCaseSprite;
        }
    }
    
    private Sprite LoadFeatherIcon(string iconName)
    {
        if (string.IsNullOrEmpty(iconName)) return defaultRewardIcon;
        
        // Try to load sprite from Resources folder
        Sprite icon = Resources.Load<Sprite>($"FeatherIcons/{iconName}");
        
        if (icon == null)
        {
            // Try to load from Sprites folder
            icon = Resources.Load<Sprite>($"Sprites/{iconName}");
        }
        
        return icon ?? defaultRewardIcon;
    }
    
    private void ApplyRarityEffects()
    {
        if (currentReward == null) return;
        
        // Apply visual effects based on rarity
        switch (currentReward.rarity)
        {
            case RewardRarity.Common:
                // Bronze/gray tint
                break;
            case RewardRarity.Rare:
                // Silver tint
                break;
            case RewardRarity.Epic:
                // Gold tint
                break;
            case RewardRarity.Legendary:
                // Rainbow/glowing effect
                break;
        }
    }
    
    private void OnRewardClicked()
    {
        if (currentReward != null)
        {
            if (isProfileDisplay)
            {
                // In profile display mode, clicking any reward opens the full rewards screen
                OpenFullRewardsScreen();
            }
            else
            {
                // In full rewards screen, show individual reward details
                ShowRewardDetails();
            }
        }
    }
    
    private void OpenFullRewardsScreen()
    {
        Debug.Log($"RewardItemUI: Opening full rewards screen from profile");
        
        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.Show(ScreenId.Rewards);
        }
        else
        {
            Debug.LogWarning("RewardItemUI: ScreenManager not found - cannot open rewards screen");
        }
    }
    
    private void ShowRewardDetails()
    {
        Debug.Log($"Reward Details: {currentReward.name}\nDescription: {currentReward.description}\nRegion: {currentReward.region}\nRarity: {currentReward.rarity}");
        
        // Mark as viewed
        if (RewardManager.Instance != null)
        {
            RewardManager.Instance.MarkRewardAsViewed(currentReward);
        }
        
        OpenRewardDetailScreen();
    }
    
    private void OpenRewardDetailScreen()
    {
        if (currentReward == null)
        {
            Debug.LogWarning("RewardItemUI: Cannot open detail screen - no current reward");
            return;
        }
        
        // Find the RewardsScreenManager in the scene
        RewardsScreenManager rewardsScreenManager = FindObjectOfType<RewardsScreenManager>();
        
        if (rewardsScreenManager != null)
        {
            rewardsScreenManager.OpenRewardDetailScreen(currentReward);
        }
        else
        {
            Debug.LogWarning("RewardItemUI: RewardsScreenManager not found in scene");
        }
    }
}
