using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class QuizHelpManager : MonoBehaviour
{
    public static QuizHelpManager Instance { get; private set; }

    [Header("Help Button References")]
    public Button helpButton;
    public Button removeTwoAnswersButton; // 50/50 lifeline
    public Button showHintButton; // Optional hint feature
    public Button skipQuestionButton; // Optional skip feature

    [Header("Help Settings")]
    [Tooltip("Number of wrong answers to remove (usually 2 for 50/50)")]
    public int wrongAnswersToRemove = 2;

    [Tooltip("Auto-find help buttons if not assigned")]
    public bool autoFindHelpButtons = true;

    [Header("Ad Integration")]
    [Tooltip("Require ad watch before using help")]
    public bool requireAdForHelp = true;

    // Track help usage (ONCE PER LEVEL, not per question)
    private bool helpUsedThisLevel = false;
    private int helpUsageCount = 0;

    // Reference to QuizUIController
    private QuizUIController quizController;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Find QuizUIController
        quizController = QuizUIController.Instance;
        
        if (quizController == null)
        {
            quizController = FindObjectOfType<QuizUIController>();
        }

        // Auto-find help buttons if enabled
        if (autoFindHelpButtons)
        {
            FindHelpButtons();
        }

        // Setup button listeners
        SetupButtonListeners();

        // Hide help buttons at start - they'll show after watching ad
        HideHelpButtons();
    }

    private void HideHelpButtons()
    {
        if (removeTwoAnswersButton != null)
        {
            removeTwoAnswersButton.gameObject.SetActive(false);
        }

        if (showHintButton != null)
        {
            showHintButton.gameObject.SetActive(false);
        }

        if (skipQuestionButton != null)
        {
            skipQuestionButton.gameObject.SetActive(false);
        }

        Debug.Log("QuizHelpManager: Help buttons hidden until ad is watched");
    }

    private void FindHelpButtons()
    {
        // Find all buttons in the scene
        Button[] allButtons = FindObjectsOfType<Button>();

        foreach (Button btn in allButtons)
        {
            string buttonName = btn.name.ToLower();

            // Try to match based on name
            if (buttonName.Contains("help") && helpButton == null)
            {
                helpButton = btn;
            }
            else if (buttonName.Contains("remove") || buttonName.Contains("50") || buttonName.Contains("lifeline"))
            {
                if (removeTwoAnswersButton == null)
                {
                    removeTwoAnswersButton = btn;
                }
            }
            else if (buttonName.Contains("hint") || buttonName.Contains("clue"))
            {
                if (showHintButton == null)
                {
                    showHintButton = btn;
                }
            }
            else if (buttonName.Contains("skip"))
            {
                if (skipQuestionButton == null)
                {
                    skipQuestionButton = btn;
                }
            }
        }
    }

    private void SetupButtonListeners()
    {
        // Setup help button
        if (helpButton != null)
        {
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(OnHelpButtonClicked);
        }

        // Setup remove two answers button
        if (removeTwoAnswersButton != null)
        {
            removeTwoAnswersButton.onClick.RemoveAllListeners();
            removeTwoAnswersButton.onClick.AddListener(OnRemoveTwoAnswersClicked);
        }

        // Setup hint button
        if (showHintButton != null)
        {
            showHintButton.onClick.RemoveAllListeners();
            showHintButton.onClick.AddListener(OnShowHintClicked);
        }

        // Setup skip button
        if (skipQuestionButton != null)
        {
            skipQuestionButton.onClick.RemoveAllListeners();
            skipQuestionButton.onClick.AddListener(OnSkipQuestionClicked);
        }
    }

    private void OnHelpButtonClicked()
    {
        if (helpUsedThisLevel)
        {
            ShowNotification("You've already used help tools this level! You can only use them once per level.", 2f);
            return;
        }

        if (requireAdForHelp)
        {
            ShowRewardedAdForHelp();
        }
        else
        {
            ActivateHelpOptions();
        }
    }

    private void ShowRewardedAdForHelp()
    {
        if (ProjectAds.instance == null)
        {
            Debug.LogError("QuizHelpManager: ProjectAds instance is null!");
            ShowNotification("Ad system not available. Help unavailable.", 2f);
            return;
        }

        Debug.Log("QuizHelpManager: Showing rewarded ad for help...");

        ShowRewardedAd(() =>
        {
            Debug.Log("QuizHelpManager: Rewarded ad watched, removing two wrong answers");
            helpUsedThisLevel = true;
            OnRemoveTwoAnswersClicked();
        },
        () =>
        {
            Debug.Log("QuizHelpManager: Rewarded ad failed or was closed");
            ShowNotification("Ad not completed. Help unavailable.", 2f);
        });
    }

    private void ActivateHelpOptions()
    {
        if (removeTwoAnswersButton != null)
        {
            removeTwoAnswersButton.interactable = true;
            removeTwoAnswersButton.gameObject.SetActive(true);
        }

        if (showHintButton != null)
        {
            showHintButton.interactable = true;
            showHintButton.gameObject.SetActive(true);
        }

        if (skipQuestionButton != null)
        {
            skipQuestionButton.interactable = true;
            skipQuestionButton.gameObject.SetActive(true);
        }

        helpUsageCount++;
        Debug.Log($"QuizHelpManager: Help activated! Usage count: {helpUsageCount}");
    }

    private void OnRemoveTwoAnswersClicked()
    {
        if (quizController == null)
        {
            Debug.LogError("QuizHelpManager: QuizUIController is null!");
            return;
        }

        // Get current question
        if (quizController.CurrentQuestions == null || 
            quizController.CurrentQuestionIndex >= quizController.CurrentQuestions.Count)
        {
            Debug.LogWarning("QuizHelpManager: No current question available");
            return;
        }

        var question = quizController.CurrentQuestions[quizController.CurrentQuestionIndex];
        string correctAnswer = question.rightAnswer;

        // Find indices of wrong answers
        List<int> wrongAnswerIndices = new List<int>();

        for (int i = 0; i < question.answer.Count; i++)
        {
            if (question.answer[i] != correctAnswer)
            {
                wrongAnswerIndices.Add(i);
            }
        }

        // Remove two random wrong answers
        if (wrongAnswerIndices.Count >= 2)
        {
            int indexesToRemove = wrongAnswersToRemove;
            if (indexesToRemove > wrongAnswerIndices.Count)
            {
                indexesToRemove = wrongAnswerIndices.Count;
            }

            // Randomly select which wrong answers to remove
            List<int> answersToRemove = wrongAnswerIndices.OrderBy(x => Random.value)
                                                          .Take(indexesToRemove)
                                                          .ToList();

            // Hide the selected wrong answer buttons
            foreach (int index in answersToRemove)
            {
                if (quizController.AnswerButtons[index] != null)
                {
                    quizController.AnswerButtons[index].interactable = false;
                    
                    // Fade out animation (optional)
                    StartCoroutine(FadeOutAnswer(quizController.AnswerButtons[index]));
                }
            }

            Debug.Log($"QuizHelpManager: Removed {indexesToRemove} wrong answers. Remaining: {question.answer.Count - indexesToRemove}");
            
            // Disable and hide the button after use
            if (removeTwoAnswersButton != null)
            {
                removeTwoAnswersButton.interactable = false;
                removeTwoAnswersButton.gameObject.SetActive(false);
            }
            
            // Also hide other help buttons
            if (showHintButton != null)
            {
                showHintButton.gameObject.SetActive(false);
            }
            
            if (skipQuestionButton != null)
            {
                skipQuestionButton.gameObject.SetActive(false);
            }

        }
    }

    private void OnShowHintClicked()
    {
        if (quizController == null)
        {
            Debug.LogError("QuizHelpManager: QuizUIController is null!");
            return;
        }

        var question = quizController.CurrentQuestions[quizController.CurrentQuestionIndex];
        string correctAnswer = question.rightAnswer;

        // Find correct answer index
        int correctIndex = -1;
        for (int i = 0; i < question.answer.Count; i++)
        {
            if (question.answer[i] == correctAnswer)
            {
                correctIndex = i;
                break;
            }
        }

        // Highlight correct answer briefly (optional)
        if (correctIndex >= 0 && quizController.AnswerButtons[correctIndex] != null)
        {
            StartCoroutine(HighlightCorrectAnswer(correctIndex));
        }

        Debug.Log("QuizHelpManager: Hint shown");

        // Disable and hide the button after use
        if (showHintButton != null)
        {
            showHintButton.interactable = false;
            showHintButton.gameObject.SetActive(false);
        }
        
        // Also hide other help buttons
        if (removeTwoAnswersButton != null)
        {
            removeTwoAnswersButton.gameObject.SetActive(false);
        }
        
        if (skipQuestionButton != null)
        {
            skipQuestionButton.gameObject.SetActive(false);
        }

    }

    private void OnSkipQuestionClicked()
    {
        if (quizController == null)
        {
            Debug.LogError("QuizHelpManager: QuizUIController is null!");
            return;
        }

        quizController.Invoke("NextQuestion", 0f);

        Debug.Log("QuizHelpManager: Question skipped");

        // Disable and hide the button after use
        if (skipQuestionButton != null)
        {
            skipQuestionButton.interactable = false;
            skipQuestionButton.gameObject.SetActive(false);
        }
        
        // Also hide other help buttons
        if (removeTwoAnswersButton != null)
        {
            removeTwoAnswersButton.gameObject.SetActive(false);
        }
        
        if (showHintButton != null)
        {
            showHintButton.gameObject.SetActive(false);
        }

    }

    private IEnumerator FadeOutAnswer(Button button)
    {
        if (button == null) yield break;

        Image image = button.GetComponent<Image>();
        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
        }

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsed / duration);
            yield return null;
        }

        button.gameObject.SetActive(false);
        Destroy(canvasGroup);
    }

    private IEnumerator HighlightCorrectAnswer(int correctIndex)
    {
        if (quizController == null || correctIndex < 0 || correctIndex >= quizController.AnswerButtons.Length)
            yield break;

        Button correctButton = quizController.AnswerButtons[correctIndex];
        if (correctButton == null) yield break;

        Image image = correctButton.GetComponent<Image>();
        Color originalColor = Color.white;
        
        if (image != null)
        {
            originalColor = image.color;
        }

        // Flash green color
        Color startColor = Color.green;
        Color endColor = originalColor;
        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (image != null)
            {
                image.color = Color.Lerp(startColor, endColor, elapsed / duration);
            }
            yield return null;
        }

        if (image != null)
        {
            image.color = originalColor;
        }
    }

    private void ShowRewardedAd(System.Action onSuccess, System.Action onFailure)
    {
        if (ProjectAds.instance == null)
        {
            onFailure?.Invoke();
            return;
        }

        // Get WhiteAdManager to set up callbacks
        WhiteAdManager adManager = ProjectAds.instance.GetComponent<WhiteAdManager>();
        
        if (adManager == null)
        {
            Debug.LogError("QuizHelpManager: WhiteAdManager not found!");
            onFailure?.Invoke();
            return;
        }

        // Subscribe to ad callbacks
        adManager.OnRewardedAdWatched = null; // Clear previous
        adManager.OnRewardedAdWatched = () =>
        {
            Debug.Log("QuizHelpManager: Rewarded ad watched successfully!");
            onSuccess?.Invoke();
            adManager.OnRewardedAdWatched = null; // Clear callback
        };

        adManager.OnRewardedAdFailed = null; // Clear previous
        adManager.OnRewardedAdFailed = () =>
        {
            Debug.Log("QuizHelpManager: Rewarded ad failed");
            onFailure?.Invoke();
            adManager.OnRewardedAdFailed = null; // Clear callback
        };

        // Show the ad
        ProjectAds.instance.ShowRewardedAd();
    }

    private void ShowNotification(string message, float duration = 2f)
    {
        Debug.Log($"QuizHelpManager: {message}");
        // TODO: Integrate with notification system
    }

    public void ResetHelpForNextQuestion()
    {
        // DO NOT reset helpUsedThisLevel - help can only be used ONCE per level
        // Just hide the help option buttons if they were used

        // Keep help buttons HIDDEN - they'll show after ad is watched (but only if not used this level)
        if (helpUsedThisLevel)
        {
            // If help was already used this level, keep buttons hidden
            if (removeTwoAnswersButton != null)
            {
                removeTwoAnswersButton.gameObject.SetActive(false);
            }
            
            if (showHintButton != null)
            {
                showHintButton.gameObject.SetActive(false);
            }

            if (skipQuestionButton != null)
            {
                skipQuestionButton.gameObject.SetActive(false);
            }
        }
        
        Debug.Log($"QuizHelpManager: Help status for next question - Used this level: {helpUsedThisLevel}");
    }

    public void ResetHelpForNewLevel()
    {
        helpUsedThisLevel = false;

        // Hide all help buttons until ad is watched
        if (removeTwoAnswersButton != null)
        {
            removeTwoAnswersButton.gameObject.SetActive(false);
        }
        
        if (showHintButton != null)
        {
            showHintButton.gameObject.SetActive(false);
        }

        if (skipQuestionButton != null)
        {
            skipQuestionButton.gameObject.SetActive(false);
        }
        
        Debug.Log("QuizHelpManager: Help reset for new level. Help tools are now available (after watching ad).");
    }

    public int GetHelpUsageCount()
    {
        return helpUsageCount;
    }

    public void SetRequireAdForHelp(bool requireAd)
    {
        requireAdForHelp = requireAd;
    }
}

