using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual level item component for the level selection grid.
/// Displays "STAGE - X" format and handles level interactions.
/// </summary>
public class LevelItem : MonoBehaviour
{
    [Header("UI References")]
    public Button levelButton;
    public TextMeshProUGUI levelNumberText;
    public Image completedIcon;
    public Image backgroundImage;

    private int levelNumber;
    private LevelManager levelManager;
    private bool isCompleted = false;

    public int LevelNumber => levelNumber;

    void Awake()
    {
        // Auto-find components if not assigned
        if (levelButton == null)
            levelButton = GetComponent<Button>();

        if (levelNumberText == null)
            levelNumberText = GetComponentInChildren<TextMeshProUGUI>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (completedIcon == null)
        {
            // Look for completed icon by name
            Transform completedTransform = transform.Find("Completed_img");
            if (completedTransform != null)
                completedIcon = completedTransform.GetComponent<Image>();
        }
    }

    public void Initialize(int levelNum, LevelManager manager, bool completed = false)
    {
        levelNumber = levelNum;
        levelManager = manager;
        isCompleted = completed;

        // Setup UI
        SetupUI();

        // Setup button
        SetupButton();

        // Set visual state
        SetCompleted(completed);
    }

    public void Setup(int levelNum, bool completed, System.Action<int> onLevelSelected)
    {
        levelNumber = levelNum;
        isCompleted = completed;

        // Setup UI
        SetupUI();

        // Setup button with callback
        if (levelButton != null)
        {
            levelButton.onClick.RemoveAllListeners();
            levelButton.onClick.AddListener(() => onLevelSelected?.Invoke(levelNumber));
            levelButton.interactable = true;
        }

        // Set visual state
        SetCompleted(completed);
    }

    private void SetupUI()
    {
        // Set level number text in "STAGE - X" format
        if (levelNumberText != null)
        {
            levelNumberText.text = $"STAGE - {levelNumber}";
        }

        // Setup completed icon
        if (completedIcon != null)
        {
            completedIcon.gameObject.SetActive(false);
        }
    }

    private void SetupButton()
    {
        if (levelButton != null)
        {
            levelButton.onClick.RemoveAllListeners();
            levelButton.onClick.AddListener(OnButtonClicked);
            levelButton.interactable = true;
        }
    }

    private void OnButtonClicked()
    {
        if (levelManager != null)
        {
            levelManager.HandleLevelClicked(levelNumber);
        }
    }

    public void SetCompleted(bool completed)
    {
        isCompleted = completed;

        // Update completed icon
        if (completedIcon != null)
        {
            completedIcon.gameObject.SetActive(completed);
        }

        // Update background color - keep orange theme
        if (backgroundImage != null)
        {
            if (completed)
            {
                // Slightly darker orange for completed stages
                backgroundImage.color = new Color(0.8f, 0.3f, 0.1f, 1f);
            }
            else
            {
                // Original orange color for stage items
                backgroundImage.color = new Color(0.8745098f, 0.3254902f, 0.08627451f, 1f);
            }
        }

        // Update text color - keep white text
        if (levelNumberText != null)
        {
            levelNumberText.color = Color.white;
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (levelButton != null)
            levelButton.interactable = interactable;
    }

    // Reset for object pooling
    public void ResetForReuse()
    {
        levelNumber = 0;
        levelManager = null;
        isCompleted = false;

        if (levelButton != null)
        {
            levelButton.onClick.RemoveAllListeners();
        }

        if (completedIcon != null)
        {
            completedIcon.gameObject.SetActive(false);
        }

        if (backgroundImage != null)
        {
            // Reset to original orange color for stage items
            backgroundImage.color = new Color(0.8745098f, 0.3254902f, 0.08627451f, 1f);
        }

        if (levelNumberText != null)
        {
            levelNumberText.text = "";
            levelNumberText.color = Color.white;
        }
    }
}