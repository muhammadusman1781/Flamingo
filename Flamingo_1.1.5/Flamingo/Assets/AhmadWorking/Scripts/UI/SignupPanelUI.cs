using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SignupPanelUI
{
    public GameObject signupPanel;
    public TMP_InputField signupEmailPhoneInput;
    public TMP_InputField signupPasswordInput;
    public TMP_InputField signupConfirmPasswordInput;
    public Button signupSubmitButton;
    public Button backToLoginButton;
    public TMP_Text signupErrorText;
}
