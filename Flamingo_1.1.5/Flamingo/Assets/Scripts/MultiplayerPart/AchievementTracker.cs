using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks achievement progress and triggers achievement unlocks
/// </summary>
public class AchievementTracker : MonoBehaviour
{
    public static AchievementTracker Instance;
    
    // PlayerPrefs Keys
    private const string PREF_CONSECUTIVE_CORRECT_NO_HELP = "Achievement_ConsecutiveCorrectNoHelp";
    private const string PREF_STAGES_COMPLETED_NO_HELP = "Achievement_StagesNoHelp";
    private const string PREF_TOTAL_STAGES_COMPLETED = "Achievement_TotalStages";
    private const string PREF_MULTIPLAYER_CONSECUTIVE_WINS = "Achievement_MultiplayerWins";
    private const string PREF_HELP_USED_CURRENT_STAGE = "Achievement_HelpUsedThisStage";
    
    // Current session tracking
    private int consecutiveCorrectWithoutHelp = 0;
    private int stagesCompletedWithoutHelp = 0;
    private bool helpUsedInCurrentStage = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void LoadProgress()
    {
        consecutiveCorrectWithoutHelp = PlayerPrefs.GetInt(PREF_CONSECUTIVE_CORRECT_NO_HELP, 0);
        stagesCompletedWithoutHelp = PlayerPrefs.GetInt(PREF_STAGES_COMPLETED_NO_HELP, 0);
        helpUsedInCurrentStage = PlayerPrefs.GetInt(PREF_HELP_USED_CURRENT_STAGE, 0) == 1;
        
        Debug.Log($"AchievementTracker: Loaded progress - Consecutive: {consecutiveCorrectWithoutHelp}, Stages no help: {stagesCompletedWithoutHelp}");
    }
    
    /// <summary>
    /// Call when a question is answered correctly without using help
    /// </summary>
    public void OnQuestionAnsweredCorrectWithoutHelp()
    {
        if (helpUsedInCurrentStage)
        {
            Debug.Log("AchievementTracker: Help was used, not counting for achievements");
            return;
        }
        
        consecutiveCorrectWithoutHelp++;
        PlayerPrefs.SetInt(PREF_CONSECUTIVE_CORRECT_NO_HELP, consecutiveCorrectWithoutHelp);
        PlayerPrefs.Save();
        
        Debug.Log($"AchievementTracker: Consecutive correct without help: {consecutiveCorrectWithoutHelp}");
        
        // Check achievements
        CheckQuestionBasedAchievements();
    }
    
    /// <summary>
    /// Call when a question is answered incorrectly or help is used
    /// </summary>
    public void OnQuestionAnsweredIncorrectOrHelpUsed()
    {
        consecutiveCorrectWithoutHelp = 0;
        PlayerPrefs.SetInt(PREF_CONSECUTIVE_CORRECT_NO_HELP, 0);
        PlayerPrefs.Save();
        
        Debug.Log("AchievementTracker: Streak reset");
    }
    
    /// <summary>
    /// Call when help is used (remove 2 options, hint, skip)
    /// </summary>
    public void OnHelpUsed()
    {
        helpUsedInCurrentStage = true;
        PlayerPrefs.SetInt(PREF_HELP_USED_CURRENT_STAGE, 1);
        consecutiveCorrectWithoutHelp = 0;
        PlayerPrefs.SetInt(PREF_CONSECUTIVE_CORRECT_NO_HELP, 0);
        PlayerPrefs.Save();
        
        Debug.Log("AchievementTracker: Help used - streaks reset");
    }
    
    /// <summary>
    /// Call when a new stage/level starts
    /// </summary>
    public void OnStageStarted()
    {
        helpUsedInCurrentStage = false;
        PlayerPrefs.SetInt(PREF_HELP_USED_CURRENT_STAGE, 0);
        PlayerPrefs.Save();
        
        Debug.Log("AchievementTracker: New stage started, help flag reset");
    }
    
