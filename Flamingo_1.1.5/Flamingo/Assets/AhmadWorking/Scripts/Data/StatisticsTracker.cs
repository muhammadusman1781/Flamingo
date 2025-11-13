using UnityEngine;
using System;
using System.Collections.Generic;

public class StatisticsTracker : MonoBehaviour
{
    public static StatisticsTracker Instance { get; private set; }
    
    [Header("Statistics Configuration")]
    public StatisticsConfig config;
    
    private GameStatistics currentStats;
    
    public event Action<GameStatistics> OnStatisticsUpdated;
    public event Action<QuizResult> OnQuizCompleted;
    public event Action<LevelResult> OnLevelCompleted;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStatisticsTracker();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadStatistics();
    }
    
    private void InitializeStatisticsTracker()
    {
        if (config == null)
        {
            config = CreateDefaultConfig();
        }
        
        currentStats = new GameStatistics();
    }
    
    private StatisticsConfig CreateDefaultConfig()
    {
        return new StatisticsConfig
        {
            defaultFriendsCount = 12,
            enableDetailedTracking = true,
            autoSaveInterval = 30f
        };
    }
    
    #region Quiz Statistics
    
    public void RecordQuizResult(QuizResult result)
    {
        if (result == null) return;
        
        currentStats.totalQuizzesPlayed++;
        currentStats.totalQuizScore += result.score;
        currentStats.totalCorrectAnswers += result.correctAnswers;
        currentStats.totalWrongAnswers += result.wrongAnswers;
        currentStats.totalPlayTime += result.timeSpent;
        
        if (result.isWin)
        {
            currentStats.quizWins++;
            currentStats.currentWinStreak++;
            currentStats.longestWinStreak = Mathf.Max(currentStats.longestWinStreak, currentStats.currentWinStreak);
        }
        else
        {
            currentStats.quizLosses++;
            currentStats.currentWinStreak = 0;
        }
        
        // Update accuracy
        currentStats.overallAccuracy = CalculateOverallAccuracy();
        
        // Update best performance
        UpdateBestPerformance(result);
        
        SaveStatistics();
        OnQuizCompleted?.Invoke(result);
        OnStatisticsUpdated?.Invoke(currentStats);
        
        Debug.Log($"StatisticsTracker: Quiz result recorded - Win: {result.isWin}, Score: {result.score}");
    }
    
    private float CalculateOverallAccuracy()
    {
        int totalAnswers = currentStats.totalCorrectAnswers + currentStats.totalWrongAnswers;
        return totalAnswers > 0 ? (float)currentStats.totalCorrectAnswers / totalAnswers * 100f : 0f;
    }
    
    private void UpdateBestPerformance(QuizResult result)
    {
        if (result.score > currentStats.bestQuizScore)
        {
            currentStats.bestQuizScore = result.score;
        }
        
        if (result.accuracy > currentStats.bestQuizAccuracy)
        {
            currentStats.bestQuizAccuracy = result.accuracy;
        }
        
        if (result.timeSpent < currentStats.fastestQuizTime || currentStats.fastestQuizTime == 0)
        {
            currentStats.fastestQuizTime = result.timeSpent;
        }
    }
    
    #endregion
    
    #region Level Statistics
    
    public void RecordLevelCompletion(LevelResult result)
    {
        if (result == null) return;
        
        if (!currentStats.completedLevels.Contains(result.levelNumber))
        {
            currentStats.completedLevels.Add(result.levelNumber);
            currentStats.levelsCompleted++;
        }
        
        currentStats.totalLevelScore += result.score;
        currentStats.totalLevelTime += result.timeSpent;
        
        if (result.score > currentStats.bestLevelScore)
        {
            currentStats.bestLevelScore = result.score;
        }
        
        SaveStatistics();
        OnLevelCompleted?.Invoke(result);
        OnStatisticsUpdated?.Invoke(currentStats);
        
        Debug.Log($"StatisticsTracker: Level {result.levelNumber} completed");
    }
    
    #endregion
    
    #region Statistics Access
    
    public GameStatistics GetStatistics()
    {
        return currentStats;
    }
    
    public int GetQuizWins()
    {
        return currentStats?.quizWins ?? 0;
    }
    
    public int GetQuizLosses()
    {
        return currentStats?.quizLosses ?? 0;
    }
    
    public int GetTotalQuizzesPlayed()
    {
        return currentStats?.totalQuizzesPlayed ?? 0;
    }
    
    public int GetLevelsCompleted()
    {
        return currentStats?.levelsCompleted ?? 0;
    }
    
    public int GetFriendsCount()
    {
        return config?.defaultFriendsCount ?? 12;
    }
    
    public float GetWinRate()
    {
        if (currentStats == null || currentStats.totalQuizzesPlayed == 0)
            return 0f;
            
        return (float)currentStats.quizWins / currentStats.totalQuizzesPlayed * 100f;
    }
    
    public float GetOverallAccuracy()
    {
        return currentStats?.overallAccuracy ?? 0f;
    }
    
    public List<int> GetCompletedLevels()
    {
        return currentStats?.completedLevels ?? new List<int>();
    }
    
    public bool IsLevelCompleted(int levelNumber)
    {
        return currentStats?.completedLevels.Contains(levelNumber) ?? false;
    }
    
    #endregion
    
    #region Data Persistence
    
    private void SaveStatistics()
    {
        if (currentStats == null) return;
        
        try
        {
            string jsonData = JsonUtility.ToJson(currentStats);
            PlayerPrefs.SetString("GameStatistics", jsonData);
            PlayerPrefs.Save();
            
            Debug.Log("StatisticsTracker: Statistics saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"StatisticsTracker: Failed to save statistics - {e.Message}");
        }
    }
    
    private void LoadStatistics()
    {
        try
        {
            string jsonData = PlayerPrefs.GetString("GameStatistics", "");
            
            if (!string.IsNullOrEmpty(jsonData))
            {
                currentStats = JsonUtility.FromJson<GameStatistics>(jsonData);
                Debug.Log("StatisticsTracker: Statistics loaded successfully");
            }
            else
            {
                currentStats = new GameStatistics();
                Debug.Log("StatisticsTracker: No saved statistics found, using defaults");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"StatisticsTracker: Failed to load statistics - {e.Message}");
            currentStats = new GameStatistics();
        }
    }
    
    #endregion
    
    #region Debug Methods
    
    [ContextMenu("Reset All Statistics")]
    public void ResetAllStatistics()
    {
        currentStats = new GameStatistics();
        PlayerPrefs.DeleteKey("GameStatistics");
        OnStatisticsUpdated?.Invoke(currentStats);
        Debug.Log("StatisticsTracker: All statistics reset");
    }
    
    [ContextMenu("Print Statistics")]
    public void PrintStatistics()
    {
        if (currentStats != null)
        {
            Debug.Log($"StatisticsTracker: Quiz Wins: {currentStats.quizWins}, Losses: {currentStats.quizLosses}");
            Debug.Log($"StatisticsTracker: Levels Completed: {currentStats.levelsCompleted}");
            Debug.Log($"StatisticsTracker: Overall Accuracy: {currentStats.overallAccuracy:F1}%");
        }
        else
        {
            Debug.Log("StatisticsTracker: No statistics available");
        }
    }
    
    #endregion
}

