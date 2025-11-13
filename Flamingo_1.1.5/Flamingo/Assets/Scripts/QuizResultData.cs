using System;
using UnityEngine;

/// <summary>
/// Data structure to hold quiz result information
/// </summary>
[System.Serializable]
public class QuizResultData
{
    [Header("Core Metrics")]
    public int levelNumber;
    public int totalQuestions;
    public int correctAnswers;
    public int wrongAnswers;
    public int score;
    public float timeTaken; // in seconds
    
    [Header("Calculated Metrics")]
    public float accuracyPercentage;
    public string grade;
    public int stars;
    public int longestStreak;
    
    [Header("Additional Info")]
    public string category;
    public bool isNewRecord;
    public int previousBestScore;
    
    // Constructor
    public QuizResultData(int total, int correct, int wrong, int finalScore, float time, int lvlNum)
    {
        totalQuestions = total;
        correctAnswers = correct;
        wrongAnswers = wrong;
        score = finalScore;
        timeTaken = time;
        levelNumber = lvlNum;
        CalculateMetrics();
    }
    
    /// <summary>
    /// Calculate derived metrics from core data
    /// </summary>
    public void CalculateMetrics()
    {
        // Calculate accuracy percentage
        accuracyPercentage = totalQuestions > 0 ? (float)correctAnswers / totalQuestions * 100f : 0f;
        
        // Calculate grade based on accuracy
        grade = CalculateGrade(accuracyPercentage);
        
        // Calculate stars (1-5)
        stars = CalculateStars(accuracyPercentage);
        
        // For now, set default values (can be updated when streak tracking is implemented)
        longestStreak = correctAnswers;
        isNewRecord = false;
        previousBestScore = 0;
    }
    
    /// <summary>
    /// Calculate letter grade based on accuracy
    /// </summary>
    private string CalculateGrade(float accuracy)
    {
        if (accuracy >= 95f) return "A+";
        if (accuracy >= 90f) return "A";
        if (accuracy >= 85f) return "A-";
        if (accuracy >= 80f) return "B+";
        if (accuracy >= 75f) return "B";
        if (accuracy >= 70f) return "B-";
        if (accuracy >= 65f) return "C+";
        if (accuracy >= 60f) return "C";
        if (accuracy >= 55f) return "C-";
        if (accuracy >= 50f) return "D";
        return "F";
    }
    
    /// <summary>
    /// Calculate star rating (1-5 stars)
    /// </summary>
    private int CalculateStars(float accuracy)
    {
        if (accuracy >= 90f) return 5;
        if (accuracy >= 80f) return 4;
        if (accuracy >= 70f) return 3;
        if (accuracy >= 60f) return 2;
        return 1;
    }
    
    /// <summary>
    /// Format time as MM:SS
    /// </summary>
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(timeTaken / 60f);
        int seconds = Mathf.FloorToInt(timeTaken % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    
    /// <summary>
    /// Get performance message based on results
    /// </summary>
    public string GetPerformanceMessage()
    {
        if (accuracyPercentage >= 90f) return "Excellent work!";
        if (accuracyPercentage >= 80f) return "Great job!";
        if (accuracyPercentage >= 70f) return "Good effort!";
        if (accuracyPercentage >= 60f) return "Not bad!";
        return "Keep practicing!";
    }
}
