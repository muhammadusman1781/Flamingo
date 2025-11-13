using System;
using System.Collections;
using UnityEngine;
using TMPro; // It is strongly recommended to use TextMeshProUGUI instead of legacy Text
using UnityEngine.UI;

// AwaisDev: Handles the result screen after quiz completion
public class QuizResultManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject resultPanel;

    [Header("Score Display")]
    public Text scoreText; // Consider changing to TextMeshProUGUI
    public Text timeText;
    public Text correctAnswersText;
    public Text wrongAnswersText;
    public Text accuracyText;
    public Text gradeText;
    public Text performanceMessageText;
    public Text stageText;

    [Header("Visual Elements")]
    public Image[] starImages; // For star rating display
    public Image accuracyBar;
    public Text totalQuestionsText;

    [Header("Buttons")]
    public Button playAgainButton;
    public Button nextLevelButton;
    public Button mainMenuButton;
    public Button shareButton;

    [Header("Ad Integration Buttons")]
    public Button continueAfterLossButton;
    public Button doubleCoinsButton;

    [Header("Animation Settings")]
    public float scoreCountDuration = 2f;
    public float timeCountDuration = 1.5f;

    [Header("Ad Settings")]
    [Tooltip("Require ad for continue after loss")]
    public bool requireAdToContinueAfterLoss = true;
    [Tooltip("Require ad to double coins")]
    public bool requireAdToDoubleCoins = true;

    // --- MODIFICATION START ---
    private int completedLevelNumber;
    private const int MAX_LEVEL_COUNT = 250; // Set your total number of levels here.
    // --- MODIFICATION END ---

    private QuizResultData currentResult;
    private bool isAnimating = false;
    private bool coinsDoubled = false;

    // Events
    public static event Action OnPlayAgain;
    public static event Action OnNextLevel;
    public static event Action OnMainMenu;
    public static event Action<QuizResultData> OnShareResult;
    public static event Action OnCoinsDoubled;
    public static event Action OnContinuedAfterLoss;

    private void Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        SetupButtonListeners();
    }

    private void OnEnable()
    {
        // Ensure stage text is ready as soon as the result screen is opened
        if (stageText != null)
        {
            int stageToShow = 0;
            if (currentResult != null && currentResult.levelNumber > 0)
            {
                stageToShow = currentResult.levelNumber;
            }
            else if (completedLevelNumber > 0)
            {
                stageToShow = completedLevelNumber;
            }
            else if (QuizUIController.Instance != null)
            {
                stageToShow = QuizUIController.Instance.currentLevelNumber;
            }

            if (stageToShow > 0)
            {
                stageText.text = $"Stage - {stageToShow}";
            }
        }
    }

    public void ShowResult(QuizResultData resultData)
    {
        if (resultPanel == null)
        {
            Debug.LogError("Result panel is not assigned!");
            return;
        }

        currentResult = resultData;

        // --- MODIFICATION START ---
        completedLevelNumber = currentResult.levelNumber;

        if (stageText != null && completedLevelNumber > 0)
        {
            stageText.text = $"Stage - {completedLevelNumber}";
        }

        if (nextLevelButton != null)
        {
            bool isLastLevel = completedLevelNumber >= MAX_LEVEL_COUNT;
            nextLevelButton.gameObject.SetActive(!isLastLevel);
        }
        // --- MODIFICATION END ---

        // Reset coins doubled flag
        coinsDoubled = false;

        // Determine win/loss status
        bool didWin = currentResult.correctAnswers > currentResult.wrongAnswers && 
                     (currentResult.accuracyPercentage >= 50f);

        // Show/hide buttons based on win/loss
        if (didWin)
        {
            // User won - show double coins button
            if (doubleCoinsButton != null)
            {
                doubleCoinsButton.gameObject.SetActive(true);
                doubleCoinsButton.interactable = true;
            }
            
            // Next level button works normally (no ad required)
            if (nextLevelButton != null)
            {
                // No special handling - works normally
            }
        }
        else
        {
            // User lost - show continue button (if exists as separate button)
            if (continueAfterLossButton != null)
            {
                continueAfterLossButton.gameObject.SetActive(true);
                continueAfterLossButton.interactable = true;
            }
            
            // Hide double coins button
            if (doubleCoinsButton != null)
            {
                doubleCoinsButton.gameObject.SetActive(false);
            }

            // Next level button will require ad (handled in click listener)
        }

        resultPanel.SetActive(true);
        StartCoroutine(AnimateResults());
    }

    public void HideResult()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void SetupButtonListeners()
    {
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(() => {
                // --- MODIFICATION ---
                if (QuizUIController.Instance != null)
                {
                    QuizUIController.Instance.LoadLevel(completedLevelNumber);
                    if (ScreenManager.Instance != null) ScreenManager.Instance.Show(ScreenId.Quiz);
                }
                else
                {
                    Debug.LogError("QuizUIController is null! Cannot restart level.");
                }
            });

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(() => {
                // Check if user lost - if so, require ad to continue
                bool didLose = currentResult != null && 
                              (currentResult.wrongAnswers > currentResult.correctAnswers || 
                               currentResult.accuracyPercentage < 50f);

                if (didLose && requireAdToContinueAfterLoss)
                {
                    // User lost - show ad before continuing
                    ShowRewardedAdForNextLevel();
                }
                else
                {
                    // User won OR ads not required - proceed normally
                    ProceedToNextLevel();
                }
            });

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => {
                OnMainMenu?.Invoke();
                // Go to the stage selection screen, not the initial home screen.
                if (ScreenManager.Instance != null) ScreenManager.Instance.Show(ScreenId.StageSelection);
            });

        if (shareButton != null)
            shareButton.onClick.AddListener(() => OnShareResult?.Invoke(currentResult));

        // Setup ad integration buttons (if separate buttons exist)
        if (continueAfterLossButton != null)
            continueAfterLossButton.onClick.AddListener(OnContinueAfterLossClicked);

        if (doubleCoinsButton != null)
            doubleCoinsButton.onClick.AddListener(OnDoubleCoinsClicked);
    }

    private void ShowRewardedAdForNextLevel()
    {
        if (ProjectAds.instance == null)
        {
            Debug.LogError("QuizResultManager: ProjectAds instance not found!");
            ShowNotification("Cannot continue. Ad system unavailable.");
            return;
        }

        WhiteAdManager adManager = ProjectAds.instance.GetComponent<WhiteAdManager>();
        
        if (adManager == null)
        {
            Debug.LogError("QuizResultManager: WhiteAdManager not found!");
            return;
        }

        // Setup callbacks
        adManager.OnRewardedAdWatched = () =>
        {
            Debug.Log("QuizResultManager: Ad watched for next level");
            ProceedToNextLevel();
            adManager.OnRewardedAdWatched = null;
        };

        adManager.OnRewardedAdFailed = () =>
        {
            Debug.LogError("QuizResultManager: Ad failed for next level");
            ShowNotification("Ad unavailable. Cannot continue to next level.");
            adManager.OnRewardedAdFailed = null;
        };

        // Show ad
        ProjectAds.instance.ShowRewardedAd();
    }

    private void ProceedToNextLevel()
    {
        OnNextLevel?.Invoke();

        // Load next level
        if (QuizUIController.Instance != null)
        {
            int nextLevel = completedLevelNumber + 1;
            QuizUIController.Instance.LoadLevel(nextLevel);
            if (ScreenManager.Instance != null) 
                ScreenManager.Instance.Show(ScreenId.Quiz);
        }

        // Hide result panel
        HideResult();
    }

    private void OnContinueAfterLossClicked()
    {
        if (currentResult == null)
        {
            Debug.LogError("QuizResultManager: No current result available");
            return;
        }

        // Check if user lost (accuracy below 50% or more wrong than correct)
        bool didLose = currentResult.wrongAnswers > currentResult.correctAnswers || 
                      (currentResult.accuracyPercentage < 50f);

        if (!didLose)
        {
            Debug.Log("QuizResultManager: User won, no need for continue logic");
            return;
        }

        if (requireAdToContinueAfterLoss)
        {
            ShowRewardedAdForContinue();
        }
        else
        {
            // No ad required - allow continue immediately
            ContinueAfterLoss();
        }
    }

    private void ShowRewardedAdForContinue()
    {
        if (ProjectAds.instance == null)
        {
            Debug.LogError("QuizResultManager: ProjectAds instance not found!");
            ShowNotification("Cannot continue. Ad system unavailable.");
            return;
        }

        WhiteAdManager adManager = ProjectAds.instance.GetComponent<WhiteAdManager>();
        
        if (adManager == null)
        {
            Debug.LogError("QuizResultManager: WhiteAdManager not found!");
            return;
        }

        // Setup callbacks
        adManager.OnRewardedAdWatched = () =>
        {
            Debug.Log("QuizResultManager: Ad watched, allowing continue after loss");
            ContinueAfterLoss();
            adManager.OnRewardedAdWatched = null;
        };

        adManager.OnRewardedAdFailed = () =>
        {
            Debug.LogError("QuizResultManager: Ad failed, cannot continue");
            ShowNotification("Ad unavailable. Cannot continue.");
            adManager.OnRewardedAdFailed = null;
        };

        // Show ad
        ProjectAds.instance.ShowRewardedAd();
    }

    private void ContinueAfterLoss()
    {
        OnContinuedAfterLoss?.Invoke();

        // If QuizUIController wants to continue within the same level, resume instead of moving to next level
        if (QuizUIController.Instance != null)
        {
            QuizUIController.Instance.ResumeAfterLoss();
            return;
        }

        // Fallback: proceed to next level
        ProceedToNextLevel();
    }

    private void OnDoubleCoinsClicked()
    {
        if (coinsDoubled)
        {
            ShowNotification("You've already doubled your coins for this round!", 2f);
            return;
        }

        if (currentResult == null)
        {
            Debug.LogError("QuizResultManager: No current result available");
            return;
        }

        if (requireAdToDoubleCoins)
        {
            ShowRewardedAdForDoubleCoins();
        }
        else
        {
            // No ad required - double immediately
            DoubleCoins();
        }
    }

    private void ShowRewardedAdForDoubleCoins()
    {
        if (ProjectAds.instance == null)
        {
            Debug.LogError("QuizResultManager: ProjectAds instance not found!");
            ShowNotification("Cannot double coins. Ad system unavailable.");
            return;
        }

        WhiteAdManager adManager = ProjectAds.instance.GetComponent<WhiteAdManager>();
        
        if (adManager == null)
        {
            Debug.LogError("QuizResultManager: WhiteAdManager not found!");
            return;
        }

        // Setup callbacks
        adManager.OnRewardedAdWatched = () =>
        {
            Debug.Log("QuizResultManager: Ad watched, doubling coins");
            DoubleCoins();
            adManager.OnRewardedAdWatched = null;
        };

        adManager.OnRewardedAdFailed = () =>
        {
            Debug.LogError("QuizResultManager: Ad failed, cannot double coins");
            ShowNotification("Ad unavailable. Cannot double coins.");
            adManager.OnRewardedAdFailed = null;
        };

        // Show ad
        ProjectAds.instance.ShowRewardedAd();
    }

    private void DoubleCoins()
    {
        if (currentResult == null) return;

        // Double the score/coins
        currentResult.score = currentResult.score * 2;

        // Update UI to show doubled amount
        if (scoreText != null)
        {
            scoreText.text = currentResult.score.ToString();
        }

        // Mark as doubled
        coinsDoubled = true;

        // Disable the button
        if (doubleCoinsButton != null)
        {
            doubleCoinsButton.interactable = false;
        }

        // Notify listeners
        OnCoinsDoubled?.Invoke();

        // TODO: Update user currency in ProfileDataManager or StatisticsTracker
        Debug.Log($"QuizResultManager: Coins doubled to {currentResult.score}");
        
        ShowNotification("Coins doubled!", 2f);
    }

    private void ShowNotification(string message, float duration = 2f)
    {
        Debug.Log($"QuizResultManager: {message}");
        // TODO: Integrate with notification system
    }

    private IEnumerator AnimateResults()
    {
        isAnimating = true;

        // Reset UI before animating
        ResetAnimatedUI();

        yield return StartCoroutine(AnimateScore());
        yield return StartCoroutine(AnimateTime());
        UpdateStaticElements();
        yield return StartCoroutine(AnimateStars());

        isAnimating = false;
    }

    private void ResetAnimatedUI()
    {
        if (scoreText != null) scoreText.text = "0";
        if (timeText != null) timeText.text = "00:00";
        if (accuracyBar != null) accuracyBar.fillAmount = 0;
        if (starImages != null)
        {
            foreach (var star in starImages)
            {
                if (star != null) star.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator AnimateScore()
    {
        if (scoreText == null) yield break;

        float startScore = 0;
        float targetScore = currentResult.score;
        float elapsed = 0f;

        while (elapsed < scoreCountDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / scoreCountDuration);
            float currentScore = Mathf.Lerp(startScore, targetScore, progress);

            scoreText.text = Mathf.RoundToInt(currentScore).ToString();
            yield return null;
        }

        scoreText.text = targetScore.ToString();
    }

    private IEnumerator AnimateTime()
    {
        if (timeText == null) yield break;

        float startTime = 0;
        float targetTime = currentResult.timeTaken;
        float elapsed = 0f;

        while (elapsed < timeCountDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / timeCountDuration);
            float currentTime = Mathf.Lerp(startTime, targetTime, progress);

            timeText.text = FormatTime(currentTime);
            yield return null;
        }

        timeText.text = currentResult.GetFormattedTime();
    }

    private IEnumerator AnimateStars()
    {
        if (starImages == null || starImages.Length == 0) yield break;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                bool shouldShow = i < currentResult.stars;
                if (shouldShow)
                {
                    starImages[i].gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
    }

    private void UpdateStaticElements()
    {
        if (correctAnswersText != null)
            correctAnswersText.text = currentResult.correctAnswers.ToString();
        if (wrongAnswersText != null)
            wrongAnswersText.text = currentResult.wrongAnswers.ToString();
        if (accuracyText != null)
            accuracyText.text = $"{currentResult.accuracyPercentage:F1}%";
        if (gradeText != null)
            gradeText.text = currentResult.grade;
        if (performanceMessageText != null)
            performanceMessageText.text = currentResult.GetPerformanceMessage();
        if (totalQuestionsText != null)
            totalQuestionsText.text = $"/ {currentResult.totalQuestions}";
        if (stageText != null)
            stageText.text = $"Stage - {currentResult.levelNumber}";
        if (accuracyBar != null)
        {
            float fillAmount = currentResult.accuracyPercentage / 100f;
            accuracyBar.fillAmount = fillAmount;
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public QuizResultData GetCurrentResult()
    {
        return currentResult;
    }

    public bool IsAnimating()
    {
        return isAnimating;
    }
}