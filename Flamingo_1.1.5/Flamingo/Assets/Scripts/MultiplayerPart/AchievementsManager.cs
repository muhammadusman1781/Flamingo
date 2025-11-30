using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AchievementData
{
    public string name;
    public string description;
    public int coinReward;
    
    public AchievementData(string name, string description, int coinReward)
    {
        this.name = name;
        this.description = description;
        this.coinReward = coinReward;
    }
}

public class AchievementsManager : MonoBehaviour
{
    public static AchievementsManager Instance;
    
    // All achievements in the game
    public List<AchievementData> allAchievements = new List<AchievementData>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeAchievements()
    {
        allAchievements = new List<AchievementData>
        {
            // Achievement 1
            new AchievementData(
                "The Sharp",
                "Answer 3 questions in a row without using any help",
                6
            ),
            
            // Achievement 2
            new AchievementData(
                "The Clever",
                "Answer 5 questions in a row without using any help",
                10
            ),
            
            // Achievement 3
            new AchievementData(
                "The Smart",
                "Complete 2 stages without using any help",
                20
            ),
            
            // Achievement 4
            new AchievementData(
                "The Persistent",
                "Complete 5 consecutive stages without using any help",
                50
            ),
            
            // Achievement 5
            new AchievementData(
                "The Super Smart",
                "Complete 20 stages",
                60
            ),
            
            // Achievement 6
            new AchievementData(
                "The Answer Master",
                "Complete 40 stages",
                120
            ),
            
            // Achievement 7
            new AchievementData(
                "Fast as a Falcon",
                "Complete 80 stages within one week",
                240
            ),
            
            // Achievement 8
            new AchievementData(
                "The Perfect Player",
                "Complete 150 stages",
                100
            ),
            
            // Achievement 9
            new AchievementData(
                "The Unbeatable",
                "Defeat 10 opponents without losing",
                200
            ),
            
            // Achievement 10
            new AchievementData(
                "Top Star",
                "Defeat 20 opponents without losing",
                250
            ),
            
            // Achievement 11
            new AchievementData(
                "Challenge Lover",
                "Play 30 times on a Sunday",
                800
            ),
            
            // Achievement 12
            new AchievementData(
                "Awesome Friend",
                "Add 5 friends in the game",
                500
            ),
            
            // Achievement 13
            new AchievementData(
                "Loyal Friend",
                "Add 20 friends in the game",
                20
            ),
            
            // Achievement 14
            new AchievementData(
                "King of Friendship",
                "Add 50 friends in the game",
                50
            ),
            
            // Achievement 15
            new AchievementData(
                "Advanced Expert",
                "Complete 25 levels",
                500
            ),
            
            // Achievement 16
            new AchievementData(
                "Level King",
                "Complete 50 levels",
                250
            ),
            
            // Achievement 17
            new AchievementData(
                "Level Emperor",
                "Complete 100 levels",
                300
            ),
            
            // Achievement 18
            new AchievementData(
                "Golden Flamingo",
                "Log in for 30 consecutive days",
                600
            ),
            
            // Achievement 19
            new AchievementData(
                "Platinum Flamingo",
                "Log in for 90 days",
                900
            ),
            
            // Achievement 20
            new AchievementData(
                "Diamond Flamingo",
                "Log in for 180 days",
                1500
            ),
            
            // Achievement 21
            new AchievementData(
                "Legendary Flamingo",
                "Log in for 365 days",
                10000
            ),
            
            // Achievement 22
            new AchievementData(
                "The Challenger",
                "Win 5 matches in the Elite League",
                50
            ),
            
            // Achievement 23
            new AchievementData(
                "Champion Challenger",
                "Win 10 Elite League matches without losing",
                200
            ),
            
            // Achievement 24
            new AchievementData(
                "Fierce Challenger",
                "Win 25 Elite League matches without losing",
                1000
            ),
            
            // Achievement 25
            new AchievementData(
                "Legend of Challenge",
                "Win 50 Elite League matches without losing",
                2500
            )
        };
        
        Debug.Log("Achievements Manager initialized with " + allAchievements.Count + " achievements");
    }
    
