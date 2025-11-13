using UnityEngine;

public class ProfileSystemInitializer : MonoBehaviour
{
    [Header("System Prefabs")]
    public GameObject profileDataManagerPrefab;
    public GameObject statisticsTrackerPrefab;
    public GameObject achievementManagerPrefab;
    public GameObject rewardManagerPrefab;
    
    [Header("Initialization Settings")]
    public bool autoInitialize = true;
    public bool initializeOnAwake = true;
    public float initializationDelay = 0.1f;
    
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    
    private bool isInitialized = false;
    
    private void Awake()
    {
        if (initializeOnAwake)
        {
            InitializeAllSystems();
        }
    }
    
    private void Start()
    {
        if (autoInitialize && !isInitialized)
        {
            if (initializationDelay > 0)
            {
                Invoke(nameof(InitializeAllSystems), initializationDelay);
            }
            else
            {
                InitializeAllSystems();
            }
        }
    }
    
    public void InitializeAllSystems()
    {
        if (isInitialized)
        {
            LogDebug("ProfileSystemInitializer: Systems already initialized");
            return;
        }
        
        LogDebug("ProfileSystemInitializer: Starting system initialization...");
        
        // Initialize systems in dependency order
        InitializeProfileDataManager();
        InitializeStatisticsTracker();
        InitializeAchievementManager();
        InitializeRewardManager();
        
        isInitialized = true;
        LogDebug("ProfileSystemInitializer: All systems initialized successfully");
    }
    
    private void InitializeProfileDataManager()
    {
        if (ProfileDataManager.Instance == null)
        {
            GameObject managerObj;
            if (profileDataManagerPrefab != null)
            {
                managerObj = Instantiate(profileDataManagerPrefab);
            }
            else
            {
                managerObj = new GameObject("ProfileDataManager");
                managerObj.AddComponent<ProfileDataManager>();
            }
            
            DontDestroyOnLoad(managerObj);
            LogDebug("ProfileSystemInitializer: ProfileDataManager initialized");
        }
        else
        {
            LogDebug("ProfileSystemInitializer: ProfileDataManager already exists");
        }
    }
    
    private void InitializeStatisticsTracker()
    {
        if (StatisticsTracker.Instance == null)
        {
            GameObject trackerObj;
            if (statisticsTrackerPrefab != null)
            {
                trackerObj = Instantiate(statisticsTrackerPrefab);
            }
            else
            {
                trackerObj = new GameObject("StatisticsTracker");
                trackerObj.AddComponent<StatisticsTracker>();
            }
            
            DontDestroyOnLoad(trackerObj);
            LogDebug("ProfileSystemInitializer: StatisticsTracker initialized");
        }
        else
        {
            LogDebug("ProfileSystemInitializer: StatisticsTracker already exists");
        }
    }
    
    private void InitializeAchievementManager()
    {
        if (AchievementManager.Instance == null)
        {
            GameObject managerObj;
            if (achievementManagerPrefab != null)
            {
                managerObj = Instantiate(achievementManagerPrefab);
            }
            else
            {
                managerObj = new GameObject("AchievementManager");
                managerObj.AddComponent<AchievementManager>();
            }
            
            DontDestroyOnLoad(managerObj);
            LogDebug("ProfileSystemInitializer: AchievementManager initialized");
        }
        else
        {
            LogDebug("ProfileSystemInitializer: AchievementManager already exists");
        }
    }
    
    private void InitializeRewardManager()
    {
        if (RewardManager.Instance == null)
        {
            GameObject managerObj;
            if (rewardManagerPrefab != null)
            {
                managerObj = Instantiate(rewardManagerPrefab);
            }
            else
            {
                managerObj = new GameObject("RewardManager");
                managerObj.AddComponent<RewardManager>();
            }
            
            DontDestroyOnLoad(managerObj);
            LogDebug("ProfileSystemInitializer: RewardManager initialized");
        }
        else
        {
            LogDebug("ProfileSystemInitializer: RewardManager already exists");
        }
    }
    
    #region Public Methods
    
    public void InitializeProfileDataManagerOnly()
    {
        InitializeProfileDataManager();
    }
    
    public void InitializeStatisticsTrackerOnly()
    {
        InitializeStatisticsTracker();
    }
    
    public void InitializeAchievementManagerOnly()
    {
        InitializeAchievementManager();
    }
    
    public void InitializeRewardManagerOnly()
    {
        InitializeRewardManager();
    }
    
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    public void ForceReinitialize()
    {
        isInitialized = false;
        InitializeAllSystems();
    }
    
    #endregion
    
    #region Debug Methods
    
    [ContextMenu("Check System Status")]
    public void CheckSystemStatus()
    {
        LogDebug("=== Profile System Status ===");
        LogDebug($"ProfileDataManager: {(ProfileDataManager.Instance != null ? "✓ Initialized" : "✗ Not Initialized")}");
        LogDebug($"StatisticsTracker: {(StatisticsTracker.Instance != null ? "✓ Initialized" : "✗ Not Initialized")}");
        LogDebug($"AchievementManager: {(AchievementManager.Instance != null ? "✓ Initialized" : "✗ Not Initialized")}");
        LogDebug($"RewardManager: {(RewardManager.Instance != null ? "✓ Initialized" : "✗ Not Initialized")}");
        LogDebug($"Overall Status: {(isInitialized ? "✓ All Systems Ready" : "✗ Systems Not Ready")}");
    }
    
    [ContextMenu("Test System Integration")]
    public void TestSystemIntegration()
    {
        LogDebug("=== Testing System Integration ===");
        
        // Test ProfileDataManager
        if (ProfileDataManager.Instance != null)
        {
            var profile = ProfileDataManager.Instance.GetCurrentProfile();
            LogDebug($"ProfileDataManager: Profile loaded - {profile != null}");
        }
        
        // Test StatisticsTracker
        if (StatisticsTracker.Instance != null)
        {
            var stats = StatisticsTracker.Instance.GetStatistics();
            LogDebug($"StatisticsTracker: Statistics loaded - {stats != null}");
        }
        
        // Test AchievementManager
        if (AchievementManager.Instance != null)
        {
            var achievements = AchievementManager.Instance.GetAllAchievements();
            LogDebug($"AchievementManager: Achievements loaded - {achievements.Count} total");
        }
        
        // Test RewardManager
        if (RewardManager.Instance != null)
        {
            var rewards = RewardManager.Instance.GetAllRewards();
            LogDebug($"RewardManager: Rewards loaded - {rewards.Count} total");
        }
    }
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
    
    #endregion
}
