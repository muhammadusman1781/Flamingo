using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using RTLTMPro;

// AwaisDev: Main quiz UI controller - handles questions, answers, and level progression
public class QuizUIController : MonoBehaviour
{
    public static QuizUIController Instance { get; private set; }

    [Header("UI References")]
    public RTLTextMeshPro questionText;
    public Button[] answerButtons = new Button[4];
    public RTLTextMeshPro[] answerTexts = new RTLTextMeshPro[4];
    public Text levelText;
    public Text scoreText;
    public Text quizNumberText;
    public Text questionNumberText;

    [Header("Quiz Settings")]
    public int questionsPerLevel = 5;
    public int timePerQuestion = 10;

    [Header("RTL Settings")]
    public bool enableFarsi = true;
    public bool preserveNumbers = true;
    public bool fixTags = true;

    [Header("Notification Settings")]
    public NotificationManager note;

    [Header("Result System")]
    public QuizResultManager resultManager;

    [Header("Level System")]
    public LevelManager levelManager;

    [Header("Help System")]
    public QuizHelpManager helpManager;

    private QuizSystemManager quizManager;
    public Grade currentGrade;
    private Level currentLevel;
    private List<Question> currentQuestions;
    private int currentQuestionIndex = 0;
    public int currentLevelNumber = 1;

    // Public accessors for help system
    public List<Question> CurrentQuestions => currentQuestions;
    public int CurrentQuestionIndex => currentQuestionIndex;
    public Button[] AnswerButtons => answerButtons;
    private int score = 0;
    private int correctAnswers = 0;
    private int wrongAnswers = 0;
    private float levelStartTime;
    private float totalTimeTaken;
    
    // Timer variables
    private float currentQuestionTimer = 0f;
    private bool isQuestionTimerActive = false;

    // Continue-after-loss (once per level)
    private bool continueUsedThisLevel = false;
    private bool awaitingContinueAfterLoss = false;

    // Keep track of current level across screen changes
    private static int persistentLevelNumber = 1;

    public static int PersistentLevelNumber => persistentLevelNumber;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple QuizUIController instances detected, destroying duplicate");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        quizManager = FindObjectOfType<QuizSystemManager>();
        if (quizManager == null)
        {
            Debug.LogError("No QuizSystemManager found in scene");
            return;
        }

        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();

        if (helpManager == null)
            helpManager = FindObjectOfType<QuizHelpManager>();

        if (quizManager.grade == null)
        {
            quizManager.InitializeForCurrentGrade();
        }

        if (questionText == null) questionText = GetComponentInChildren<RTLTextMeshPro>();
        if (answerButtons.Length == 0 || answerButtons[0] == null)
        {
            var buttons = GetComponentsInChildren<Button>();
            for (int i = 0; i < Mathf.Min(4, buttons.Length); i++)
            {
                answerButtons[i] = buttons[i];
            }
        }

        if (answerTexts[0] == null)
        {
            var texts = GetComponentsInChildren<RTLTextMeshPro>();
            int textIndex = 0;
            for (int i = 0; i < texts.Length && textIndex < answerTexts.Length; i++)
            {
                if (texts[i] != questionText && texts[i] != levelText && texts[i] != scoreText)
                {
                    answerTexts[textIndex] = texts[i];
                    textIndex++;
                }
            }
        }

