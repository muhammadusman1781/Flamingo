using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTLTMPro;
using UnityEngine.UI;
public class HomeScreen : MonoBehaviour
{
    public RTLTextMeshPro PlayerName;
    public GameObject MaleIcon;
    public GameObject FemaleIcon;
    public Image XpBar;
    public GameObject BottomBar;
    
    [Header("Loading")]
    public GameObject loadingPanel;

    private void OnEnable()
    {
        // Load user profile first
        LoadUserProfile();
    }
    
    private void LoadUserProfile()
    {
        if (NetworkingHandler.instance == null || NetworkingHandler.instance.serverConstants == null)
        {
            Debug.LogError("NetworkingHandler or ServerConstants is null!");
            return;
        }
        
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        
        string apiUrl = NetworkingHandler.instance.serverConstants.baseUrl + "/auth/user/";
        
        NetworkingHandler.instance.getMessage(
            apiUrl,
            isTokenNeeded: true,
            onSuccess: OnUserProfileSuccess,
            onFail: OnUserProfileFail
        );
    }
    
    private void OnUserProfileSuccess(string response)
    {
        Debug.Log($"HomeScreen - User profile received: {response}");
        
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
        
        try
        {
            UserProfileResponse profileResponse = JsonUtility.FromJson<UserProfileResponse>(response);
            
            if (profileResponse != null && profileResponse.data != null)
            {
                // Store the full profile data in ServerConstants for other screens to use
                NetworkingHandler.instance.serverConstants.FullUserProfile = profileResponse.data;
                
                // Update UI
                UpdateUI(profileResponse.data);
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
    
    private void UpdateUI(UserProfileData profile)
    {
        if (profile == null)
            return;
        
        // Update player name
        PlayerName.text = profile.first_name + " " + profile.last_name;
        
        // Update gender icons
        if (profile.gender == "Male")
        {
            MaleIcon.SetActive(true);
            FemaleIcon.SetActive(false);
        }
        else
        {
            MaleIcon.SetActive(false);
            FemaleIcon.SetActive(true);
        }
        
        BottomBar.SetActive(true);
    }
}
