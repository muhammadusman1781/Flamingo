using System;
using UnityEngine;
using System.Collections;

// AwaisDev: Handles quiz scoring and game flow
public class QuizGameManager : MonoBehaviour
{
    [Header("Quiz Settings")]
    public int totalQuestions = 10;
    public int pointsPerCorrectAnswer = 100;
    public int timeBonusMultiplier = 10;

    [Header("References")]
    public QuizResultManager resultManager;

    // Current quiz state
    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;
    private int wrongAnswers = 0;
    private int currentScore = 0;
    private float quizStartTime;
    private float currentTime;
    private bool isQuizActive = false;

    // --- MODIFICATION START ---
    private int currentLevelNumber; // Add this to store the level number
                                    // --- MODIFICATION END ---

    private int currentStreak = 0;
    private int longestStreak = 0;

    // Events
    public static event Action<int, int> OnQuestionAnswered; // correct, wrong
    public static event Action<int> OnScoreUpdated;
    public static event Action<float> OnTimeUpdated;
    public static event Action OnQuizCompleted;


    private void Update()
    {
        if (isQuizActive)
        {
            UpdateTime();
        }
    }

    /// <summary>
    /// Start a new quiz for a specific level.
    /// </summary>
    // --- MODIFICATION START ---
    public void StartQuiz(int levelNumber) // It now accepts the level number
    {
        currentLevelNumber = levelNumber; // And stores it
                                          // --- MODIFICATION END ---

        currentQuestionIndex = 0;
        correctAnswers = 0;
        wrongAnswers = 0;
        currentScore = 0;
        currentStreak = 0;
        longestStreak = 0;
        quizStartTime = Time.time;
        isQuizActive = true;
    }

    public void SubmitAnswer(bool isCorrect)
    {
        if (!isQuizActive) return;

        if (isCorrect)
        {
            correctAnswers++;
            currentStreak++;
            if (currentStreak > longestStreak)
                longestStreak = currentStreak;

            int streakBonus = Mathf.Max(0, currentStreak - 1) * 10;
            int baseScore = pointsPerCorrectAnswer + streakBonus;
            currentScore += baseScore;
        }
        else
        {
            wrongAnswers++;
            currentStreak = 0;
        }

        currentQuestionIndex++;

        OnQuestionAnswered?.Invoke(correctAnswers, wrongAnswers);
        OnScoreUpdated?.Invoke(currentScore);

        if (currentQuestionIndex >= totalQuestions)
        {
            CompleteQuiz();
        }
    }

    private void CompleteQuiz()
    {
        isQuizActive = false;
        currentTime = Time.time - quizStartTime;

        float timeBonus = Mathf.Max(0, (totalQuestions * 30f - currentTime) * timeBonusMultiplier);
        currentScore += Mathf.RoundToInt(timeBonus);

        // --- MODIFICATION START ---
        // Create result data and pass the stored 'currentLevelNumber'. This fixes the error.
        QuizResultData resultData = new QuizResultData(
            totalQuestions,
            correctAnswers,
            wrongAnswers,
            currentScore,
            currentTime,
            currentLevelNumber // The missing argument is now provided
        );
        // --- MODIFICATION END ---

        resultData.longestStreak = longestStreak;
        resultData.category = $"Level {currentLevelNumber}";

        if (resultManager != null)
        {
            resultManager.ShowResult(resultData);
        }

        OnQuizCompleted?.Invoke();
    }

    private void UpdateTime()
    {
        currentTime = Time.time - quizStartTime;
        OnTimeUpdated?.Invoke(currentTime);
    }

    // --- The rest of the script remains the same ---

    private void RestartQuiz()
    {
        if (resultManager != null)
            resultManager.HideResult();

        StartQuiz(currentLevelNumber); // Restart the same level
    }

    private void LoadMainMenu() { /* Your logic here */ }
    private void ShareResult(QuizResultData result) { /* Your logic here */ }
    public float GetProgress() { return totalQuestions > 0 ? (float)currentQuestionIndex / totalQuestions : 0f; }
    public int GetCurrentScore() { return currentScore; }
    public float GetCurrentTime() { return currentTime; }
    public bool IsQuizActive() { return isQuizActive; }
}