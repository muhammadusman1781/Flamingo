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
    
    [Header("Buttons")]
    public Button showMoreFeathersButton;
    
    [Header("Screens")]
    public GameObject feathersScreen; // Reference to the all feathers screen
    
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
        
        // Load user profile
        LoadUserProfile();
    }
    
    private void LoadUserProfile()
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
        Debug.Log($"User profile received: {response}");
        
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
        
        try
        {
            UserProfileResponse profileResponse = JsonUtility.FromJson<UserProfileResponse>(response);
            
            if (profileResponse != null && profileResponse.data != null)
            {
                currentUserProfile = profileResponse.data;
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
    }
    
    private void UpdateFeatherSlots()
    {
        if (currentUserProfile == null || currentUserProfile.user_feathers == null)
            return;
        
        // Sort feathers by priority
        List<UserFeather> sortedFeathers = FeatherPriority.SortByPriority(currentUserProfile.user_feathers);
        
        // Take top 3 feathers
        List<UserFeather> topFeathers = sortedFeathers.Take(3).ToList();
        
        // Update each slot
        for (int i = 0; i < featherSlots.Length; i++)
        {
            if (i < topFeathers.Count)
            {
                // Show this slot with feather data
                if (featherSlots[i] != null)
                    featherSlots[i].SetActive(true);
                
                // Set feather image (you'll need to load the sprite based on feather_type)
                if (featherImages[i] != null)
                {
                    // Load feather sprite - you can implement a sprite manager or use Resources
                    Sprite featherSprite = GetFeatherSprite(topFeathers[i].feather_type);
                    if (featherSprite != null)
                        featherImages[i].sprite = featherSprite;
                }
                
                // Set feather count
                if (featherCountTexts[i] != null)
                    featherCountTexts[i].text = topFeathers[i].feather.ToString();
            }
            else
            {
                // Hide this slot (no feather available)
                if (featherSlots[i] != null)
                    featherSlots[i].SetActive(false);
            }
        }
        
        // Show/hide "Show More" button based on feather count
        if (showMoreFeathersButton != null)
        {
            showMoreFeathersButton.gameObject.SetActive(currentUserProfile.user_feathers.Count > 0);
        }
    }
    
    [Header("Feather Sprites")]
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
    
    public UserProfileData GetCurrentUserProfile()
    {
        return currentUserProfile;
    }
}