        ConfigureRTLTextComponents();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;
            if (answerButtons[i] != null)
            {
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
            }
        }
    }

    private void ConfigureRTLTextComponents()
    {
        if (questionText != null)
        {
            questionText.Farsi = enableFarsi;
            questionText.PreserveNumbers = preserveNumbers;
            questionText.FixTags = fixTags;
        }

        foreach (var answerText in answerTexts)
        {
            if (answerText != null)
            {
                answerText.Farsi = enableFarsi;
                answerText.PreserveNumbers = preserveNumbers;
                answerText.FixTags = fixTags;
            }
        }
    }

    public void OnQuizScreenShown()
    {
        if (quizManager == null)
        {
            quizManager = FindObjectOfType<QuizSystemManager>();
            if (quizManager == null)
            {
                Debug.LogError("No QuizSystemManager found in scene");
                return;
            }
        }

        if (quizManager.grade == null)
        {
            Debug.LogWarning("Grade not initialized yet, waiting...");
            StartCoroutine(WaitForGradeInitialization());
            return;
        }

        currentGrade = quizManager.grade;
        LoadLevel(persistentLevelNumber);
    }

    private System.Collections.IEnumerator WaitForGradeInitialization()
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (quizManager.grade == null && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (quizManager.grade != null)
        {
            OnQuizScreenShown();
        }
        else
        {
            Debug.LogError("Timeout waiting for grade initialization");
        }
    }

    public void LoadLevel(int levelNumber)
    {
        if (currentGrade == null || currentGrade.levels == null || levelNumber < 1 || levelNumber > currentGrade.levels.Count)
        {
            Debug.LogWarning($"Invalid level {levelNumber}");
            return;
        }

        currentLevelNumber = levelNumber;
        persistentLevelNumber = levelNumber;
        currentLevel = currentGrade.levels[levelNumber - 1];
        currentQuestions = new List<Question>(currentLevel.questions);
        currentQuestionIndex = 0;
        correctAnswers = 0;
        wrongAnswers = 0;
        levelStartTime = Time.time;

        if (helpManager != null)
        {
            helpManager.ResetHelpForNewLevel();
        }

        continueUsedThisLevel = false;
        awaitingContinueAfterLoss = false;

        ShuffleQuestions();
        UpdateLevelUI();
        LoadQuestion();
    }

    private void ShuffleQuestions()
    {
        if (currentQuestions == null) return;
        for (int i = 0; i < currentQuestions.Count; i++)
        {
            var temp = currentQuestions[i];
            int randomIndex = Random.Range(i, currentQuestions.Count);
            currentQuestions[i] = currentQuestions[randomIndex];
            currentQuestions[randomIndex] = temp;
        }
    }

    private void LoadQuestion()
    {
        if (currentQuestions == null || currentQuestionIndex >= currentQuestions.Count)
        {
            return;
        }

        var question = currentQuestions[currentQuestionIndex];

        currentQuestionTimer = timePerQuestion;
        isQuestionTimerActive = true;

        UpdateQuestionNumberUI();
        UpdateTimerUI();

        if (questionText != null)
        {
            questionText.text = question.question;
        }

        for (int i = 0; i < answerTexts.Length && i < question.answer.Count; i++)
        {
            if (answerTexts[i] != null)
            {
                answerTexts[i].text = question.answer[i];
            }
            if (answerButtons[i] != null)
            {
                answerButtons[i].interactable = true;
                answerButtons[i].gameObject.SetActive(true);
            }
        }

        for (int i = question.answer.Count; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        if (helpManager != null)
        {
            helpManager.ResetHelpForNextQuestion();
        }
    }

    private void OnAnswerSelected(int answerIndex)
    {
        if (currentQuestions == null || currentQuestionIndex >= currentQuestions.Count) return;

        isQuestionTimerActive = false;

        var question = currentQuestions[currentQuestionIndex];
        bool isCorrect = false;

        if (answerIndex < question.answer.Count)
        {
            string selectedAnswer = question.answer[answerIndex];
            isCorrect = selectedAnswer == question.rightAnswer;
        }

        foreach (var button in answerButtons)
        {
            if (button != null) button.interactable = false;
        }

        if (answerButtons[answerIndex] != null)
        {
            var colors = answerButtons[answerIndex].colors;
            colors.normalColor = isCorrect ? Color.green : Color.red;
            answerButtons[answerIndex].colors = colors;
        }

        if (isCorrect)
        {
            correctAnswers++;
            score += 10;
        }
        else
        {
            wrongAnswers++;
            
            if (!continueUsedThisLevel && note != null)
            {
                awaitingContinueAfterLoss = true;
                ShowContinueNotification();
                return;
            }
            else if (continueUsedThisLevel)
            {
                ShowFinalResult();
                return;
            }
        }

        UpdateTimerUI();
        Invoke(nameof(NextQuestion), 1.5f);
    }

    private void ShowContinueNotification()
    {
        if (note == null)
        {
            Debug.LogError("QuizUIController: NotificationManager not assigned!");
            ShowFinalResult();
            return;
        }

        List<string> buttons = new List<string> { "Continue Stage", "Result" };
        note.createNotification(
            "",
            "Do you want to continue?",
            2,
            buttons,
            (buttonText) =>
            {
                string normalized = Regex.Replace((buttonText ?? string.Empty), "\\s+", " ")
                                          .Trim()
                                          .ToLowerInvariant();

                if (normalized == "continue stage")
                {
                    note.removeNotification();
                    ShowRewardedAdForContinue();
                }
                else if (normalized == "result")
                {
                    note.removeNotification();
                    ShowFinalResult();
                }
            }
        );

        StartCoroutine(CustomizeContinueNotificationColorsDelayed());
    }

    private System.Collections.IEnumerator CustomizeContinueNotificationColorsDelayed()
    {
        yield return null;
        CustomizeContinueNotificationColors();
    }

    private void CustomizeContinueNotificationColors()
    {
        if (note == null || note.noteSpawned == null)
        {
            return;
        }

        GameObject spawnedNote = note.noteSpawned;
        
        Color backgroundColor = new Color(0.9607843f, 0.8627451f, 0.7568628f, 1f); // #F5DCC1
        
        Image backgroundImage = spawnedNote.GetComponent<Image>();
        if (backgroundImage == null)
        {
            Image[] allImages = spawnedNote.GetComponentsInChildren<Image>(true);
            foreach (Image img in allImages)
            {
                if (img.GetComponent<Button>() == null)
                {
                    backgroundImage = img;
                    break;
                }
            }
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
            Debug.Log($"QuizUIController: Set notification background color to #F5DCC1");
        }
        else
        {
            Debug.LogWarning("QuizUIController: Could not find background image to colorize in notification");
        }

        Color textColor = new Color(0.8666667f, 0.3333333f, 0.2117647f, 1f); // #DD5536
        TMPro.TextMeshProUGUI[] textComponents = spawnedNote.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
        foreach (TMPro.TextMeshProUGUI text in textComponents)
        {
            if (text != null)
            {
                text.color = textColor;
            }
        }
        Debug.Log($"QuizUIController: Set notification message text color to #DD5536");

        Color buttonColor = new Color(0.8666667f, 0.3333333f, 0.2117647f, 1f); // #DD5536
        Button[] buttons = spawnedNote.GetComponentsInChildren<Button>();
        int buttonsColored = 0;
        foreach (Button btn in buttons)
        {
            if (btn != null)
            {
                ColorBlock colors = btn.colors;
                colors.normalColor = buttonColor;
                colors.highlightedColor = new Color(buttonColor.r * 0.9f, buttonColor.g * 0.9f, buttonColor.b * 0.9f, 1f);
                colors.pressedColor = new Color(buttonColor.r * 0.8f, buttonColor.g * 0.8f, buttonColor.b * 0.8f, 1f);
                colors.selectedColor = buttonColor;
                btn.colors = colors;

                Image btnImage = btn.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = buttonColor;
                }
                buttonsColored++;
            }
        }
        Debug.Log($"QuizUIController: Set {buttonsColored} button colors to #DD5536");
    }

    private void ShowRewardedAdForContinue()
    {
        if (ProjectAds.instance == null)
        {
            Debug.LogError("QuizUIController: ProjectAds instance not found!");
            ShowFinalResult();
            return;
        }

        WhiteAdManager adManager = ProjectAds.instance.GetComponent<WhiteAdManager>();
        
        if (adManager == null)
        {
            Debug.LogError("QuizUIController: WhiteAdManager not found!");
            ShowFinalResult();
            return;
        }

        // Setup callbacks
        adManager.OnRewardedAdWatched = () =>
        {
            Debug.Log("QuizUIController: Ad watched, continuing stage");
            ResumeAfterLoss();
            adManager.OnRewardedAdWatched = null;
        };

        adManager.OnRewardedAdFailed = () =>
        {
            Debug.LogError("QuizUIController: Ad failed, cannot continue");
            if (note != null)
            {
                List<string> buttons = new List<string> { "OK" };
                note.createNotification(
                    "",
                    "Ad unavailable. Cannot continue.",
                    1,
                    buttons,
                    (buttonText) =>
                    {
                        note.removeNotification();
                        ShowFinalResult();
                    }
                );
            }
            else
            {
                ShowFinalResult();
            }
            adManager.OnRewardedAdFailed = null;
        };

        // Show ad
        ProjectAds.instance.ShowRewardedAd();
    }

    private void ShowFinalResult()
    {
        totalTimeTaken = Time.time - levelStartTime;
        var resultData = new QuizResultData(
            questionsPerLevel,
            correctAnswers,
            wrongAnswers,
            score,
            totalTimeTaken,
            currentLevelNumber
        );
        resultData.category = $"Level {currentLevelNumber}";
        resultData.longestStreak = correctAnswers;

        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.Show(ScreenId.Results);
        }
        if (resultManager != null)
        {
            resultManager.ShowResult(resultData);
        }
    }

    public void ResumeAfterLoss()
    {
        if (!awaitingContinueAfterLoss || continueUsedThisLevel)
        {
            return;
        }
        continueUsedThisLevel = true;
        awaitingContinueAfterLoss = false;

        ResetButtonColors();
        Invoke(nameof(NextQuestion), 0f);
    }

    private void NextQuestion()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex >= questionsPerLevel || currentQuestionIndex >= currentQuestions.Count)
        {
            CompleteLevel();
        }
        else
        {
            ResetButtonColors();
            LoadQuestion();
        }
    }


    private void CompleteLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CompleteLevel(currentLevelNumber);
        }

        ShowFinalResult();
    }

    private string GetGradeText(float percentage)
    {
        if (percentage >= 90f) return "A+ (Excellent!)";
        if (percentage >= 80f) return "A (Very Good!)";
        if (percentage >= 70f) return "B (Good!)";
        if (percentage >= 60f) return "C (Satisfactory)";
        if (percentage >= 50f) return "D (Needs Improvement)";
        return "F (Try Again)";
    }

    private void ResetButtonColors()
    {
        foreach (var button in answerButtons)
        {
            if (button != null)
            {
                var colors = button.colors;
                colors.normalColor = Color.white;
                button.colors = colors;
            }
        }
    }

    private void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevelNumber}";
        }

        if (quizNumberText != null)
        {
            int quizNumber = currentLevel != null ? currentLevel.levelNumber : currentLevelNumber;
            quizNumberText.text = $"Quiz {quizNumber}";
        }
        UpdateQuestionNumberUI();
    }

    private void UpdateScoreUI()
    {
        // Score UI removed - timer displayed instead
    }

    private void UpdateTimerUI()
    {
        if (scoreText != null)
        {
            int seconds = Mathf.CeilToInt(currentQuestionTimer);
            scoreText.text = seconds.ToString();
        }
    }

    private void UpdateQuestionNumberUI()
    {
        if (questionNumberText != null && currentQuestions != null)
        {
            int currentQuestionNum = currentQuestionIndex + 1;
            int totalQuestions = currentQuestions.Count;
            questionNumberText.text = $"Question {currentQuestionNum}/{totalQuestions}";
        }
    }

    void Update()
    {
        if (isQuestionTimerActive && currentQuestionTimer > 0f)
        {
            currentQuestionTimer -= Time.deltaTime;
            UpdateTimerUI();
            
            if (currentQuestionTimer <= 0f)
            {
                currentQuestionTimer = 0f;
                isQuestionTimerActive = false;
                
                Debug.Log("QuizUIController: Time's up!");
                
                foreach (var button in answerButtons)
                {
                    if (button != null) button.interactable = false;
                }
                
                wrongAnswers++;

                if (!continueUsedThisLevel && note != null)
                {
                    awaitingContinueAfterLoss = true;
                    ShowContinueNotification();
                }
                else if (continueUsedThisLevel)
                {
                    ShowFinalResult();
                }
                else
                {
                    UpdateTimerUI();
                    Invoke(nameof(NextQuestion), 1.5f);
                }
            }
        }
    }
}