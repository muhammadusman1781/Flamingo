using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RTLTMPro;

public class FortuneWheelScreen : MonoBehaviour
{
    [Header("Wheel Settings")]
    public Transform wheelTransform; // The wheel GameObject transform
    public Button spinButton;
    public float spinDuration = 3f; // Duration of spin animation
    public int minSpins = 3; // Minimum number of full rotations
    public int maxSpins = 5; // Maximum number of full rotations

    [Header("Wheel Slot Texts")]
    public List<Text> slotAmountTexts = new List<Text>(); // Assign text fields for amounts (e.g., "50")
    public List<Text> slotTypeTexts = new List<Text>(); // Assign text fields for types (e.g., "Dinars")

    [Header("Result Display")]
    public GameObject resultPanel;
    public RTLTextMeshPro resultText;
    public Button claimButton;

    // Private variables
    private List<SpinOption> spinOptions;
    private bool isSpinning = false;
    private SpinOption selectedReward;
    private float[] slotAngles; // Angles for each slot on the wheel

    public Button homeButton;

    void Start()
    {
        // Setup button listeners
        if (spinButton != null)
        {
            spinButton.onClick.AddListener(OnSpinButtonClick);
        }

        if (claimButton != null)
        {
            claimButton.onClick.AddListener(OnClaimButtonClick);
        }

        // Hide result panel initially
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (homeButton != null)
        {
            homeButton.onClick.AddListener(OnHomeButtonClick);
        }
    }

    void OnEnable()
    {
        // Fetch spin options from server
        FetchSpinOptions();
        
        // Reset wheel rotation to 0 when screen opens
        // Comment this out if you want to keep the wheel at its previous position
        if (wheelTransform != null)
        {
            wheelTransform.rotation = Quaternion.Euler(0, 0, 0);
        }

        // Hide result panel
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        // Enable spin button
        if (spinButton != null)
        {
            spinButton.interactable = true;
        }
    }

    void FetchSpinOptions()
    {
        if (NetworkingHandler.instance == null)
        {
            Debug.LogError("NetworkingHandler instance is not available!");
            return;
        }

        if (NetworkingHandler.instance.serverConstants == null)
        {
            Debug.LogError("ServerConstants is not assigned!");
            return;
        }

        string apiUrl = NetworkingHandler.instance.serverConstants.baseUrl + "/auth/spins/";
        Debug.Log($"Fetching spin options from: {apiUrl}");

        NetworkingHandler.instance.getMessage(
            apiUrl,
            isTokenNeeded: true,
            onSuccess: OnFetchSpinOptionsSuccess,
            onFail: OnFetchSpinOptionsFail
        );
    }

