using UnityEngine;
using System;

public class ProfileDataManager : MonoBehaviour
{
    public static ProfileDataManager Instance { get; private set; }
    
    [Header("Data Configuration")]
    public ProfileDataConfig config;
    
    private ProfileData currentProfile;
    private ServerConstants serverConstants;
    
    public event Action<ProfileData> OnProfileDataChanged;
    public event Action<string> OnDataError;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDataManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadProfileData();
    }
    
    private void InitializeDataManager()
    {
        if (config == null)
        {
            config = CreateDefaultConfig();
        }
        
        if (NetworkingHandler.instance != null)
        {
            serverConstants = NetworkingHandler.instance.serverConstants;
        }
    }
    
    private ProfileDataConfig CreateDefaultConfig()
    {
        return new ProfileDataConfig
        {
            defaultPlayerName = "Player Name",
            defaultPlayerId = "757585856494",
            maxNameLength = 50,
            minAge = 1,
            maxAge = 120
        };
    }
    
    #region Data Access
    
    public ProfileData GetCurrentProfile()
    {
        return currentProfile;
    }
    
    public bool IsProfileComplete()
    {
        return currentProfile != null && 
               !string.IsNullOrEmpty(currentProfile.firstName) &&
               !string.IsNullOrEmpty(currentProfile.lastName) &&
               currentProfile.age > 0 &&
               !string.IsNullOrEmpty(currentProfile.region) &&
               !string.IsNullOrEmpty(currentProfile.gender);
    }
    
    public string GetPlayerDisplayName()
    {
        if (currentProfile == null) return config.defaultPlayerName;
        
        string fullName = $"{currentProfile.firstName} {currentProfile.lastName}".Trim();
        return string.IsNullOrEmpty(fullName) ? config.defaultPlayerName : fullName;
    }
    
    public string GetPlayerId()
    {
        if (currentProfile == null) return config.defaultPlayerId;
        
        if (!string.IsNullOrEmpty(currentProfile.email))
        {
            string id = currentProfile.email.GetHashCode().ToString().Replace("-", "");
            return id.Length > 12 ? id.Substring(0, 12) : id.PadLeft(12, '0');
        }
        
        return config.defaultPlayerId;
    }
    
    #endregion
    
    #region Data Updates
    
    public bool UpdateProfileData(string firstName, string lastName, int age, string region, string gender)
    {
        if (!ValidateProfileData(firstName, lastName, age, region, gender))
        {
            return false;
        }
        
        if (currentProfile == null)
        {
            currentProfile = new ProfileData();
        }
        
        currentProfile.firstName = firstName;
        currentProfile.lastName = lastName;
        currentProfile.age = age;
        currentProfile.region = region;
        currentProfile.gender = gender;
        currentProfile.lastUpdated = DateTime.Now;
        
        SaveProfileData();
        OnProfileDataChanged?.Invoke(currentProfile);
        
        return true;
    }
    
    public void UpdatePoints(int points)
    {
        if (currentProfile == null)
        {
            currentProfile = new ProfileData();
        }
        
        currentProfile.points = points;
        SaveProfileData();
        OnProfileDataChanged?.Invoke(currentProfile);
    }
    
    public void UpdateRewards(int[] rewards)
    {
        if (currentProfile == null)
        {
            currentProfile = new ProfileData();
        }
        
        currentProfile.rewards = rewards;
        SaveProfileData();
        OnProfileDataChanged?.Invoke(currentProfile);
    }
    
    #endregion
    
    #region Data Validation
    
    private bool ValidateProfileData(string firstName, string lastName, int age, string region, string gender)
    {
        if (string.IsNullOrEmpty(firstName) || firstName.Length > config.maxNameLength)
        {
            OnDataError?.Invoke("Invalid first name");
            return false;
        }
        
        if (string.IsNullOrEmpty(lastName) || lastName.Length > config.maxNameLength)
        {
            OnDataError?.Invoke("Invalid last name");
            return false;
        }
        
        if (age < config.minAge || age > config.maxAge)
        {
            OnDataError?.Invoke($"Age must be between {config.minAge} and {config.maxAge}");
            return false;
        }
        
        if (string.IsNullOrEmpty(region))
        {
            OnDataError?.Invoke("Region is required");
            return false;
        }
        
        if (string.IsNullOrEmpty(gender))
        {
            OnDataError?.Invoke("Gender is required");
            return false;
        }
        
        return true;
    }
    
    #endregion
    
    #region Data Persistence
    
    private void SaveProfileData()
    {
        if (currentProfile == null) return;
        
        try
        {
            string jsonData = JsonUtility.ToJson(currentProfile);
            PlayerPrefs.SetString("ProfileData", jsonData);
            PlayerPrefs.Save();
            
            // Sync with server constants if available
            SyncWithServerConstants();
            
            Debug.Log("ProfileDataManager: Profile data saved successfully");
        }
        catch (Exception e)
        {
            OnDataError?.Invoke($"Failed to save profile data: {e.Message}");
        }
    }
    
    private void LoadProfileData()
    {
        try
        {
            string jsonData = PlayerPrefs.GetString("ProfileData", "");
            
            if (!string.IsNullOrEmpty(jsonData))
            {
                currentProfile = JsonUtility.FromJson<ProfileData>(jsonData);
                Debug.Log("ProfileDataManager: Profile data loaded successfully");
            }
            else
            {
                // Try to load from server constants
                LoadFromServerConstants();
            }
        }
        catch (Exception e)
        {
            OnDataError?.Invoke($"Failed to load profile data: {e.Message}");
            currentProfile = new ProfileData();
        }
    }
    
    private void LoadFromServerConstants()
    {
        if (serverConstants?.UserProfileData != null)
        {
            var serverData = serverConstants.UserProfileData;
            currentProfile = new ProfileData
            {
                firstName = serverData.first_name,
                lastName = serverData.last_name,
                age = serverData.age,
                region = serverData.region,
                gender = serverData.gender,
                email = serverData.email,
                points = serverData.points,
                rewards = serverData.rewards,
                lastUpdated = DateTime.Now
            };
            
            Debug.Log("ProfileDataManager: Profile data loaded from server constants");
        }
        else
        {
            currentProfile = new ProfileData();
            Debug.Log("ProfileDataManager: No existing profile data found, using defaults");
        }
    }
    
    private void SyncWithServerConstants()
    {
        if (serverConstants?.UserProfileData != null && currentProfile != null)
        {
            serverConstants.UserProfileData.first_name = currentProfile.firstName;
            serverConstants.UserProfileData.last_name = currentProfile.lastName;
            serverConstants.UserProfileData.age = currentProfile.age;
            serverConstants.UserProfileData.region = currentProfile.region;
            serverConstants.UserProfileData.gender = currentProfile.gender;
            serverConstants.UserProfileData.points = currentProfile.points;
            serverConstants.UserProfileData.rewards = currentProfile.rewards;
        }
    }
    
    #endregion
    
    #region Debug Methods
    
    [ContextMenu("Reset Profile Data")]
    public void ResetProfileData()
    {
        currentProfile = new ProfileData();
        PlayerPrefs.DeleteKey("ProfileData");
        OnProfileDataChanged?.Invoke(currentProfile);
        Debug.Log("ProfileDataManager: Profile data reset");
    }
    
    [ContextMenu("Print Profile Data")]
    public void PrintProfileData()
    {
        if (currentProfile != null)
        {
            Debug.Log($"ProfileDataManager: Name: {GetPlayerDisplayName()}, ID: {GetPlayerId()}, Points: {currentProfile.points}");
        }
        else
        {
            Debug.Log("ProfileDataManager: No profile data available");
        }
    }
    
    #endregion
}

/// <summary>
/// Profile data structure
/// </summary>
[System.Serializable]
public class ProfileData
{
    public string firstName;
    public string lastName;
    public int age;
    public string region;
    public string gender;
    public string email;
    public int points;
    public int[] rewards;
    public DateTime lastUpdated;
}

/// <summary>
/// Configuration for profile data validation
/// </summary>
[System.Serializable]
public class ProfileDataConfig
{
    public string defaultPlayerName;
    public string defaultPlayerId;
    public int maxNameLength;
    public int minAge;
    public int maxAge;
}
