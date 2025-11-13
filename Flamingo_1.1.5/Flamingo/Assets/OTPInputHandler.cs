using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class OTPInputHandler : MonoBehaviour
{
    // Assign all your OTP input fields in the Inspector, in order.
    public InputField[] otpInputs;

    void Start()
    {
        if (otpInputs == null || otpInputs.Length == 0)
        {
            Debug.LogError("OTPInputHandler: No input fields assigned!");
            return;
        }

        // Set up listeners for each input field
        for (int i = 0; i < otpInputs.Length; i++)
        {
            otpInputs[i].characterLimit = 1;
            int currentIndex = i;
            otpInputs[i].onValueChanged.AddListener(value => OnOTPFieldValueChanged(currentIndex, value));
        }
    }

    // --- NEW SECTION: Handles Backspace on Empty Fields ---
    void Update()
    {
        // Check for the backspace key press
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            // Find the currently focused input field
            for (int i = 0; i < otpInputs.Length; i++)
            {
                if (otpInputs[i].isFocused)
                {
                    // If the focused field is empty, clear the text of the previous one
                    if (string.IsNullOrEmpty(otpInputs[i].text))
                    {
                        int prevIndex = i - 1;
                        if (prevIndex >= 0)
                        {
                            // Clear the previous field's text and activate it
                            otpInputs[prevIndex].text = "";
                            otpInputs[prevIndex].ActivateInputField();
                            otpInputs[prevIndex].Select();
                        }
                    }
                    // Break after handling the focused field
                    break;
                }
            }
        }
    }
    // --- END OF NEW SECTION ---

    private void OnOTPFieldValueChanged(int index, string value)
    {
        // If a character is entered
        if (!string.IsNullOrEmpty(value))
        {
            // Move to the next input field
            int nextIndex = index + 1;
            if (nextIndex < otpInputs.Length && otpInputs[nextIndex] != null)
            {
                otpInputs[nextIndex].ActivateInputField();
                otpInputs[nextIndex].Select();
            }
        }
        // The user experience will be: delete char -> field becomes empty -> press backspace again -> previous field is cleared.
    }

    // Call this method to focus the first input field when the panel is shown
    public void ActivateFirstInput()
    {
        if (otpInputs != null && otpInputs.Length > 0 && otpInputs[0] != null)
        {
            otpInputs[0].ActivateInputField();
            otpInputs[0].Select();
        }
    }

    // Helper method to get the full OTP code
    public string GetOTPCode()
    {
        if (otpInputs == null) return "";
        return string.Concat(otpInputs.Select(i => i.text));
    }
    
    // Helper method to clear all input fields
    public void ClearAllInputs()
    {
        if (otpInputs == null) return;
        
        foreach (var input in otpInputs)
        {
            if (input != null)
            {
                input.text = "";
            }
        }
    }
}