using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }
    
    [Header("Achievement Configuration")]
    public AchievementConfig config;
    
    private List<Achievement> allAchievements;
    private List<Achievement> unlockedAchievements;
    
    public event Action<Achievement> OnAchievementUnlocked;
    public event Action<Achievement> OnAchievementProgressUpdated;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAchievementManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadAchievements();
        SubscribeToEvents();
    }
    
    private void InitializeAchievementManager()
    {
        if (config == null)
        {
            config = CreateDefaultConfig();
        }
        
        allAchievements = new List<Achievement>();
        unlockedAchievements = new List<Achievement>();
        
        CreateAchievementsFromFigma();
    }
    
    private AchievementConfig CreateDefaultConfig()
    {
        return new AchievementConfig
        {
            enableNotifications = true,
            autoSaveProgress = true,
            checkInterval = 1f
        };
    }
    
    #region Achievement Creation
    
    private void CreateAchievementsFromFigma()
    {
        // Row 1 Achievements
        CreateAchievement("the_sharp_one", "The Sharp One", "Win your first quiz", 
            AchievementType.QuizWin, 1, 100, "achievement_sharp");
        
        CreateAchievement("the_clever_one", "The Clever One", "Win 5 quizzes", 
            AchievementType.QuizWin, 5, 200, "achievement_clever");
        
        CreateAchievement("the_smart_one", "The Smart One", "Get a perfect score in any quiz", 
            AchievementType.PerfectScore, 1, 300, "achievement_smart");
        
        // Row 2 Achievements
        CreateAchievement("the_persistent_one", "The Persistent One", "Complete 10 quizzes", 
            AchievementType.QuizComplete, 10, 250, "achievement_persistent");
        
        CreateAchievement("the_super_smart", "The Super Smart", "Get 90% accuracy in 5 quizzes", 
            AchievementType.HighAccuracy, 5, 400, "achievement_super_smart");
        
        CreateAchievement("the_answer_master", "The Answer Master", "Answer 100 questions correctly", 
            AchievementType.CorrectAnswers, 100, 500, "achievement_answer_master");
        
        // Row 3 Achievements
        CreateAchievement("fast_as_falcon", "Fast as a Falcon", "Complete a quiz in under 30 seconds", 
            AchievementType.SpeedRun, 1, 300, "achievement_fast_falcon");
        
        CreateAchievement("the_unbeatable", "The Unbeatable", "Win 20 quizzes in a row", 
            AchievementType.WinStreak, 20, 1000, "achievement_unbeatable");
        
        CreateAchievement("the_amazing_friend", "The Amazing Friend", "Complete 5 levels", 
            AchievementType.LevelComplete, 5, 200, "achievement_amazing_friend");
        
        // Row 4 Achievements
        CreateAchievement("the_loyal_friend", "The Loyal Friend", "Complete 10 levels", 
            AchievementType.LevelComplete, 10, 300, "achievement_loyal_friend");
        
        CreateAchievement("the_friendship_king", "The Friendship King", "Complete 25 levels", 
            AchievementType.LevelComplete, 25, 500, "achievement_friendship_king");
        
        CreateAchievement("the_first_star", "The First Star", "Earn your first star rating", 
            AchievementType.Special, 1, 100, "achievement_first_star");
        
        // Row 5 Achievements
        CreateAchievement("challenge_lover", "Challenge Lover", "Complete 50 levels", 
            AchievementType.LevelComplete, 50, 750, "achievement_challenge_lover");
        
        CreateAchievement("the_advanced_expert", "The Advanced Expert", "Complete 100 levels", 
            AchievementType.LevelComplete, 100, 1000, "achievement_advanced_expert");
        
        CreateAchievement("level_king", "Level King", "Complete 150 levels", 
            AchievementType.LevelComplete, 150, 1500, "achievement_level_king");
        
        // Row 6 Achievements
        CreateAchievement("level_emperor", "Level Emperor", "Complete 200 levels", 
            AchievementType.LevelComplete, 200, 2000, "achievement_level_emperor");
        
        CreateAchievement("the_challenger", "The Challenger", "Complete 250 levels", 
            AchievementType.LevelComplete, 250, 2500, "achievement_challenger");
        
        CreateAchievement("the_champion_challenger", "The Champion Challenger", "Complete 300 levels", 
            AchievementType.LevelComplete, 300, 3000, "achievement_champion_challenger");
        
        // Row 7 Achievements
        CreateAchievement("the_fierce_challenger", "The Fierce Challenger", "Complete 400 levels", 
            AchievementType.LevelComplete, 400, 4000, "achievement_fierce_challenger");
        
        CreateAchievement("the_legend_of_challenge", "The Legend of Challenge", "Complete 500 levels", 
            AchievementType.LevelComplete, 500, 5000, "achievement_legend_challenge");
        
        Debug.Log($"AchievementManager: Created {allAchievements.Count} achievements");
    }
    
    private void CreateAchievement(string id, string title, string description, 
        AchievementType type, int requirement, int rewardPoints, string iconName)
    {
        Achievement achievement = new Achievement
        {
            id = id,
            title = title,
            description = description,
            type = type,
            requirement = requirement,
            rewardPoints = rewardPoints,
            iconName = iconName,
            currentProgress = 0,
            isUnlocked = false,
            unlockedAt = DateTime.MinValue
        };
        
        allAchievements.Add(achievement);
    }
    
    #endregion
    
    #region Achievement Updates
    
    public void UpdateQuizAchievements(QuizResult quizResult)
    {
        if (quizResult == null) return;
        
        UpdateAchievementProgress("the_sharp_one", quizResult.isWin ? 1 : 0);
        UpdateAchievementProgress("the_clever_one", quizResult.isWin ? 1 : 0);
        UpdateAchievementProgress("the_persistent_one", 1);
        UpdateAchievementProgress("the_answer_master", quizResult.correctAnswers);
        UpdateAchievementProgress("the_unbeatable", quizResult.isWin ? 1 : 0);
        
        if (quizResult.accuracy >= 100f)
        {
            UpdateAchievementProgress("the_smart_one", 1);
        }
        
        if (quizResult.accuracy >= 90f)
        {
            UpdateAchievementProgress("the_super_smart", 1);
        }
        
        if (quizResult.timeSpent <= 30f)
        {
            UpdateAchievementProgress("fast_as_falcon", 1);
        }
    }
    
    public void UpdateLevelAchievements(LevelResult levelResult)
    {
        if (levelResult == null) return;
        
        UpdateAchievementProgress("the_amazing_friend", 1);
        UpdateAchievementProgress("the_loyal_friend", 1);
        UpdateAchievementProgress("the_friendship_king", 1);
        UpdateAchievementProgress("challenge_lover", 1);
        UpdateAchievementProgress("the_advanced_expert", 1);
        UpdateAchievementProgress("level_king", 1);
        UpdateAchievementProgress("level_emperor", 1);
        UpdateAchievementProgress("the_challenger", 1);
        UpdateAchievementProgress("the_champion_challenger", 1);
        UpdateAchievementProgress("the_fierce_challenger", 1);
        UpdateAchievementProgress("the_legend_of_challenge", 1);
    }
    
    private void UpdateAchievementProgress(string achievementId, int progress)
    {
        Achievement achievement = GetAchievementById(achievementId);
        if (achievement == null || achievement.isUnlocked) return;
        
        achievement.currentProgress += progress;
        OnAchievementProgressUpdated?.Invoke(achievement);
        
        if (achievement.currentProgress >= achievement.requirement)
        {
            UnlockAchievement(achievement);
        }
    }
    
    private void UnlockAchievement(Achievement achievement)
    {
        if (achievement.isUnlocked) return;
        
        achievement.isUnlocked = true;
        achievement.unlockedAt = DateTime.Now;
        
        if (!unlockedAchievements.Contains(achievement))
        {
            unlockedAchievements.Add(achievement);
        }
        
        // Award reward points
        AwardRewardPoints(achievement.rewardPoints);
        
        // Show notification
        ShowAchievementNotification(achievement);
        
        // Save achievements
        if (config.autoSaveProgress)
        {
            SaveAchievements();
        }
        
        OnAchievementUnlocked?.Invoke(achievement);
        Debug.Log($"AchievementManager: Achievement unlocked - {achievement.title}");
    }
    
    #endregion
    
    #region Achievement Access
    
    public Achievement GetAchievementById(string id)
    {
        return allAchievements.FirstOrDefault(a => a.id == id);
    }
    
    public List<Achievement> GetAllAchievements()
    {
        return new List<Achievement>(allAchievements);
    }
    
    public List<Achievement> GetUnlockedAchievements()
    {
        return new List<Achievement>(unlockedAchievements);
    }
    
    public List<Achievement> GetAchievementsByType(AchievementType type)
    {
        return allAchievements.Where(a => a.type == type).ToList();
    }
    
    public int GetUnlockedAchievementCount()
    {
        return unlockedAchievements.Count;
    }
    
    public int GetTotalAchievementCount()
    {
        return allAchievements.Count;
    }
    
    public float GetAchievementProgressPercentage()
    {
        if (allAchievements.Count == 0) return 0f;
        return (float)unlockedAchievements.Count / allAchievements.Count * 100f;
    }
    
    #endregion
    
    #region Event Handling
    
    private void SubscribeToEvents()
    {
        if (StatisticsTracker.Instance != null)
        {
            StatisticsTracker.Instance.OnQuizCompleted += UpdateQuizAchievements;
            StatisticsTracker.Instance.OnLevelCompleted += UpdateLevelAchievements;
        }
    }
    
    private void OnDestroy()
    {
        if (StatisticsTracker.Instance != null)
        {
            StatisticsTracker.Instance.OnQuizCompleted -= UpdateQuizAchievements;
            StatisticsTracker.Instance.OnLevelCompleted -= UpdateLevelAchievements;
        }
    }
    
    #endregion
    
    #region Reward Integration
    
    private void AwardRewardPoints(int points)
    {
        if (ProfileDataManager.Instance != null)
        {
            var currentProfile = ProfileDataManager.Instance.GetCurrentProfile();
            if (currentProfile != null)
            {
                ProfileDataManager.Instance.UpdatePoints(currentProfile.points + points);
            }
        }
    }
    
    #endregion
    
    #region Notifications
    
    private void ShowAchievementNotification(Achievement achievement)
    {
        if (!config.enableNotifications) return;
        
        Debug.Log($"🎉 Achievement Unlocked: {achievement.title} - {achievement.description}");
        // TODO: Implement UI notification system
    }
    
    #endregion
    
    #region Data Persistence
    
    private void SaveAchievements()
    {
        try
        {
            AchievementSaveData saveData = new AchievementSaveData
            {
                unlockedIds = unlockedAchievements.Select(a => a.id).ToList(),
                progressData = allAchievements.ToDictionary(a => a.id, a => a.currentProgress)
            };
            
            string jsonData = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("AchievementData", jsonData);
            PlayerPrefs.Save();
            
            Debug.Log("AchievementManager: Achievements saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"AchievementManager: Failed to save achievements - {e.Message}");
        }
    }
    
    private void LoadAchievements()
    {
        try
        {
            string jsonData = PlayerPrefs.GetString("AchievementData", "");
            
            if (!string.IsNullOrEmpty(jsonData))
            {
                AchievementSaveData saveData = JsonUtility.FromJson<AchievementSaveData>(jsonData);
                
                unlockedAchievements.Clear();
                foreach (string id in saveData.unlockedIds)
                {
                    Achievement achievement = GetAchievementById(id);
                    if (achievement != null)
                    {
                        achievement.isUnlocked = true;
                        unlockedAchievements.Add(achievement);
                    }
                }
                
                // Restore progress data
                foreach (var kvp in saveData.progressData)
                {
                    Achievement achievement = GetAchievementById(kvp.Key);
                    if (achievement != null)
                    {
                        achievement.currentProgress = kvp.Value;
                    }
                }
                
                Debug.Log($"AchievementManager: Loaded {unlockedAchievements.Count} unlocked achievements");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"AchievementManager: Failed to load achievements - {e.Message}");
        }
    }
    
    #endregion
    
    #region Debug Methods
    
    [ContextMenu("Reset All Achievements")]
    public void ResetAllAchievements()
    {
        foreach (var achievement in allAchievements)
        {
            achievement.isUnlocked = false;
            achievement.currentProgress = 0;
            achievement.unlockedAt = DateTime.MinValue;
        }
        
        unlockedAchievements.Clear();
        PlayerPrefs.DeleteKey("AchievementData");
        Debug.Log("AchievementManager: All achievements reset");
    }
    
    [ContextMenu("Unlock All Achievements")]
    public void UnlockAllAchievements()
    {
        foreach (var achievement in allAchievements)
        {
            if (!achievement.isUnlocked)
            {
                UnlockAchievement(achievement);
            }
        }
    }
    
    #endregion
}

/// <summary>
/// Achievement data structure
/// </summary>
[System.Serializable]
public class Achievement
{
    public string id;
    public string title;
    public string description;
    public AchievementType type;
    public int requirement;
    public int currentProgress;
    public bool isUnlocked;
    public int rewardPoints;
    public string iconName;
    public DateTime unlockedAt;
}

/// <summary>
/// Achievement types
/// </summary>
public enum AchievementType
{
    QuizWin,
    QuizComplete,
    PerfectScore,
    HighAccuracy,
    CorrectAnswers,
    SpeedRun,
    WinStreak,
    LevelComplete,
    Special
}

/// <summary>
/// Achievement configuration
/// </summary>
[System.Serializable]
public class AchievementConfig
{
    public bool enableNotifications;
    public bool autoSaveProgress;
    public float checkInterval;
}

/// <summary>
/// Achievement save data
/// </summary>
[System.Serializable]
public class AchievementSaveData
{
    public List<string> unlockedIds = new List<string>();
    public Dictionary<string, int> progressData = new Dictionary<string, int>();
}