    /// <summary>
    /// Check if an achievement is completed by the user
    /// </summary>
    public bool IsAchievementCompleted(string achievementName)
    {
        if (NetworkingHandler.instance == null || 
            NetworkingHandler.instance.serverConstants == null || 
            NetworkingHandler.instance.serverConstants.FullUserProfile == null)
        {
            return false;
        }
        
        var userAchievements = NetworkingHandler.instance.serverConstants.FullUserProfile.achievements;
        
        if (userAchievements == null)
        {
            return false;
        }
        
        return userAchievements.Contains(achievementName);
    }
    
    /// <summary>
    /// Get achievement by name
    /// </summary>
    public AchievementData GetAchievement(string achievementName)
    {
        return allAchievements.Find(a => a.name == achievementName);
    }
    
    /// <summary>
    /// Add achievement to user profile via API
    /// </summary>
    public void AddAchievementToProfile(string achievementName, System.Action onSuccess = null, System.Action<string> onError = null)
    {
        AchievementData achievement = GetAchievement(achievementName);
        
        if (achievement == null)
        {
            Debug.LogError("Achievement not found: " + achievementName);
            onError?.Invoke("Achievement not found");
            return;
        }
        
        // Check if already completed
        if (IsAchievementCompleted(achievementName))
        {
            Debug.Log("Achievement already completed: " + achievementName);
            onError?.Invoke("Achievement already completed");
            return;
        }
        
        string apiUrl = NetworkingHandler.instance.serverConstants.baseUrl + "/auth/achievement/add/";
        
        AddAchievementRequest request = new AddAchievementRequest();
        request.achievement = achievement.name;
        request.coin = achievement.coinReward;
        
        string jsonData = JsonUtility.ToJson(request);
        
        Debug.Log("Adding achievement: " + achievement.name + " with reward: " + achievement.coinReward + " coins");
        
        NetworkingHandler.instance.postMessage(
            apiUrl,
            jsonData,
            true, // Token needed
            (response) => OnAchievementAdded(response, achievementName, onSuccess),
            (error) => OnAchievementAddFailed(error, onError)
        );
    }
    
    void OnAchievementAdded(string response, string achievementName, System.Action onSuccess)
    {
        Debug.Log("Achievement added successfully: " + response);
        
        try
        {
            AddAchievementResponse apiResponse = JsonUtility.FromJson<AddAchievementResponse>(response);
            
            if (apiResponse.status == "success")
            {
                // Update local profile
                if (NetworkingHandler.instance != null && 
                    NetworkingHandler.instance.serverConstants != null && 
                    NetworkingHandler.instance.serverConstants.FullUserProfile != null)
                {
                    if (NetworkingHandler.instance.serverConstants.FullUserProfile.achievements == null)
                    {
                        NetworkingHandler.instance.serverConstants.FullUserProfile.achievements = new List<string>();
                    }
                    
                    if (!NetworkingHandler.instance.serverConstants.FullUserProfile.achievements.Contains(achievementName))
                    {
                        NetworkingHandler.instance.serverConstants.FullUserProfile.achievements.Add(achievementName);
                    }
                    
                    // Update coins in profile
                    AchievementData achievement = GetAchievement(achievementName);
                    if (achievement != null)
                    {
                        NetworkingHandler.instance.serverConstants.FullUserProfile.points += achievement.coinReward;
                    }
                }
                
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError("Failed to add achievement: " + apiResponse.message);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing achievement response: " + e.Message);
        }
    }
    
    void OnAchievementAddFailed(string error, System.Action<string> onError)
    {
        Debug.LogError("Failed to add achievement: " + error);
        onError?.Invoke(error);
    }
    
    /// <summary>
    /// Get list of completed achievements
    /// </summary>
    public List<string> GetCompletedAchievements()
    {
        if (NetworkingHandler.instance == null || 
            NetworkingHandler.instance.serverConstants == null || 
            NetworkingHandler.instance.serverConstants.FullUserProfile == null)
        {
            return new List<string>();
        }
        
        return NetworkingHandler.instance.serverConstants.FullUserProfile.achievements ?? new List<string>();
    }
    
    /// <summary>
    /// Get count of completed achievements
    /// </summary>
    public int GetCompletedAchievementsCount()
    {
        return GetCompletedAchievements().Count;
    }
    
    /// <summary>
    /// Get total possible achievements count
    /// </summary>
    public int GetTotalAchievementsCount()
    {
        return allAchievements.Count;
    }
}
