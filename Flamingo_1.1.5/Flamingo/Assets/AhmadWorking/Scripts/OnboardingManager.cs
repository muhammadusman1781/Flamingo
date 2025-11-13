using UnityEngine; // AwaisDev: Onboarding + login flow wired to ScreenManager
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class OnboardingManager : MonoBehaviour
{
    // Onboarding screens
    [Header("OnBoarding Screens")]
    [Space]
    public GameObject[] onboardingScreens;
    private int currentOnboardingScreen = 0;

    // Login UI
    [Header("Login Panel Objects")]
    [Space]
    public LoginPanelUI LoginPanelUI;

    // Signup UI
    [Header("Register Panel Objects")]
    [Space]
    public SignupPanelUI signupPanelUI;

    // OTP Verification UI
    [Header("Otp Verification Panel Objects")]
    [Space]
    public OTPVerifyPageUI otpVerifyPageUI;
    public OTPInputHandler otpInputHandler;

    // Profile Form UI
    [Header("profile Edit Panel Objects")]
    [Space]
    public ProfileFormUI profileFormUI;

    // Notification Manager
    [Header("Notification Settings")]
    public NotificationManager note;

    // Authentication Manager
    [Header("Authentication")]
    public AuthenticationManager AuthenticationManager;

    // Navigation
    [Space]
    public Button nextButton;

    // Regular expressions for validation
    private const string EmailRegexPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    private const string PhoneRegexPattern = @"^\+?[1-9]\d{1,14}$";
    private const string PasswordRegexPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@$!%*?&]{8,}$";

    // Dev settings
    [Header("Dev Settings")]
    public bool autoLoginOnStart = false;
    public bool devBypassAuthentication = false;
    public bool devBypassSignup = false;
    public bool clearTokenOnStart = true; // Clear token on start for testing
    public bool autoFillSignupData = false; // Auto-fill signup form for testing
    public string devEmail = "xynova.net@gmail.com";
    public string devPassword = "787890";
    public string devBypassToken = "DEV_TOKEN_12345";
    public string devBypassFirstName = "Dev User";

    void Start()
    {
        StartCoroutine(InitializeAfterFrame());
    }
    
    // Check if user is already logged in
    private bool IsUserLoggedIn()
    {
        if (NetworkingHandler.instance != null && NetworkingHandler.instance.serverConstants != null)
        {
            var token = NetworkingHandler.instance.serverConstants.UserProfileData.token;
            return !string.IsNullOrEmpty(token);
        }
        return false;
    }
    
    // Logout user and clear session data
    public void LogoutUser()
    {
        if (NetworkingHandler.instance != null && NetworkingHandler.instance.serverConstants != null)
        {
            // Clear user data
            NetworkingHandler.instance.serverConstants.UserProfileData.token = "";
            NetworkingHandler.instance.serverConstants.UserProfileData.email = "";
            NetworkingHandler.instance.serverConstants.UserProfileData.first_name = "";
            NetworkingHandler.instance.serverConstants.UserProfileData.points = 0;
            NetworkingHandler.instance.serverConstants.UserProfileData.rewards = new int[0];
            
            Debug.Log("User logged out successfully");
            
            // Navigate back to login screen
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.SetAsRoot(ScreenId.Login);
            }
            else
            {
                ShowLoginPanel();
            }
        }
    }
    
    // Clear user token for testing purposes
    private void ClearUserToken()
    {
        if (NetworkingHandler.instance != null && NetworkingHandler.instance.serverConstants != null)
        {
            NetworkingHandler.instance.serverConstants.UserProfileData.token = "";
            Debug.Log("Token cleared for testing - user will see login screen");
        }
    }

    IEnumerator InitializeAfterFrame()
    {
        yield return null;

        // Clear token on start for testing if enabled
        if (clearTokenOnStart)
        {
            ClearUserToken();
        }

        InitializeUI();
        SetupButtonListeners();

        // Disabled auto-login to prevent duplicate API calls
        // if (autoLoginOnStart && LoginPanelUI != null)
        // {
        //     if (LoginPanelUI.loginPanel != null) LoginPanelUI.loginPanel.SetActive(true);
        //     if (!string.IsNullOrEmpty(devEmail)) LoginPanelUI.loginEmailPhoneInput.text = devEmail;
        //     if (!string.IsNullOrEmpty(devPassword)) LoginPanelUI.loginPasswordInput.text = devPassword;
        //     OnLoginClicked();
        // }
    }

    void InitializeUI()
    {
        // Show first onboarding screen
        if (onboardingScreens.Length > 0)
        {
            onboardingScreens[0].SetActive(true);
        }

        // Hide other panels and clear error messages
        if (LoginPanelUI != null && LoginPanelUI.loginPanel != null)
            LoginPanelUI.loginPanel.SetActive(false);
        if (signupPanelUI != null && signupPanelUI.signupPanel != null)
            signupPanelUI.signupPanel.SetActive(false);
        if (otpVerifyPageUI != null && otpVerifyPageUI.otpPanel != null)
            otpVerifyPageUI.otpPanel.SetActive(false);
        if (profileFormUI != null && profileFormUI.profileFormPanel != null)
            profileFormUI.profileFormPanel.SetActive(false);
        ClearErrorMessages();
    }

    void SetupButtonListeners()
    {
        if (nextButton != null) nextButton.onClick.AddListener(NextOnboardingScreen);

        if (LoginPanelUI != null)
        {
            if (LoginPanelUI.loginButton != null) LoginPanelUI.loginButton.onClick.AddListener(OnLoginClicked);
            if (LoginPanelUI.signupButton != null) LoginPanelUI.signupButton.onClick.AddListener(OnSignupClicked);
        }

        if (signupPanelUI != null)
        {
            if (signupPanelUI.signupSubmitButton != null) signupPanelUI.signupSubmitButton.onClick.AddListener(OnSignupSubmitClicked);
            if (signupPanelUI.backToLoginButton != null) signupPanelUI.backToLoginButton.onClick.AddListener(ShowLoginPanel);
        }

        if (otpVerifyPageUI != null)
        {
            if (otpVerifyPageUI.otpSubmitButton != null) otpVerifyPageUI.otpSubmitButton.onClick.AddListener(OnOTPSubmitClicked);
            if (otpVerifyPageUI.resendOtpButton != null) otpVerifyPageUI.resendOtpButton.onClick.AddListener(OnSendOtpClicked);
        }

        if (profileFormUI != null)
        {
            if (profileFormUI.profileSubmitButton != null) profileFormUI.profileSubmitButton.onClick.AddListener(OnProfileSubmitClicked);
        }
    }

    void ClearErrorMessages()
    {
        // Clear all error texts
        if (LoginPanelUI != null && LoginPanelUI.loginErrorText != null)
            LoginPanelUI.loginErrorText.text = "";
        if (signupPanelUI != null && signupPanelUI.signupErrorText != null)
            signupPanelUI.signupErrorText.text = "";
        if (otpVerifyPageUI != null && otpVerifyPageUI.otpErrorText != null)
            otpVerifyPageUI.otpErrorText.text = "";
        if (profileFormUI != null && profileFormUI.profileErrorText != null)
            profileFormUI.profileErrorText.text = "";
    }

    public void NextOnboardingScreen()
    {
        if (currentOnboardingScreen < onboardingScreens.Length)
            onboardingScreens[currentOnboardingScreen].SetActive(false);

        currentOnboardingScreen++;

        if (currentOnboardingScreen < onboardingScreens.Length)
        {
            onboardingScreens[currentOnboardingScreen].SetActive(true);
        }
        else
        {
            ShowLoginPanel();
        }
    }

    public void ShowLoginPanel()
    {
        // Check if user is already logged in
        if (IsUserLoggedIn())
        {
            Debug.Log("User is already logged in, redirecting to home screen");
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.SetAsRoot(ScreenId.Home);
            }
            else
            {
                SceneManager.LoadScene("GamePlay");
            }
            return;
        }

        foreach (var screen in onboardingScreens)
        {
            screen.SetActive(false);
        }
        if (signupPanelUI != null && signupPanelUI.signupPanel != null)
            signupPanelUI.signupPanel.SetActive(false);
        if (otpVerifyPageUI != null && otpVerifyPageUI.otpPanel != null)
            otpVerifyPageUI.otpPanel.SetActive(false);
        if (profileFormUI != null && profileFormUI.profileFormPanel != null)
            profileFormUI.profileFormPanel.SetActive(false);

        if (LoginPanelUI != null && LoginPanelUI.loginPanel != null)
        {
            LoginPanelUI.loginPanel.SetActive(true);
            LoginPanelUI.loginEmailPhoneInput.text = "";
            LoginPanelUI.loginPasswordInput.text = "";
            
            ConfigurePasswordField(LoginPanelUI.loginPasswordInput);
            
            if (LoginPanelUI.loginErrorText != null)
                LoginPanelUI.loginErrorText.text = "";
        }
    }

    void OnLoginClicked()
    {
        Debug.Log("OnLoginClicked called");
        Debug.Log($"LoginPanelUI is null: {LoginPanelUI == null}");
        Debug.Log($"loginEmailPhoneInput is null: {LoginPanelUI?.loginEmailPhoneInput == null}");
        
        string emailPhone = LoginPanelUI?.loginEmailPhoneInput?.text?.Trim() ?? "";
        string password = LoginPanelUI?.loginPasswordInput?.text ?? "";
        
        Debug.Log($"Email input text: '{emailPhone}'");
        Debug.Log($"Password input text: '{password}'");

        if (string.IsNullOrEmpty(emailPhone))
        {
            ShowErrorNotification("Login Error", "Please enter email or phone number");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowErrorNotification("Login Error", "Please enter password");
            return;
        }

        // Disable button to prevent double-clicks
        if (LoginPanelUI.loginButton != null)
            LoginPanelUI.loginButton.interactable = false;

        // Dev bypass (no server)
        if (devBypassAuthentication)
        {
            Debug.Log("Using dev bypass authentication - skipping server");
            var scBypass = NetworkingHandler.instance.serverConstants;
            scBypass.UserProfileData.token = string.IsNullOrEmpty(devBypassToken) ? "DEV_TOKEN" : devBypassToken;
            scBypass.UserProfileData.email = emailPhone;
            scBypass.UserProfileData.first_name = string.IsNullOrEmpty(devBypassFirstName) ? "Player" : devBypassFirstName;
            scBypass.UserProfileData.points = 0;
            scBypass.UserProfileData.rewards = new int[0];

            // Re-enable button
            if (LoginPanelUI.loginButton != null)
                LoginPanelUI.loginButton.interactable = true;

            // Hide login panel for dev bypass
            HideLoginPanel();

            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.SetAsRoot(ScreenId.Home);
            }
            else
            {
                SceneManager.LoadScene("GamePlay");
            }
            return;
        }

        // Use AuthenticationManager for login
        if (AuthenticationManager != null)
        {
            AuthenticationManager.login();
        }
        else
        {
            ShowErrorNotification("Login Error", "Authentication system not available");
            if (LoginPanelUI.loginButton != null)
                LoginPanelUI.loginButton.interactable = true;
        }
    }

    void OnSignupClicked()
    {
        if (LoginPanelUI != null && LoginPanelUI.loginPanel != null)
            LoginPanelUI.loginPanel.SetActive(false);

        if (signupPanelUI != null && signupPanelUI.signupPanel != null)
        {
            signupPanelUI.signupPanel.SetActive(true);
            signupPanelUI.signupEmailPhoneInput.text = "";
            signupPanelUI.signupPasswordInput.text = "";
            signupPanelUI.signupConfirmPasswordInput.text = "";
            
            // Configure password fields to hide text with asterisks
            ConfigurePasswordField(signupPanelUI.signupPasswordInput);
            ConfigurePasswordField(signupPanelUI.signupConfirmPasswordInput);
            
            if (signupPanelUI.signupErrorText != null)
                signupPanelUI.signupErrorText.text = "";
            
            // Auto-fill data for testing if enabled
            if (autoFillSignupData)
            {
                signupPanelUI.signupEmailPhoneInput.text = devEmail;
                signupPanelUI.signupPasswordInput.text = devPassword;
                signupPanelUI.signupConfirmPasswordInput.text = devPassword;
                Debug.Log("Auto-filled signup form with test data");
            }
        }
    }

    void OnSignupSubmitClicked()
    {
        Debug.Log("OnSignupSubmitClicked called");
        Debug.Log($"signupPanelUI is null: {signupPanelUI == null}");
        Debug.Log($"signupEmailPhoneInput is null: {signupPanelUI?.signupEmailPhoneInput == null}");
        
        // Additional debugging for UI state
        if (signupPanelUI != null)
        {
            Debug.Log($"signupPanel active: {signupPanelUI.signupPanel?.activeInHierarchy}");
            Debug.Log($"signupEmailPhoneInput active: {signupPanelUI.signupEmailPhoneInput?.gameObject.activeInHierarchy}");
            Debug.Log($"signupEmailPhoneInput interactable: {signupPanelUI.signupEmailPhoneInput?.interactable}");
        }
        
        string emailPhone = signupPanelUI?.signupEmailPhoneInput?.text?.Trim() ?? "";
        string password = signupPanelUI?.signupPasswordInput?.text ?? "";
        string confirmPassword = signupPanelUI?.signupConfirmPasswordInput?.text ?? "";
        
        Debug.Log($"Signup email input text: '{emailPhone}'");
        Debug.Log($"Signup password input text: '{password}'");
        Debug.Log($"Signup confirm password input text: '{confirmPassword}'");

        if (string.IsNullOrEmpty(emailPhone))
        {
            ShowErrorNotification("Signup Error", "Please enter email or phone number");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowErrorNotification("Signup Error", "Please enter password");
            return;
        }

        if (password != confirmPassword)
        {
            ShowErrorNotification("Signup Error", "Passwords do not match");
            return;
        }

        if (!IsValidEmail(emailPhone) && !IsValidPhone(emailPhone))
        {
            ShowErrorNotification("Signup Error", "Please enter a valid email address or phone number");
            return;
        }

        if (!IsValidPassword(password))
        {
            ShowErrorNotification("Signup Error", "Password must be at least 8 characters long and contain uppercase, lowercase, and numbers");
            return;
        }

        // Dev bypass for signup testing
        if (devBypassSignup)
        {
            Debug.Log("Using dev bypass signup - skipping server");
            var scBypass = NetworkingHandler.instance.serverConstants;
            scBypass.UserProfileData.email = emailPhone;
            scBypass.UserProfileData.first_name = "Test User";
            scBypass.UserProfileData.points = 0;
            scBypass.UserProfileData.rewards = new int[0];
            
            // Re-enable button
            if (signupPanelUI.signupSubmitButton != null)
                signupPanelUI.signupSubmitButton.interactable = true;

            // Navigate directly to home for testing
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.SetAsRoot(ScreenId.Home);
            }
            else
            {
                SceneManager.LoadScene("GamePlay");
            }
            return;
        }

        // Call AuthenticationManager to register the user
        if (AuthenticationManager != null)
        {
            // Show loading state
            if (signupPanelUI.signupSubmitButton != null)
                signupPanelUI.signupSubmitButton.interactable = false;

            AuthenticationManager.register(emailPhone, password);
        }
        else
        {
            ShowErrorNotification("Signup Error", "Authentication system not available");
            // Re-enable button if error
            if (signupPanelUI.signupSubmitButton != null)
                signupPanelUI.signupSubmitButton.interactable = true;
        }
    }

    public void ShowOTPPanel()
    {
        if (LoginPanelUI != null && LoginPanelUI.loginPanel != null)
            LoginPanelUI.loginPanel.SetActive(false);
        if (signupPanelUI != null && signupPanelUI.signupPanel != null)
            signupPanelUI.signupPanel.SetActive(false);

        if (otpVerifyPageUI != null && otpVerifyPageUI.otpPanel != null)
        {
            otpVerifyPageUI.otpPanel.SetActive(true);

            // --- MODIFIED SECTION START ---

            // Clear previous input values using OTPInputHandler
            if (otpInputHandler != null)
            {
                otpInputHandler.ClearAllInputs();
            }

            // Use our new handler to focus the first field
            if (otpInputHandler != null)
            {
                otpInputHandler.ActivateFirstInput();
            }
            else
            {
                Debug.LogWarning("OTPInputHandler is not assigned in the OnboardingManager inspector!");
            }


            if (otpVerifyPageUI.otpErrorText != null)
                otpVerifyPageUI.otpErrorText.text = "";
        }
    }

    void OnSendOtpClicked()
    {
        if (AuthenticationManager != null)
        {
            AuthenticationManager.resendOTPFunc();
        }
    }

    void OnOTPSubmitClicked()
    {
        if (otpInputHandler == null)
        {
            ShowErrorNotification("OTP Error", "OTP Input Handler not found. Please assign it in the Inspector.");
            return;
        }

        string otpCode = otpInputHandler.GetOTPCode();

        if (string.IsNullOrEmpty(otpCode) || otpCode.Length < 6)
        {
            ShowErrorNotification("OTP Error", "Please enter the complete OTP code");
            return;
        }
        HideOTPPanel();
        if (note != null)
        {
            note.createNotification("Verifying", "Please wait...", 0, new List<string>());
        }

        if (AuthenticationManager != null)
        {
            AuthenticationManager.verifyOTPFunc(otpCode);
        }
        else
        {
            if (note != null) note.removeNotification();
            ShowErrorNotification("OTP Error", "Authentication system not available");
        }
    }

    public void ShowProfileForm()
    {
        // Hide OTP panel when showing profile form
        if (otpVerifyPageUI != null && otpVerifyPageUI.otpPanel != null)
        {
            otpVerifyPageUI.otpPanel.SetActive(false);
            Debug.Log("OnboardingManager: OTP panel disabled, showing profile form");
        }

        if (profileFormUI != null && profileFormUI.profileFormPanel != null)
        {
            profileFormUI.profileFormPanel.SetActive(true);

            profileFormUI.firstNameInput.text = "";
            profileFormUI.lastNameInput.text = "";
            profileFormUI.phoneNumberInput.text = "";
            profileFormUI.regionInput.text = "";
            profileFormUI.ageInput.text = "";

            profileFormUI.genderDropdown.ClearOptions();
            profileFormUI.genderDropdown.AddOptions(new System.Collections.Generic.List<string> {
                "Male", "Female", "Other", "Prefer not to say"
            });

            if (profileFormUI.profileErrorText != null)
                profileFormUI.profileErrorText.text = "";
        }
    }
    
    public void HideOTPPanel()
    {
        if (otpVerifyPageUI != null && otpVerifyPageUI.otpPanel != null)
        {
            otpVerifyPageUI.otpPanel.SetActive(false);
            Debug.Log("OnboardingManager: OTP panel disabled");
        }
    }
    
    public void HideLoginPanel()
    {
        if (LoginPanelUI != null && LoginPanelUI.loginPanel != null)
        {
            LoginPanelUI.loginPanel.SetActive(false);
            Debug.Log("OnboardingManager: Login panel disabled - user logged in");
        }
    }

    void OnProfileSubmitClicked()
    {
        if (profileFormUI == null)
        {
            ShowErrorNotification("Profile Error", "Profile form not found");
            return;
        }

        // Get profile data from form
        string firstName = profileFormUI.firstNameInput?.text?.Trim() ?? "";
        string lastName = profileFormUI.lastNameInput?.text?.Trim() ?? "";
        string region = profileFormUI.regionInput?.text?.Trim() ?? "";
        string ageText = profileFormUI.ageInput?.text?.Trim() ?? "";
        string gender = profileFormUI.genderDropdown?.options[profileFormUI.genderDropdown.value]?.text ?? "";

        if (string.IsNullOrEmpty(firstName))
        {
            ShowErrorNotification("Profile Error", "Please enter your first name");
            return;
        }

        if (string.IsNullOrEmpty(lastName))
        {
            ShowErrorNotification("Profile Error", "Please enter your last name");
            return;
        }

        if (string.IsNullOrEmpty(ageText) || !int.TryParse(ageText, out int age) || age < 1 || age > 120)
        {
            ShowErrorNotification("Profile Error", "Please enter a valid age (1-120)");
            return;
        }

        if (string.IsNullOrEmpty(region))
        {
            ShowErrorNotification("Profile Error", "Please enter your region");
            return;
        }

        if (string.IsNullOrEmpty(gender))
        {
            ShowErrorNotification("Profile Error", "Please select your gender");
            return;
        }

        if (note != null)
        {
            note.createNotification("Updating Profile", "Please wait...", 0, new List<string>());
        }

        if (AuthenticationManager != null)
        {
            AuthenticationManager.updateProfileFunc(firstName, lastName, age, region, gender);
            // Navigation will be handled by AuthenticationManager after successful update
        }
        else
        {
            ShowErrorNotification("Profile Error", "Authentication system not available");
        }
    }

    void DevBypassAndEnter()
    {
        var scBypass = NetworkingHandler.instance.serverConstants;
        scBypass.UserProfileData.token = string.IsNullOrEmpty(devBypassToken) ? "DEV_TOKEN" : devBypassToken;
        scBypass.UserProfileData.email = devEmail;
        scBypass.UserProfileData.first_name = string.IsNullOrEmpty(devBypassFirstName) ? "Player" : devBypassFirstName;
        scBypass.UserProfileData.points = 0;
        scBypass.UserProfileData.rewards = new int[0];

        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.SetAsRoot(ScreenId.Home);
        }
        else
        {
            SceneManager.LoadScene("GamePlay");
        }
    }

    // Show error notification using NotificationManager or fallback
    private void ShowErrorNotification(string title, string message)
    {
        Debug.LogError($"OnboardingManager: {title} - {message}");

        // Try to use NotificationManager first
        if (note != null)
        {
            try
            {
                note.createNotification(title, message, 1, new List<string> { "OK" },
                    msg => note.removeNotification());
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to show notification: {e.Message}");
            }
        }

        // Fallback: Show error in relevant UI text
        if (LoginPanelUI != null && LoginPanelUI.loginPanel != null && LoginPanelUI.loginPanel.activeInHierarchy)
        {
            if (LoginPanelUI.loginErrorText != null)
                LoginPanelUI.loginErrorText.text = $"{title}: {message}";
        }
        else if (signupPanelUI != null && signupPanelUI.signupPanel != null && signupPanelUI.signupPanel.activeInHierarchy)
        {
            if (signupPanelUI.signupErrorText != null)
                signupPanelUI.signupErrorText.text = $"{title}: {message}";
        }
        else if (otpVerifyPageUI != null && otpVerifyPageUI.otpPanel != null && otpVerifyPageUI.otpPanel.activeInHierarchy)
        {
            if (otpVerifyPageUI.otpErrorText != null)
                otpVerifyPageUI.otpErrorText.text = $"{title}: {message}";
        }
        else if (profileFormUI != null && profileFormUI.profileFormPanel != null && profileFormUI.profileFormPanel.activeInHierarchy)
        {
            if (profileFormUI.profileErrorText != null)
                profileFormUI.profileErrorText.text = $"{title}: {message}";
        }
    }

    // Validation methods
    private bool IsValidEmail(string email)
    {
        return !string.IsNullOrEmpty(email) && Regex.IsMatch(email, EmailRegexPattern);
    }

    private bool IsValidPhone(string phone)
    {
        return !string.IsNullOrEmpty(phone) && Regex.IsMatch(phone, PhoneRegexPattern);
    }

    private bool IsValidPassword(string password)
    {
        return !string.IsNullOrEmpty(password) && Regex.IsMatch(password, PasswordRegexPattern);
    }
    
    private void ConfigurePasswordField(InputField passwordField)
    {
        if (passwordField != null)
        {
            passwordField.contentType = InputField.ContentType.Password;
            passwordField.asteriskChar = '*';
        }
    }
}