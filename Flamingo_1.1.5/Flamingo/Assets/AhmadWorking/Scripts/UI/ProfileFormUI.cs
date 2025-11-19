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
    public Dropdown gradeDropdown;
    public InputField regionInput;
    public InputField ageInput;
    public Button profileSubmitButton;
    public TMP_Text profileErrorText;
    public Button MaleButton;
    public Button FemaleButton;
    public bool isMale = true;
    public bool isFemale = false;
}