    void OnFetchSpinOptionsSuccess(string response)
    {
        Debug.Log($"Spin options fetched successfully: {response}");

        try
        {
            FortuneWheelResponse wheelResponse = JsonUtility.FromJson<FortuneWheelResponse>(response);

            if (wheelResponse != null && wheelResponse.data != null && wheelResponse.data.Count > 0)
            {
                spinOptions = wheelResponse.data;
                Debug.Log($"Loaded {spinOptions.Count} spin options");

                // Initialize slot angles based on number of options
                CalculateSlotAngles();

                // Update wheel slot texts
                UpdateWheelSlotTexts();
            }
            else
            {
                Debug.LogError("Failed to parse spin options or data is empty");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing spin options: {ex.Message}");
        }
    }

    void OnFetchSpinOptionsFail(string error)
    {
        Debug.LogError($"Failed to fetch spin options: {error}");
    }

    void CalculateSlotAngles()
    {
        if (spinOptions == null || spinOptions.Count == 0) return;

        int slotCount = spinOptions.Count;
        slotAngles = new float[slotCount];
        float angleStep = 360f / slotCount;

        for (int i = 0; i < slotCount; i++)
        {
            // Calculate angle for each slot (clockwise from top)
            // Slot 0 is at the top (0 degrees), slot 1 is at angleStep degrees, etc.
            slotAngles[i] = i * angleStep;
            Debug.Log($"Slot {i} angle: {slotAngles[i]} degrees");
        }

        Debug.Log($"Calculated {slotCount} slot angles with step: {angleStep} degrees");
    }

    void UpdateWheelSlotTexts()
    {
        if (spinOptions == null) return;

        // Update amount texts
        if (slotAmountTexts != null)
        {
            int minCount = Mathf.Min(spinOptions.Count, slotAmountTexts.Count);
            for (int i = 0; i < minCount; i++)
            {
                if (slotAmountTexts[i] != null)
                {
                    SpinOption option = spinOptions[i];
                    slotAmountTexts[i].text = option.value.ToString();
                }
            }
        }

        // Update type texts
        if (slotTypeTexts != null)
        {
            int minCount = Mathf.Min(spinOptions.Count, slotTypeTexts.Count);
            for (int i = 0; i < minCount; i++)
            {
                if (slotTypeTexts[i] != null)
                {
                    SpinOption option = spinOptions[i];
                    string typeText = GetTypeDisplayText(option.type);
                    slotTypeTexts[i].text = typeText;
                    Debug.Log($"Slot {i}: {option.value} {typeText}");
                }
            }
        }
    }

    string GetDisplayTextForOption(SpinOption option)
    {
        switch (option.type.ToLower())
        {
            case "coin":
                return $"{option.value} Coins";
            case "dinars":
                return $"{option.value} Dinars";
            case "tabs":
                return $"{option.value} Tabs";
            default:
                return $"{option.value} {option.type}";
        }
    }

    string GetTypeDisplayText(string type)
    {
        switch (type.ToLower())
        {
            case "coin":
                return "Coins";
            case "dinars":
                return "Dinars";
            case "tabs":
                return "Tabs";
            default:
                return type;
        }
    }

    void OnSpinButtonClick()
    {
        if (isSpinning)
        {
            Debug.LogWarning("Wheel is already spinning!");
            return;
        }

        if (spinOptions == null || spinOptions.Count == 0)
        {
            Debug.LogError("No spin options available!");
            return;
        }

        // Disable spin button during spin
        if (spinButton != null)
        {
            spinButton.interactable = false;
        }

        // Select reward based on probability
        selectedReward = SelectRewardByProbability();
        
        Debug.Log($"Selected reward: {GetDisplayTextForOption(selectedReward)} (ID: {selectedReward.id}, Probability: {selectedReward.percentage}%)");

        // Start spinning animation
        StartCoroutine(SpinWheel());
    }

    SpinOption SelectRewardByProbability()
    {
        // Calculate total percentage (should be 100, but let's be safe)
        float totalPercentage = 0f;
        foreach (var option in spinOptions)
        {
            float percentage;
            if (float.TryParse(option.percentage, out percentage))
            {
                totalPercentage += percentage;
            }
        }

        // Generate random number between 0 and total percentage
        float randomValue = Random.Range(0f, totalPercentage);
        
        Debug.Log($"Random value: {randomValue} out of {totalPercentage}");

        // Select option based on cumulative probability
        float cumulativePercentage = 0f;
        foreach (var option in spinOptions)
        {
            float percentage;
            if (float.TryParse(option.percentage, out percentage))
            {
                cumulativePercentage += percentage;
                if (randomValue <= cumulativePercentage)
                {
                    return option;
                }
            }
        }

        // Fallback: return first option if something goes wrong
        return spinOptions[0];
    }

    IEnumerator SpinWheel()
    {
        isSpinning = true;

        // Find the index of selected reward
        int selectedIndex = spinOptions.IndexOf(selectedReward);
        if (selectedIndex == -1)
        {
            Debug.LogError("Selected reward not found in options list!");
            selectedIndex = 0;
        }

        // Calculate target angle
        // The pin is at the top (0 degrees)
        // We need to rotate the wheel so the selected slot aligns with the pin
        float targetSlotAngle = slotAngles[selectedIndex];
        
        // Random number of full spins
        int fullSpins = Random.Range(minSpins, maxSpins + 1);
        
        // Get current rotation normalized to 0-360 range
        float currentRotation = wheelTransform.eulerAngles.z % 360f;
        if (currentRotation < 0) currentRotation += 360f;
        
        // Calculate the target angle where we want to end up (slot aligned with pin at top)
        // Since we rotate clockwise and slot angles are clockwise from top,
        // we need to rotate to (360 - targetSlotAngle) to bring the slot to the top
        float finalTargetAngle = 360f - targetSlotAngle;
        
        // Calculate how much we need to rotate from current position
        // We add full spins and then rotate to the final target
        float rotationNeeded = finalTargetAngle - currentRotation;
        if (rotationNeeded < 0) rotationNeeded += 360f; // Ensure positive rotation
        
        float totalRotation = (fullSpins * 360f) + rotationNeeded;

        Debug.Log($"Current rotation: {currentRotation}°, Target slot index: {selectedIndex}, Slot angle: {targetSlotAngle}°");
        Debug.Log($"Final target: {finalTargetAngle}°, Rotation needed: {rotationNeeded}°, Full spins: {fullSpins}, Total rotation: {totalRotation}°");

        // Starting and ending rotation
        float startRotation = wheelTransform.eulerAngles.z;
        float endRotation = startRotation + totalRotation;

        // Spin animation with easing
        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / spinDuration;

            // Ease out cubic for smooth deceleration
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

            float rotationAngle = Mathf.Lerp(startRotation, endRotation, easedProgress);
            wheelTransform.rotation = Quaternion.Euler(0, 0, rotationAngle);

            yield return null;
        }

        // Ensure final rotation is exact
        wheelTransform.rotation = Quaternion.Euler(0, 0, endRotation);

        isSpinning = false;

        // Show result after a short delay
        yield return new WaitForSeconds(0.5f);
        ShowResult();
    }