    /// <summary>
    /// Call when a stage/level is completed
    /// </summary>
    public void OnStageCompleted()
    {
        // Track total stages
        int totalStages = PlayerPrefs.GetInt(PREF_TOTAL_STAGES_COMPLETED, 0);
        totalStages++;
        PlayerPrefs.SetInt(PREF_TOTAL_STAGES_COMPLETED, totalStages);
        
        // Track stages without help
        if (!helpUsedInCurrentStage)
        {
            stagesCompletedWithoutHelp++;
            PlayerPrefs.SetInt(PREF_STAGES_COMPLETED_NO_HELP, stagesCompletedWithoutHelp);
            
            Debug.Log($"AchievementTracker: Stage completed without help! Total: {stagesCompletedWithoutHelp}");
            CheckStageBasedAchievementsNoHelp();
        }
        else
        {
            // Reset consecutive stages without help
            stagesCompletedWithoutHelp = 0;
            PlayerPrefs.SetInt(PREF_STAGES_COMPLETED_NO_HELP, 0);
        }
        
        PlayerPrefs.Save();
        
        Debug.Log($"AchievementTracker: Total stages completed: {totalStages}");
        CheckStageBasedAchievements(totalStages);
        
        // Reset for next stage
        helpUsedInCurrentStage = false;
        PlayerPrefs.SetInt(PREF_HELP_USED_CURRENT_STAGE, 0);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Call when multiplayer match is won
    /// </summary>
    public void OnMultiplayerWin()
    {
        int consecutiveWins = PlayerPrefs.GetInt(PREF_MULTIPLAYER_CONSECUTIVE_WINS, 0);
        consecutiveWins++;
        PlayerPrefs.SetInt(PREF_MULTIPLAYER_CONSECUTIVE_WINS, consecutiveWins);
        PlayerPrefs.Save();
        
        Debug.Log($"AchievementTracker: Multiplayer consecutive wins: {consecutiveWins}");
        CheckMultiplayerAchievements(consecutiveWins);
    }
    
    /// <summary>
    /// Call when multiplayer match is lost
    /// </summary>
    public void OnMultiplayerLoss()
    {
        PlayerPrefs.SetInt(PREF_MULTIPLAYER_CONSECUTIVE_WINS, 0);
        PlayerPrefs.Save();
        
        Debug.Log("AchievementTracker: Multiplayer win streak reset");
    }
    
    // Achievement checking methods
    
    private void CheckQuestionBasedAchievements()
    {
        if (AchievementsManager.Instance == null) return;
        
        // Achievement 1: The Sharp - Answer 3 questions in a row without help
        if (consecutiveCorrectWithoutHelp == 3)
        {
            UnlockAchievement("The Sharp");
        }
        
        // Achievement 2: The Clever - Answer 5 questions in a row without help
        if (consecutiveCorrectWithoutHelp == 5)
        {
            UnlockAchievement("The Clever");
        }
    }
    
    private void CheckStageBasedAchievementsNoHelp()
    {
        if (AchievementsManager.Instance == null) return;
        
        // Achievement 3: The Smart - Complete 2 stages without help
        if (stagesCompletedWithoutHelp == 2)
        {
            UnlockAchievement("The Smart");
        }
        
        // Achievement 4: The Persistent - Complete 5 consecutive stages without help
        if (stagesCompletedWithoutHelp == 5)
        {
            UnlockAchievement("The Persistent");
        }
    }
    
    private void CheckStageBasedAchievements(int totalStages)
    {
        if (AchievementsManager.Instance == null) return;
        
        // Achievement 5: The Super Smart - Complete 20 stages
        if (totalStages == 20)
        {
            UnlockAchievement("The Super Smart");
        }
        
        // Achievement 6: The Answer Master - Complete 40 stages
        if (totalStages == 40)
        {
            UnlockAchievement("The Answer Master");
        }
        
        // Achievement 8: The Perfect Player - Complete 150 stages
        if (totalStages == 150)
        {
            UnlockAchievement("The Perfect Player");
        }
    }
    
    private void CheckMultiplayerAchievements(int consecutiveWins)
    {
        if (AchievementsManager.Instance == null) return;
        
        // Achievement 9: The Unbeatable - Defeat 10 opponents without losing
        if (consecutiveWins == 10)
        {
            UnlockAchievement("The Unbeatable");
        }
        
        // Achievement 10: Top Star - Defeat 20 opponents without losing
        if (consecutiveWins == 20)
        {
            UnlockAchievement("Top Star");
        }
    }
    
    private void UnlockAchievement(string achievementName)
    {
        if (AchievementsManager.Instance == null)
        {
            Debug.LogError("AchievementTracker: AchievementsManager not found!");
            return;
        }
        
        // Check if already unlocked
        if (AchievementsManager.Instance.IsAchievementCompleted(achievementName))
        {
            Debug.Log($"AchievementTracker: Achievement '{achievementName}' already unlocked");
            return;
        }
        
        Debug.Log($"🏆 AchievementTracker: Unlocking achievement '{achievementName}'!");
        
        AchievementsManager.Instance.AddAchievementToProfile(
            achievementName,
            onSuccess: () => {
                Debug.Log($"✅ Achievement '{achievementName}' unlocked successfully!");
                ShowAchievementUnlockedNotification(achievementName);
            },
            onError: (error) => {
                Debug.LogError($"❌ Failed to unlock achievement '{achievementName}': {error}");
            }
        );
    }
    
    private void ShowAchievementUnlockedNotification(string achievementName)
    {
        // TODO: Show a nice popup/notification to the player
        Debug.Log($"🎉 ACHIEVEMENT UNLOCKED: {achievementName}");
        
        // You can integrate with NotificationManager here if needed
    }
    
    // Public getters for debugging
    public int GetConsecutiveCorrectWithoutHelp() => consecutiveCorrectWithoutHelp;
    public int GetStagesCompletedWithoutHelp() => stagesCompletedWithoutHelp;
    public int GetTotalStagesCompleted() => PlayerPrefs.GetInt(PREF_TOTAL_STAGES_COMPLETED, 0);
    public int GetMultiplayerConsecutiveWins() => PlayerPrefs.GetInt(PREF_MULTIPLAYER_CONSECUTIVE_WINS, 0);
    
    // Reset methods (for testing or debugging)
    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey(PREF_CONSECUTIVE_CORRECT_NO_HELP);
        PlayerPrefs.DeleteKey(PREF_STAGES_COMPLETED_NO_HELP);
        PlayerPrefs.DeleteKey(PREF_TOTAL_STAGES_COMPLETED);
        PlayerPrefs.DeleteKey(PREF_MULTIPLAYER_CONSECUTIVE_WINS);
        PlayerPrefs.DeleteKey(PREF_HELP_USED_CURRENT_STAGE);
        PlayerPrefs.Save();
        
        consecutiveCorrectWithoutHelp = 0;
        stagesCompletedWithoutHelp = 0;
        helpUsedInCurrentStage = false;
        
        Debug.Log("AchievementTracker: All progress reset");
    }
}

