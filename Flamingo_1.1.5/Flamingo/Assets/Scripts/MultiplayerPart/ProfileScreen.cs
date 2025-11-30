using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RTLTMPro;

public class ProfileScreen : MonoBehaviour
{
    [Header("UI References")]
    public RTLTextMeshPro playerNameText;
    public RTLTextMeshPro friendsCountText;
    public RTLTextMeshPro winsCountText;
    public RTLTextMeshPro lossesCountText;
    
    [Header("Feather Slots")]
    public GameObject[] featherSlots = new GameObject[3]; // 3 slots for top feathers
    public Image[] featherImages = new Image[3]; // Images for the 3 feather slots
    public RTLTextMeshPro[] featherCountTexts = new RTLTextMeshPro[3]; // Count text for each feather
    
    [Header("Achievement Slots (The Sharp, The Clever, The Smart)")]
    public Image[] achievementImages = new Image[3]; // Images for the 3 achievements
    
    [Header("Achievement Sprites")]
    [Tooltip("Completed sprite for 'The Sharp' achievement")]
    public Sprite theSharpCompletedSprite;
    [Tooltip("Not completed sprite for 'The Sharp' achievement")]
    public Sprite theSharpNotCompletedSprite;
    
    [Tooltip("Completed sprite for 'The Clever' achievement")]
    public Sprite theCleverCompletedSprite;
    [Tooltip("Not completed sprite for 'The Clever' achievement")]
    public Sprite theCleverNotCompletedSprite;
    
    [Tooltip("Completed sprite for 'The Smart' achievement")]
    public Sprite theSmartCompletedSprite;
    [Tooltip("Not completed sprite for 'The Smart' achievement")]
    public Sprite theSmartNotCompletedSprite;
    
    [Header("Buttons")]
    public Button showMoreFeathersButton;
    public Button showAchievementsButton;
    
    [Header("Screens")]
    public GameObject feathersScreen; // Reference to the all feathers screen
    public GameObject achievementsScreen; // Reference to the achievements screen
    
    [Header("Loading")]
    public GameObject loadingPanel;
    
    private ServerConstants serverConstants;
    private UserProfileData currentUserProfile;
    
    private void Start()
    {
        // Get server constants reference
        if (NetworkingHandler.instance != null)
        {
            serverConstants = NetworkingHandler.instance.serverConstants;
        }
        else
        {
            Debug.LogError("NetworkingHandler instance not found!");
        }
        
        // Setup button listeners
        if (showMoreFeathersButton != null)
        {
            showMoreFeathersButton.onClick.AddListener(OnShowMoreFeathersClicked);
        }
        
        if (showAchievementsButton != null)
        {
            showAchievementsButton.onClick.AddListener(OnShowAchievementsClicked);
        }
    }
    
    private void OnEnable()
    {
        // Load user profile from cached data in ServerConstants
        LoadUserProfileFromCache();
    }
    
    private void LoadUserProfileFromCache()
    {
        if (NetworkingHandler.instance == null || NetworkingHandler.instance.serverConstants == null)
        {
            Debug.LogError("NetworkingHandler or ServerConstants is null!");
            return;
        }
        
        // Get the cached profile data from ServerConstants
        currentUserProfile = NetworkingHandler.instance.serverConstants.FullUserProfile;
        
        if (currentUserProfile != null)
        {
            // Profile data is already loaded, just update UI
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("User profile not yet loaded. Waiting for HomeScreen to load it.");
            // Optionally, you could load it here as a fallback
            // LoadUserProfileFromAPI();
        }
    }
    
    // Keep this method as a fallback if needed
    private void LoadUserProfileFromAPI()
    {
        if (serverConstants == null)
        {
            Debug.LogError("ServerConstants is null!");
            return;
        }
        
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        
        string apiUrl = serverConstants.baseUrl + "/auth/user/";
        
        NetworkingHandler.instance.getMessage(
            apiUrl,
            isTokenNeeded: true,
            onSuccess: OnUserProfileSuccess,
            onFail: OnUserProfileFail
        );
    }
    