    void ShowResult()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (resultText != null && selectedReward != null)
        {
            string rewardText = $"You won: {GetDisplayTextForOption(selectedReward)}!";
            resultText.text = rewardText;
            Debug.Log(rewardText);
        }
    }

    void OnClaimButtonClick()
    {
        if (selectedReward == null)
        {
            Debug.LogError("No reward selected to claim!");
            return;
        }

        // Disable claim button to prevent multiple clicks
        if (claimButton != null)
        {
            claimButton.interactable = false;
        }

        Debug.Log($"Claiming reward: ID={selectedReward.id}, Type={selectedReward.type}, Value={selectedReward.value}");
        
        // Send claim request to server
        ClaimReward();
    }

    void ClaimReward()
    {
        if (NetworkingHandler.instance == null)
        {
            Debug.LogError("NetworkingHandler instance is not available!");
            OnClaimRewardFail("NetworkingHandler not available");
            return;
        }

        if (NetworkingHandler.instance.serverConstants == null)
        {
            Debug.LogError("ServerConstants is not assigned!");
            OnClaimRewardFail("ServerConstants not assigned");
            return;
        }

        // Create request payload
        SpinUpdateRequest request = new SpinUpdateRequest
        {
            id = selectedReward.id,
            action = "add"
        };

        string jsonToSend = JsonUtility.ToJson(request);
        string apiUrl = NetworkingHandler.instance.serverConstants.baseUrl + "/auth/spin/update/";

        Debug.Log($"Claiming reward to: {apiUrl}");
        Debug.Log($"Request payload: {jsonToSend}");

        NetworkingHandler.instance.postMessage(
            apiUrl,
            jsonToSend,
            isTokenNeeded: true,
            onSuccess: OnClaimRewardSuccess,
            onFail: OnClaimRewardFail
        );
    }

    void OnClaimRewardSuccess(string response)
    {
        Debug.Log($"Reward claimed successfully: {response}");

        try
        {
            SpinUpdateResponse claimResponse = JsonUtility.FromJson<SpinUpdateResponse>(response);

            if (claimResponse != null && claimResponse.status == "success")
            {
                Debug.Log($"Success: {claimResponse.message}");
                
                // Hide result panel
                if (resultPanel != null)
                {
                    resultPanel.SetActive(false);
                }

                // Re-enable spin button for next spin
                if (spinButton != null)
                {
                    spinButton.interactable = true;
                }

                // Re-enable claim button
                if (claimButton != null)
                {
                    claimButton.interactable = true;
                }

                // Optionally refresh user profile to update coins/dinars
                // You might want to call a method to refresh the user's balance here
            }
            else
            {
                Debug.LogError("Claim response status is not success");
                OnClaimRewardFail("Invalid response status");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing claim response: {ex.Message}");
            OnClaimRewardFail($"Parse error: {ex.Message}");
        }
    }

    void OnClaimRewardFail(string error)
    {
        Debug.LogError($"Failed to claim reward: {error}");

        // Re-enable claim button so user can try again
        if (claimButton != null)
        {
            claimButton.interactable = true;
        }

        // Optionally show error message to user
        // ShowErrorMessage($"Failed to claim reward: {error}");
    }

    void OnHomeButtonClick()
    {
        // Go to home screen
        UIScreensManager.Instance.GoToHomeScreen();
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        // Clean up listeners
        if (spinButton != null)
        {
            spinButton.onClick.RemoveAllListeners();
        }

        if (claimButton != null)
        {
            claimButton.onClick.RemoveAllListeners();
        }
    }
}

