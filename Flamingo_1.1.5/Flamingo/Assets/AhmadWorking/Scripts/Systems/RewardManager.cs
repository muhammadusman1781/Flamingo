using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }
    
    [Header("Reward Configuration")]
    public RewardConfig config;
    
    private List<Reward> allRewards;
    private List<Reward> unlockedRewards;
    
    public event Action<Reward> OnRewardUnlocked;
    public event Action<Reward> OnRewardViewed;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeRewardManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadRewards();
        SubscribeToEvents();
    }
    
    private void InitializeRewardManager()
    {
        if (config == null)
        {
            config = CreateDefaultConfig();
        }
        
        allRewards = new List<Reward>();
        unlockedRewards = new List<Reward>();
        
        CreateFeatherRewardsFromFigma();
    }
    
    private RewardConfig CreateDefaultConfig()
    {
        return new RewardConfig
        {
            enableNotifications = true,
            autoSaveProgress = true,
            showRewardDetails = true
        };
    }
    
    #region Reward Creation
    
    private void CreateFeatherRewardsFromFigma()
    {
        // Bronze Feather - Coda
        CreateReward(1, "Bronze Feather", "Coda", 
            "The rhythm of Coda's past and the echo of its future. Known for its unwavering loyalty and steadfast influence, the Bronze Feather was awarded to members of the tribe who demonstrated exceptional patience, bravery, and a profound understanding of the cycles of life and death.",
            RewardRarity.Common, "the_sharp_one", "feather_bronze", "bronze_case");
        
        // Silver Feather - Argentius
        CreateReward(2, "Silver Feather", "Argentius",
            "The symbol of change and the essence of the Paradox. Once a bond between two entities, the Silver Feather was entrusted to shamans and warriors who embraced the balance between friendly and strength.",
            RewardRarity.Rare, "the_clever_one", "feather_silver", "silver_case");
        
        // Golden Feather - Narius
        CreateReward(3, "Golden Feather", "Narius",
            "The spirit of the sun and the light of the heavens. From the heart of the golden plains, where bountiful energy thrives, the Golden Feather was given to priests and healers.",
            RewardRarity.Epic, "the_smart_one", "feather_golden", "gold_case");
        
        // Platinum Feather - Quintera
        CreateReward(4, "Platinum Feather", "Quintera",
            "The secret of the stars and the whispers of the cosmos. Forged in the deepest mines where precious minerals glow under the earth, the Platinum Feather was bestowed upon scholars and mystics.",
            RewardRarity.Epic, "the_persistent_one", "feather_platinum", "platinum_case");
        
        // Titanium Feather - Sylva
        CreateReward(5, "Titanium Feather", "Sylva",
            "The strength of the mountains and the resilience of the forest. Crafted from the heart of ancient trees and infused with the spirit of the earth, the Titanium Feather was given to guardians and protectors.",
            RewardRarity.Rare, "the_amazing_friend", "feather_titanium", "titanium_case");
        
        // Diamond Feather - Bahamut
        CreateReward(6, "Diamond Feather", "Bahamut",
            "The light of the moon and the song of the wind. Forged in the icy peaks where ancient dragons slumber, the Diamond Feather was bestowed upon bards and storytellers.",
            RewardRarity.Legendary, "the_loyal_friend", "feather_diamond", "diamond_case");
        
        // Mercury Feather - Tarkus
        CreateReward(7, "Mercury Feather", "Tarkus",
            "The shadows of twilight and the secrets of the deep. Forged in the darkest depths where ancient spirits dwell, the Mercury Feather was entrusted to assassins and spies.",
            RewardRarity.Epic, "the_friendship_king", "feather_mercury", "mercury_case");
        
        // Ruby Feather - Ignis
        CreateReward(8, "Ruby Feather", "Ignis",
            "The fire of passion and the fury of battle. Forged in the heart of volcanoes where molten rock flows, the Ruby Feather was given to warriors and berserkers.",
            RewardRarity.Epic, "challenge_lover", "feather_ruby", "ruby_case");
        
        // Emerald Feather - Viridis
        CreateReward(9, "Emerald Feather", "Viridis",
            "The essence of life and the bounty of nature. Forged in the deepest forests where ancient trees stand tall, the Emerald Feather was given to druids and healers.",
            RewardRarity.Legendary, "the_advanced_expert", "feather_emerald", "emerald_case");
        
        // Legendary Feather - Aether
        CreateReward(10, "Legendary Feather", "Aether",
            "The unbound spirit and the ultimate truth. Forged in the celestial realms where gods and mortals meet, the Legendary Feather was given to heroes and champions.",
            RewardRarity.Legendary, "the_legend_of_challenge", "feather_legendary", "legendary_case");
        
        Debug.Log($"RewardManager: Created {allRewards.Count} feather rewards");
    }
    
    private void CreateReward(int id, string name, string region, string description, 
        RewardRarity rarity, string unlockRequirement, string iconName, string caseName)
    {
        Reward reward = new Reward
        {
            id = id,
            name = name,
            region = region,
            description = description,
            rarity = rarity,
            unlockRequirement = unlockRequirement,
            iconName = iconName,
            caseName = caseName,
            isUnlocked = false,
            unlockedAt = DateTime.MinValue,
            isViewed = false,
            count = 0 // Initialize count to 0
        };
        
        allRewards.Add(reward);
    }
    
    #endregion
    
    #region Reward Management
    
    public void CheckAndUnlockRewards(string achievementId)
    {
        List<Reward> rewardsToUnlock = allRewards.Where(r => 
            r.unlockRequirement == achievementId && !r.isUnlocked).ToList();
        
        foreach (var reward in rewardsToUnlock)
        {
            UnlockReward(reward);
        }
    }
    
    private void UnlockReward(Reward reward)
    {
        if (reward.isUnlocked) return;
        
        reward.isUnlocked = true;
        reward.unlockedAt = DateTime.Now;
        
        if (!unlockedRewards.Contains(reward))
        {
            unlockedRewards.Add(reward);
        }
        
        // Update server rewards array
        UpdateServerRewards();
        
        // Show notification
        ShowRewardNotification(reward);
        
        // Save rewards
        if (config.autoSaveProgress)
        {
            SaveRewards();
        }
        
        OnRewardUnlocked?.Invoke(reward);
        Debug.Log($"RewardManager: Reward unlocked - {reward.name}");
    }
    
    public void MarkRewardAsViewed(Reward reward)
    {
        if (reward != null && reward.isUnlocked)
        {
            reward.isViewed = true;
            OnRewardViewed?.Invoke(reward);
            
            if (config.autoSaveProgress)
            {
                SaveRewards();
            }
        }
    }
    
    public void AddRewardCount(int rewardId, int amount = 1)
    {
        Reward reward = GetRewardById(rewardId);
        if (reward != null)
        {
            reward.count += amount;
            
            // Auto-unlock if count > 0
            if (reward.count > 0 && !reward.isUnlocked)
            {
                reward.isUnlocked = true;
                reward.unlockedAt = DateTime.Now;
                
                if (!unlockedRewards.Contains(reward))
                {
                    unlockedRewards.Add(reward);
                }
                
                OnRewardUnlocked?.Invoke(reward);
            }
            
            if (config.autoSaveProgress)
            {
                SaveRewards();
            }
            
            Debug.Log($"RewardManager: Added {amount} to {reward.name}, new count: {reward.count}");
        }
    }
    
    public void SetRewardCount(int rewardId, int count)
    {
        Reward reward = GetRewardById(rewardId);
        if (reward != null)
        {
            reward.count = count;
            
            // Auto-unlock if count > 0
            if (reward.count > 0 && !reward.isUnlocked)
            {
                reward.isUnlocked = true;
                reward.unlockedAt = DateTime.Now;
                
                if (!unlockedRewards.Contains(reward))
                {
                    unlockedRewards.Add(reward);
                }
                
                OnRewardUnlocked?.Invoke(reward);
            }
            
            if (config.autoSaveProgress)
            {
                SaveRewards();
            }
            
            Debug.Log($"RewardManager: Set {reward.name} count to {reward.count}");
        }
    }
    
    public int GetRewardCount(int rewardId)
    {
        Reward reward = GetRewardById(rewardId);
        return reward?.count ?? 0;
    }
    
    #endregion
    
    #region Reward Access
    
    public Reward GetRewardById(int id)
    {
        return allRewards.FirstOrDefault(r => r.id == id);
    }
    
    public List<Reward> GetAllRewards()
    {
        return new List<Reward>(allRewards);
    }
    
    public List<Reward> GetUnlockedRewards()
    {
        return new List<Reward>(unlockedRewards);
    }
    
    public List<Reward> GetRewardsByRarity(RewardRarity rarity)
    {
        return allRewards.Where(r => r.rarity == rarity).ToList();
    }
    
    public List<Reward> GetRewardsByRegion(string region)
    {
        return allRewards.Where(r => r.region == region).ToList();
    }
    
    public List<Reward> GetUnlockedRewardsByRarity(RewardRarity rarity)
    {
        return unlockedRewards.Where(r => r.rarity == rarity).ToList();
    }
    
    public int GetUnlockedRewardCount()
    {
        return unlockedRewards.Count;
    }
    
    public int GetTotalRewardCount()
    {
        return allRewards.Count;
    }
    
    public float GetRewardProgressPercentage()
    {
        if (allRewards.Count == 0) return 0f;
        return (float)unlockedRewards.Count / allRewards.Count * 100f;
    }
    
    public int GetUnlockedRewardsByRarityCount(RewardRarity rarity)
    {
        return unlockedRewards.Count(r => r.rarity == rarity);
    }
    
    #endregion
    
    #region Event Handling
    
    private void SubscribeToEvents()
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
        }
    }
    
    private void OnDestroy()
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
        }
    }
    
    private void OnAchievementUnlocked(Achievement achievement)
    {
        CheckAndUnlockRewards(achievement.id);
    }
    
    #endregion
    
    #region Server Integration
    
    private void UpdateServerRewards()
    {
        if (ProfileDataManager.Instance != null)
        {
            var currentProfile = ProfileDataManager.Instance.GetCurrentProfile();
            if (currentProfile != null)
            {
                int[] rewardIds = unlockedRewards.Select(r => r.id).ToArray();
                ProfileDataManager.Instance.UpdateRewards(rewardIds);
            }
        }
    }
    
    #endregion
    
    #region Notifications
    
    private void ShowRewardNotification(Reward reward)
    {
        if (!config.enableNotifications) return;
        
        Debug.Log($"🎁 Reward Unlocked: {reward.name} - {reward.description}");
        // TODO: Implement UI notification system
    }
    
    #endregion
    
    #region Data Persistence
    
    private void SaveRewards()
    {
        try
        {
            RewardSaveData saveData = new RewardSaveData
            {
                unlockedIds = unlockedRewards.Select(r => r.id).ToList(),
                viewedIds = unlockedRewards.Where(r => r.isViewed).Select(r => r.id).ToList(),
                rewardCounts = allRewards.ToDictionary(r => r.id.ToString(), r => r.count)
            };
            
            string jsonData = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("RewardData", jsonData);
            PlayerPrefs.Save();
            
            Debug.Log("RewardManager: Rewards saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"RewardManager: Failed to save rewards - {e.Message}");
        }
    }
    
    private void LoadRewards()
    {
        try
        {
            string jsonData = PlayerPrefs.GetString("RewardData", "");
            
            if (!string.IsNullOrEmpty(jsonData))
            {
                RewardSaveData saveData = JsonUtility.FromJson<RewardSaveData>(jsonData);
                
                unlockedRewards.Clear();
                foreach (int id in saveData.unlockedIds)
                {
                    Reward reward = GetRewardById(id);
                    if (reward != null)
                    {
                        reward.isUnlocked = true;
                        unlockedRewards.Add(reward);
                    }
                }
                
                // Restore viewed status
                foreach (int id in saveData.viewedIds)
                {
                    Reward reward = GetRewardById(id);
                    if (reward != null)
                    {
                        reward.isViewed = true;
                    }
                }
                
                // Restore count data
                if (saveData.rewardCounts != null)
                {
                    foreach (var kvp in saveData.rewardCounts)
                    {
                        if (int.TryParse(kvp.Key, out int rewardId))
                        {
                            Reward reward = GetRewardById(rewardId);
                            if (reward != null)
                            {
                                reward.count = kvp.Value;
                            }
                        }
                    }
                }
                
                Debug.Log($"RewardManager: Loaded {unlockedRewards.Count} unlocked rewards with counts");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"RewardManager: Failed to load rewards - {e.Message}");
        }
    }
    
    #endregion
    
    #region Debug Methods
    
    [ContextMenu("Reset All Rewards")]
    public void ResetAllRewards()
    {
        foreach (var reward in allRewards)
        {
            reward.isUnlocked = false;
            reward.isViewed = false;
            reward.unlockedAt = DateTime.MinValue;
            reward.count = 0; // Reset count to 0
        }
        
        unlockedRewards.Clear();
        PlayerPrefs.DeleteKey("RewardData");
        Debug.Log("RewardManager: All rewards reset to count 0");
    }
    
    [ContextMenu("Unlock All Rewards")]
    public void UnlockAllRewards()
    {
        foreach (var reward in allRewards)
        {
            if (!reward.isUnlocked)
            {
                UnlockReward(reward);
            }
        }
    }
    
    [ContextMenu("Add Random Counts")]
    public void AddRandomCounts()
    {
        foreach (var reward in allRewards)
        {
            int randomCount = UnityEngine.Random.Range(0, 5);
            SetRewardCount(reward.id, randomCount);
        }
        Debug.Log("RewardManager: Added random counts to all rewards");
    }
    
    #endregion
}

/// <summary>
/// Reward data structure
/// </summary>
[System.Serializable]
public class Reward
{
    public int id;
    public string name;
    public string region;
    public string description;
    public RewardRarity rarity;
    public string unlockRequirement;
    public string iconName;
    public string caseName;
    public bool isUnlocked;
    public bool isViewed;
    public DateTime unlockedAt;
    public int count; // Number of feathers collected
}

/// <summary>
/// Reward rarity levels
/// </summary>
public enum RewardRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

/// <summary>
/// Reward configuration
/// </summary>
[System.Serializable]
public class RewardConfig
{
    public bool enableNotifications;
    public bool autoSaveProgress;
    public bool showRewardDetails;
}

/// <summary>
/// Reward save data
/// </summary>
[System.Serializable]
public class RewardSaveData
{
    public List<int> unlockedIds = new List<int>();
    public List<int> viewedIds = new List<int>();
    public Dictionary<string, int> rewardCounts = new Dictionary<string, int>();
}
