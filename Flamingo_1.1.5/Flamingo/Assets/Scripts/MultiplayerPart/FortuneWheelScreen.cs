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
    public List<RTLTextMeshPro> slotTexts = new List<RTLTextMeshPro>(); // Assign 10 text fields for wheel slots

    [Header("Result Display")]
    public GameObject resultPanel;
    public RTLTextMeshPro resultText;
    public Button claimButton;

    // Private variables
    private List<SpinOption> spinOptions;
    private bool isSpinning = false;
    private SpinOption selectedReward;
    private float[] slotAngles; // Angles for each slot on the wheel

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
    }

    void OnEnable()
    {
        // Fetch spin options from server
        FetchSpinOptions();
        
        // Reset wheel rotation
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
            slotAngles[i] = i * angleStep;
        }

        Debug.Log($"Calculated {slotCount} slot angles with step: {angleStep} degrees");
    }

    void UpdateWheelSlotTexts()
    {
        if (spinOptions == null || slotTexts == null) return;

        int minCount = Mathf.Min(spinOptions.Count, slotTexts.Count);

        for (int i = 0; i < minCount; i++)
        {
            if (slotTexts[i] != null)
            {
                SpinOption option = spinOptions[i];
                string displayText = GetDisplayTextForOption(option);
                slotTexts[i].text = displayText;
                Debug.Log($"Slot {i}: {displayText}");
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
        // The arrow is at the top (0 degrees), so we need to rotate the wheel
        // so that the selected slot is at the top
        float targetSlotAngle = slotAngles[selectedIndex];
        
        // Random number of full spins
        int fullSpins = Random.Range(minSpins, maxSpins + 1);
        float totalRotation = (fullSpins * 360f) + (360f - targetSlotAngle);

        Debug.Log($"Target slot index: {selectedIndex}, Slot angle: {targetSlotAngle}");
        Debug.Log($"Full spins: {fullSpins}, Total rotation: {totalRotation} degrees");

        // Starting rotation
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

            float currentRotation = Mathf.Lerp(startRotation, endRotation, easedProgress);
            wheelTransform.rotation = Quaternion.Euler(0, 0, currentRotation);

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
        // Hide result panel
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        // Re-enable spin button (or implement claim logic here)
        if (spinButton != null)
        {
            spinButton.interactable = true;
        }

        // TODO: Implement actual claim logic - send reward to server
        Debug.Log("Reward claimed!");
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

