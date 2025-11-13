using UnityEngine;
// AwaisDev: Chooses initial screen based on auth token

public class AppEntry : MonoBehaviour
{
    [Tooltip("If true, assumes logged-in when token exists in ServerConstants")] public bool useServerConstantsAuth = true;
    [Tooltip("Fallback initial screen if not logged in")] public ScreenId notLoggedInScreen = ScreenId.Login;
    [Tooltip("Initial screen if logged in")] public ScreenId loggedInScreen = ScreenId.Home;

    void Start()
    {
        var screenManager = ScreenManager.Instance;
        if (screenManager == null)
        {
            Debug.LogWarning("AppEntry: ScreenManager not found in scene.");
            return;
        }

        bool isLoggedIn = false;
        if (useServerConstantsAuth && NetworkingHandler.instance != null && NetworkingHandler.instance.serverConstants != null)
        {
            var token = NetworkingHandler.instance.serverConstants.UserProfileData.token;
            isLoggedIn = !string.IsNullOrEmpty(token);
        }

        if (isLoggedIn)
        {
            // Check if the user's profile is complete (e.g., if first_name exists)
            var firstName = NetworkingHandler.instance.serverConstants.UserProfileData.first_name;
            bool isProfileComplete = !string.IsNullOrEmpty(firstName);

            if (isProfileComplete)
            {
                // If profile is complete, go to the main logged-in screen
                screenManager.SetAsRoot(loggedInScreen);
            }
            else
            {
                // If profile is not complete, force user to the StudentInfo/Profile screen
                screenManager.SetAsRoot(ScreenId.StudentInfo);
            }
        }
        else
        {
            // If not logged in, go to the login screen
            screenManager.SetAsRoot(notLoggedInScreen);
        }
    }
}