/// <summary>
/// Game statistics data structure
/// </summary>
[System.Serializable]
public class GameStatistics
{
    [Header("Quiz Statistics")]
    public int quizWins;
    public int quizLosses;
    public int totalQuizzesPlayed;
    public int totalQuizScore;
    public int totalCorrectAnswers;
    public int totalWrongAnswers;
    public float overallAccuracy;
    public int bestQuizScore;
    public float bestQuizAccuracy;
    public float fastestQuizTime;
    public int currentWinStreak;
    public int longestWinStreak;
    
    [Header("Level Statistics")]
    public int levelsCompleted;
    public List<int> completedLevels = new List<int>();
    public int totalLevelScore;
    public int bestLevelScore;
    public float totalLevelTime;
    
    [Header("Session Data")]
    public float totalPlayTime;
    public DateTime lastPlayed;
}

/// <summary>
/// Quiz result data
/// </summary>
[System.Serializable]
public class QuizResult
{
    public bool isWin;
    public int score;
    public int correctAnswers;
    public int wrongAnswers;
    public float timeSpent;
    public float accuracy;
    public int levelNumber;
    public DateTime completedAt;
}

/// <summary>
/// Level result data
/// </summary>
[System.Serializable]
public class LevelResult
{
    public int levelNumber;
    public int score;
    public float timeSpent;
    public bool isCompleted;
    public DateTime completedAt;
}

/// <summary>
/// Statistics configuration
/// </summary>
[System.Serializable]
public class StatisticsConfig
{
    public int defaultFriendsCount;
    public bool enableDetailedTracking;
    public float autoSaveInterval;
}
