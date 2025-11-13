using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SignupPanelUI
{
    public GameObject signupPanel;
    public InputField signupEmailPhoneInput;
    public InputField signupPasswordInput;
    public InputField signupConfirmPasswordInput;
    public Button signupSubmitButton;
    public Button backToLoginButton;
    public TMP_Text signupErrorText;
}
