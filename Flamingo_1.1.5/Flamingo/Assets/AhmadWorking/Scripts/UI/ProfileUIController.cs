using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ProfileUIController : UIScreen
{
    [Header("Profile Information UI")]
    public TextMeshProUGUI playerNameText;
    public Text playerIdText;
    public Image profilePictureImage;
    
    [Header("Statistics UI")]
    public Text friendsCountText;
    public Text winsCountText;
    public Text lossesCountText;
    
    [Header("Rewards Section UI")]
    public Transform rewardsContainer;
    public GameObject rewardItemPrefab;
    public Button showAllRewardsButton;
    public Text rewardsTitleText;
    
    [Header("Achievements Section UI")]
    public Transform achievementsContainer;
    public GameObject achievementItemPrefab;
    public Button showAllAchievementsButton;
    public Text achievementsTitleText;
    
    [Header("Profile Actions UI")]
    public Button editProfileButton;
    public Button settingsButton;
    
    [Header("Default Values")]
    public string defaultPlayerName = "Player Name";
    public string defaultPlayerId = "757585856494";
    public Sprite defaultProfilePicture;
    
    private ProfileDataManager profileDataManager;
    private StatisticsTracker statisticsTracker;
    private AchievementManager achievementManager;
    private RewardManager rewardManager;
    
    protected override void OnShown()
    {
        base.OnShown();
        RefreshProfileUI();
    }
    
    private void Start()
    {
        InitializeControllers();
        SetupButtonListeners();
        InitializeUI();
    }
    
    private void InitializeControllers()
    {
        profileDataManager = ProfileDataManager.Instance;
        statisticsTracker = StatisticsTracker.Instance;
        achievementManager = AchievementManager.Instance;
        rewardManager = RewardManager.Instance;
        
        // Subscribe to data change events
        if (profileDataManager != null)
        {
            profileDataManager.OnProfileDataChanged += OnProfileDataChanged;
        }
        
        if (statisticsTracker != null)
        {
            statisticsTracker.OnStatisticsUpdated += OnStatisticsUpdated;
        }
        
        if (achievementManager != null)
        {
            achievementManager.OnAchievementUnlocked += OnAchievementUnlocked;
        }
        
        if (rewardManager != null)
        {
            rewardManager.OnRewardUnlocked += OnRewardUnlocked;
        }
    }
    
    private void SetupButtonListeners()
    {
        if (showAllRewardsButton != null)
            showAllRewardsButton.onClick.AddListener(ShowAllRewards);
            
        if (showAllAchievementsButton != null)
            showAllAchievementsButton.onClick.AddListener(ShowAllAchievements);
            
        if (editProfileButton != null)
            editProfileButton.onClick.AddListener(EditProfile);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
    }
    
    private void InitializeUI()
    {
        // Set default values
        SetDefaultUIValues();
        
        // Load current data
        RefreshProfileUI();
    }
    
    private void SetDefaultUIValues()
    {
        if (playerNameText != null)
            playerNameText.text = defaultPlayerName;
            
        if (playerIdText != null)
            playerIdText.text = defaultPlayerId;
            
        if (profilePictureImage != null && defaultProfilePicture != null)
            profilePictureImage.sprite = defaultProfilePicture;
    }
    
    #region UI Refresh Methods
    
    public void RefreshProfileUI()
    {
        RefreshProfileInformation();
        RefreshStatistics();
        RefreshRewards();
        RefreshAchievements();
    }
    
    private void RefreshProfileInformation()
    {
        if (profileDataManager == null) return;
        
        var profileData = profileDataManager.GetCurrentProfile();
        if (profileData == null) return;
        
        // Update player name
        if (playerNameText != null)
        {
            playerNameText.text = profileDataManager.GetPlayerDisplayName();
        }
        
        // Update player ID
        if (playerIdText != null)
        {
            playerIdText.text = profileDataManager.GetPlayerId();
        }
    }
    
    private void RefreshStatistics()
    {
        if (statisticsTracker == null) return;
        
        // Update friends count
        if (friendsCountText != null)
        {
            friendsCountText.text = statisticsTracker.GetFriendsCount().ToString();
        }
        
        // Update wins count
        if (winsCountText != null)
        {
            winsCountText.text = statisticsTracker.GetQuizWins().ToString();
        }
        
        // Update losses count
        if (lossesCountText != null)
        {
            lossesCountText.text = statisticsTracker.GetQuizLosses().ToString();
        }
    }
    
    private void RefreshRewards()
    {
        if (rewardsContainer == null || rewardItemPrefab == null || rewardManager == null) return;
        
        // Clear existing rewards
        ClearContainer(rewardsContainer);
        
        // Show first 3 rewards (Bronze, Silver, Gold) regardless of unlock status
        // This matches the Figma design where these rewards are always visible
        var allRewards = rewardManager.GetAllRewards();
        for (int i = 0; i < Mathf.Min(allRewards.Count, 3); i++)
        {
            CreateRewardItem(allRewards[i]);
        }
    }
    
    private void RefreshAchievements()
    {
        if (achievementsContainer == null || achievementItemPrefab == null || achievementManager == null) return;
        
        // Clear existing achievements
        ClearContainer(achievementsContainer);
        
        // Load first 3 unlocked achievements
        var unlockedAchievements = achievementManager.GetUnlockedAchievements();
        for (int i = 0; i < Mathf.Min(unlockedAchievements.Count, 3); i++)
        {
            CreateAchievementItem(unlockedAchievements[i]);
        }
    }
    
    #endregion
    
    #region UI Creation Methods
    
    private void CreateRewardItem(Reward reward)
    {
        GameObject rewardObj = Instantiate(rewardItemPrefab, rewardsContainer);
        RewardItemUI rewardItemUI = rewardObj.GetComponent<RewardItemUI>();
        
        if (rewardItemUI != null)
        {
            rewardItemUI.SetupReward(reward, true);
        }
    }
    
    private void CreateAchievementItem(Achievement achievement)
    {
        GameObject achievementObj = Instantiate(achievementItemPrefab, achievementsContainer);
        AchievementItemUI achievementItemUI = achievementObj.GetComponent<AchievementItemUI>();
        
        if (achievementItemUI != null)
        {
            achievementItemUI.SetupAchievement(achievement);
        }
    }
    
    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            if (child != container)
                Destroy(child.gameObject);
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnProfileDataChanged(ProfileData profileData)
    {
        RefreshProfileInformation();
    }
    
    private void OnStatisticsUpdated(GameStatistics statistics)
    {
        RefreshStatistics();
    }
    
    private void OnAchievementUnlocked(Achievement achievement)
    {
        RefreshAchievements();
    }
    
    private void OnRewardUnlocked(Reward reward)
    {
        RefreshRewards();
    }
    
    #endregion
    
    #region Button Event Handlers
    
    private void ShowAllRewards()
    {
        Debug.Log("ShowAllRewards clicked - Navigate to full rewards screen");
        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.Show(ScreenId.Rewards);
        }
    }
    
    private void ShowAllAchievements()
    {
        Debug.Log("ShowAllAchievements clicked - Navigate to full achievements screen");
        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.Show(ScreenId.Achievements);
        }
    }
    
    private void EditProfile()
    {
        Debug.Log("EditProfile clicked - Navigate to profile edit screen");
        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.Show(ScreenId.StudentInfo);
        }
    }
    
    private void OpenSettings()
    {
        Debug.Log("OpenSettings clicked - Navigate to settings screen");
        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.Show(ScreenId.Settings);
        }
    }
    
    #endregion
    
    #region Cleanup
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (profileDataManager != null)
        {
            profileDataManager.OnProfileDataChanged -= OnProfileDataChanged;
        }
        
        if (statisticsTracker != null)
        {
            statisticsTracker.OnStatisticsUpdated -= OnStatisticsUpdated;
        }
        
        if (achievementManager != null)
        {
            achievementManager.OnAchievementUnlocked -= OnAchievementUnlocked;
        }
        
        if (rewardManager != null)
        {
            rewardManager.OnRewardUnlocked -= OnRewardUnlocked;
        }
    }
    
    #endregion
}
