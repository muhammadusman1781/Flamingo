using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ProfileFormUI
{
    public GameObject profileFormPanel;
    public InputField firstNameInput;
    public InputField lastNameInput;
    public InputField phoneNumberInput;
    public Dropdown genderDropdown;
    public InputField regionInput;
    public InputField ageInput;
    public Button profileSubmitButton;
    public TMP_Text profileErrorText;
}
