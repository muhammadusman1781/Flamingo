using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RewardsScreenManager : UIScreen
{
    [Header("Rewards Display")]
    public Transform rewardsScrollContent;
    public GameObject rewardItemPrefab;
    public ScrollRect scrollRect;
    
    [Header("Reward Screens Array")]
    [Tooltip("Array of reward detail screens that can be opened based on reward type")]
    public GameObject[] rewardDetailScreens;
    
    private RewardManager rewardManager;
    private List<GameObject> spawnedRewardItems = new List<GameObject>();
    
    protected override void OnShown()
    {
        base.OnShown();
        InitializeRewardsScreen();
    }
    
    // Removed Start() initialization to avoid conflicts with OnShown()
    // Initialization now only happens in OnShown() when screen is actually displayed
    
    private void InitializeRewardsScreen()
    {
        Debug.Log("RewardsScreenManager: Initializing rewards screen...");
        
        rewardManager = RewardManager.Instance;
        
        if (rewardManager == null)
        {
            Debug.LogError("RewardsScreenManager: RewardManager not found! Make sure RewardManager is in the scene.");
            return;
        }
        
        Debug.Log("RewardsScreenManager: RewardManager found, subscribing to events...");
        
        // Subscribe to reward events to refresh display when counts change
        rewardManager.OnRewardUnlocked += OnRewardChanged;
        rewardManager.OnRewardViewed += OnRewardChanged;
        
        Debug.Log("RewardsScreenManager: Refreshing rewards display...");
        RefreshRewardsDisplay();
        
        Debug.Log("RewardsScreenManager: Initialization complete!");
    }
    
    private void OnRewardChanged(Reward reward)
    {
        RefreshRewardsDisplay();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (rewardManager != null)
        {
            rewardManager.OnRewardUnlocked -= OnRewardChanged;
            rewardManager.OnRewardViewed -= OnRewardChanged;
        }
    }
    
    private void RefreshRewardsDisplay()
    {
        if (rewardsScrollContent == null || rewardItemPrefab == null || rewardManager == null)
        {
            Debug.LogError("RewardsScreenManager: Missing required components!");
            return;
        }
        
        // Clear existing reward items
        ClearRewardItems();
        
        // Get rewards based on current filter
        List<Reward> rewardsToShow = GetFilteredRewards();
        
        // Sort rewards by ID to match Figma design order
        rewardsToShow.Sort((a, b) => a.id.CompareTo(b.id));
        
        // Spawn reward items
        foreach (Reward reward in rewardsToShow)
        {
            CreateRewardItem(reward);
        }
        
        Debug.Log($"RewardsScreenManager: Displayed {rewardsToShow.Count} rewards");
    }
    
    private List<Reward> GetFilteredRewards()
    {
        return rewardManager.GetAllRewards();
    }
    
    private void CreateRewardItem(Reward reward)
    {
        GameObject rewardObj = Instantiate(rewardItemPrefab, rewardsScrollContent);
        RewardItemUI rewardItemUI = rewardObj.GetComponent<RewardItemUI>();
        
        if (rewardItemUI != null)
        {
            rewardItemUI.SetupReward(reward, false);
        }
        
        spawnedRewardItems.Add(rewardObj);
    }
    
    private void ClearRewardItems()
    {
        foreach (GameObject item in spawnedRewardItems)
        {
            if (item != null)
                Destroy(item);
        }
        spawnedRewardItems.Clear();
    }
    
    #region Public Methods
    
    public void RefreshRewards()
    {
        RefreshRewardsDisplay();
    }
    
    public void ShowRewardDetails(Reward reward)
    {
        Debug.Log($"RewardsScreenManager: Showing details for {reward.name}");
        OpenRewardDetailScreen(reward);
    }
    
    public void OpenRewardDetailScreen(Reward reward)
    {
        if (reward == null)
        {
            Debug.LogWarning("RewardsScreenManager: Cannot open detail screen for null reward");
            return;
        }
        
        if (rewardDetailScreens == null || rewardDetailScreens.Length == 0)
        {
            Debug.LogWarning("RewardsScreenManager: No reward detail screens configured");
            return;
        }
        
        GameObject targetScreen = GetRewardDetailScreen(reward);
        
        if (targetScreen != null)
        {
            Hide();
            targetScreen.SetActive(true);
            
            UIScreen targetUIScreen = targetScreen.GetComponent<UIScreen>();
            if (targetUIScreen != null)
            {
                targetUIScreen.Show();
            }
            
            Debug.Log($"RewardsScreenManager: Opened detail screen for {reward.name} (Icon: {reward.iconName})");
        }
        else
        {
            Debug.LogWarning($"RewardsScreenManager: No detail screen found for reward {reward.name} with icon {reward.iconName}");
        }
    }
    
    private GameObject GetRewardDetailScreen(Reward reward)
    {
        if (rewardDetailScreens == null || rewardDetailScreens.Length == 0)
            return null;
        
        string iconName = reward.iconName?.ToLower() ?? "";
        
        if (iconName.Contains("bronze") || iconName.Contains("coda"))
        {
            return GetScreenByIndex(0);
        }
        else if (iconName.Contains("silver") || iconName.Contains("arabic"))
        {
            return GetScreenByIndex(1); // Silver feather detail screen
        }
        else if (iconName.Contains("gold") || iconName.Contains("english"))
        {
            return GetScreenByIndex(2); // Gold feather detail screen
        }
        else if (iconName.Contains("platinum") || iconName.Contains("math"))
        {
            return GetScreenByIndex(3); // Platinum feather detail screen
        }
        else if (iconName.Contains("titanium") || iconName.Contains("science"))
        {
            return GetScreenByIndex(4); // Titanium feather detail screen
        }
        else if (iconName.Contains("diamond") || iconName.Contains("history"))
        {
            return GetScreenByIndex(5); // Diamond feather detail screen
        }
        else if (iconName.Contains("mercury") || iconName.Contains("geography"))
        {
            return GetScreenByIndex(6); // Mercury feather detail screen
        }
        else if (iconName.Contains("ruby") || iconName.Contains("islamic"))
        {
            return GetScreenByIndex(7); // Ruby feather detail screen
        }
        else if (iconName.Contains("emerald") || iconName.Contains("general"))
        {
            return GetScreenByIndex(8); // Emerald feather detail screen
        }
        else if (iconName.Contains("legendary") || iconName.Contains("special"))
        {
            return GetScreenByIndex(9); // Legendary feather detail screen
        }
        
        return GetScreenByIndex(0);
    }

    private GameObject GetScreenByIndex(int index)
    {
        if (rewardDetailScreens != null && index >= 0 && index < rewardDetailScreens.Length)
        {
            return rewardDetailScreens[index];
        }
        return null;
    }
    
    #endregion
}