    private void OnUserProfileSuccess(string response)
    {
        Debug.Log($"ProfileScreen - User profile received: {response}");
        
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
        
        try
        {
            UserProfileResponse profileResponse = JsonUtility.FromJson<UserProfileResponse>(response);
            
            if (profileResponse != null && profileResponse.data != null)
            {
                currentUserProfile = profileResponse.data;
                // Also update the cache
                NetworkingHandler.instance.serverConstants.FullUserProfile = profileResponse.data;
                UpdateUI();
            }
            else
            {
                Debug.LogError("Failed to parse user profile response");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing user profile: {ex.Message}");
        }
    }
    
    private void OnUserProfileFail(string error)
    {
        Debug.LogError($"Failed to load user profile: {error}");
        
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
    
    private void UpdateUI()
    {
        if (currentUserProfile == null)
            return;
        
        // Update player name (first_name + last_name)
        string playerName = $"{currentUserProfile.first_name} {currentUserProfile.last_name}";
        if (playerNameText != null)
            playerNameText.text = playerName;
        
        // Update friends count
        if (friendsCountText != null)
            friendsCountText.text = currentUserProfile.friends_count.ToString();
        
        // Update wins count
        if (winsCountText != null)
            winsCountText.text = currentUserProfile.win.ToString();
        
        // Update losses count
        if (lossesCountText != null)
            lossesCountText.text = currentUserProfile.lose.ToString();
        
        // Update feather slots
        UpdateFeatherSlots();
        
        // Update achievement slots
        UpdateAchievementSlots();
    }
    
    private void UpdateFeatherSlots()
    {
        if (currentUserProfile == null)
            return;
        
        // Sort feathers by priority (handle null case)
        List<UserFeather> sortedFeathers = new List<UserFeather>();
        if (currentUserProfile.user_feathers != null && currentUserProfile.user_feathers.Count > 0)
        {
            sortedFeathers = FeatherPriority.SortByPriority(currentUserProfile.user_feathers);
        }
        
        // Take top 3 feathers
        List<UserFeather> topFeathers = sortedFeathers.Take(3).ToList();
        
        // Update each slot - ALWAYS show all 3 slots
        for (int i = 0; i < featherSlots.Length; i++)
        {
            // Always activate the slot
            if (featherSlots[i] != null)
                featherSlots[i].SetActive(true);
            
            if (i < topFeathers.Count)
            {
                // User has this feather - show with full opacity
                if (featherImages[i] != null)
                {
                    // Load feather sprite
                    Sprite featherSprite = GetFeatherSprite(topFeathers[i].feather_type);
                    if (featherSprite != null)
                        featherImages[i].sprite = featherSprite;
                    
                    // Set full opacity
                    Color imageColor = featherImages[i].color;
                    imageColor.a = 1f;
                    featherImages[i].color = imageColor;
                }
                
                // Set feather count
                if (featherCountTexts[i] != null)
                {
                    featherCountTexts[i].text = topFeathers[i].feather.ToString();
                    featherCountTexts[i].gameObject.SetActive(true);
                }
            }
            else
            {
                // User doesn't have this feather - show placeholder with reduced opacity
                if (featherImages[i] != null)
                {
                    // Set placeholder sprite if available, otherwise keep current sprite
                    if (placeholderFeatherSprite != null)
                    {
                        featherImages[i].sprite = placeholderFeatherSprite;
                    }
                    
                    // Set reduced opacity (30% opacity for empty slots)
                    Color imageColor = featherImages[i].color;
                    imageColor.a = 0.3f;
                    featherImages[i].color = imageColor;
                }
                
                // Hide or clear the feather count text
                if (featherCountTexts[i] != null)
                {
                    featherCountTexts[i].text = "";
                    featherCountTexts[i].gameObject.SetActive(false);
                }
            }
        }
        
        // Show/hide "Show More" button based on feather count
        if (showMoreFeathersButton != null)
        {
            bool hasFeathers = currentUserProfile.user_feathers != null && currentUserProfile.user_feathers.Count > 0;
            showMoreFeathersButton.gameObject.SetActive(hasFeathers);
        }
    }
    
    private void UpdateAchievementSlots()
    {
        if (currentUserProfile == null)
            return;
        
        // Get user's completed achievements list
        List<string> completedAchievements = currentUserProfile.achievements;
        
        // If achievements list is null, initialize as empty
        if (completedAchievements == null)
        {
            completedAchievements = new List<string>();
        }
        
        // Achievement 0: "The Sharp"
        if (achievementImages.Length > 0 && achievementImages[0] != null)
        {
            bool isTheSharpCompleted = completedAchievements.Contains("The Sharp");
            
            if (isTheSharpCompleted && theSharpCompletedSprite != null)
            {
                achievementImages[0].sprite = theSharpCompletedSprite;
                Debug.Log("The Sharp: COMPLETED");
            }
            else if (!isTheSharpCompleted && theSharpNotCompletedSprite != null)
            {
                achievementImages[0].sprite = theSharpNotCompletedSprite;
                Debug.Log("The Sharp: NOT COMPLETED");
            }
        }
        
        // Achievement 1: "The Clever"
        if (achievementImages.Length > 1 && achievementImages[1] != null)
        {
            bool isTheCleverCompleted = completedAchievements.Contains("The Clever");
            
            if (isTheCleverCompleted && theCleverCompletedSprite != null)
            {
                achievementImages[1].sprite = theCleverCompletedSprite;
                Debug.Log("The Clever: COMPLETED");
            }
            else if (!isTheCleverCompleted && theCleverNotCompletedSprite != null)
            {
                achievementImages[1].sprite = theCleverNotCompletedSprite;
                Debug.Log("The Clever: NOT COMPLETED");
            }
        }
        
        // Achievement 2: "The Smart"
        if (achievementImages.Length > 2 && achievementImages[2] != null)
        {
            bool isTheSmartCompleted = completedAchievements.Contains("The Smart");
            
            if (isTheSmartCompleted && theSmartCompletedSprite != null)
            {
                achievementImages[2].sprite = theSmartCompletedSprite;
                Debug.Log("The Smart: COMPLETED");
            }
            else if (!isTheSmartCompleted && theSmartNotCompletedSprite != null)
            {
                achievementImages[2].sprite = theSmartNotCompletedSprite;
                Debug.Log("The Smart: NOT COMPLETED");
            }
        }
        
        Debug.Log($"Achievement slots updated. Completed achievements: {string.Join(", ", completedAchievements)}");
    }
    
    [Header("Feather Sprites")]
    [SerializeField] private Sprite placeholderFeatherSprite; // Placeholder for empty slots
    [SerializeField] private Sprite legendaryFeatherSprite;
    [SerializeField] private Sprite emeraldFeatherSprite;
    [SerializeField] private Sprite rubyFeatherSprite;
    [SerializeField] private Sprite mercuryFeatherSprite;
    [SerializeField] private Sprite diamondFeatherSprite;
    [SerializeField] private Sprite titaniumFeatherSprite;
    [SerializeField] private Sprite platinumFeatherSprite;
    [SerializeField] private Sprite goldenFeatherSprite;
    [SerializeField] private Sprite silverFeatherSprite;
    [SerializeField] private Sprite bronzeFeatherSprite;

    private Sprite GetFeatherSprite(string featherType)
    {
        switch (featherType)
        {
            case "Legendary":
                return legendaryFeatherSprite;
            case "Emerald":
                return emeraldFeatherSprite;
            case "Ruby":
                return rubyFeatherSprite;
            case "Mercury":
                return mercuryFeatherSprite;
            case "Diamond":
                return diamondFeatherSprite;
            case "Titanium":
                return titaniumFeatherSprite;
            case "Platinum":
                return platinumFeatherSprite;
            case "Golden":
                return goldenFeatherSprite;
            case "Silver":
                return silverFeatherSprite;
            case "Bronze":
                return bronzeFeatherSprite;
            default:
                return null;
        }
    }
    
    private void OnShowMoreFeathersClicked()
    {
        if (feathersScreen != null && currentUserProfile != null)
        {
            // Pass user profile data to feathers screen
            FeathersScreen feathersScreenScript = feathersScreen.GetComponent<FeathersScreen>();
            if (feathersScreenScript != null)
            {
                feathersScreenScript.SetUserProfile(currentUserProfile);
            }
            
            // Show feathers screen
            feathersScreen.SetActive(true);
            
            // Optionally hide profile screen
            // gameObject.SetActive(false);
        }
    }
    
    private void OnShowAchievementsClicked()
    {
        if (achievementsScreen != null)
        {
            Debug.Log("Opening Achievements Screen");
            
            // Show achievements screen
            achievementsScreen.SetActive(true);
            
            // Hide profile screen
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Achievements Screen reference not set!");
        }
    }
    
    public UserProfileData GetCurrentUserProfile()
    {
        return currentUserProfile;
    }
}
