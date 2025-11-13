using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizSystemManager : MonoBehaviour
{
    public Grade grade;
    public int currentGrade = 1; // Default to grade 1
    private QuizHandler[] quizHandlers;

    public void Start()
    {
        // Find all QuizHandler components in the scene
        quizHandlers = FindObjectsOfType<QuizHandler>();
        Debug.Log($"QuizSystemManager: Found {quizHandlers.Length} QuizHandler(s)");

        InitializeForCurrentGrade();
    }

    public void InitializeForCurrentGrade()
    {
        if (quizHandlers == null || quizHandlers.Length == 0)
        {
            quizHandlers = FindObjectsOfType<QuizHandler>();
        }

        Grade foundGrade = null;

        foreach (QuizHandler quizHandler in quizHandlers)
        {
            if (quizHandler.gradesQuiz != null && quizHandler.gradesQuiz.Count > 0)
            {
                Debug.Log($"QuizHandler has {quizHandler.gradesQuiz.Count} grades");

                // Search through all grades in this handler
                foreach (Grade g in quizHandler.gradesQuiz)
                {
                    Debug.Log($"Checking grade: {g.grade}, looking for: {currentGrade}");
                    if (g.grade == currentGrade)
                    {
                        foundGrade = g;
                        Debug.Log($"Found matching grade: {g.grade}");
                        break;
                    }
                }

                if (foundGrade != null) break;
            }
            else
            {
                Debug.LogWarning("QuizHandler has no gradesQuiz data");
            }
        }

        if (foundGrade != null)
        {
            grade = foundGrade;
            Debug.Log($"QuizSystemManager: Successfully loaded grade {currentGrade} with {grade.levels?.Count ?? 0} levels");
        }
        else
        {
            Debug.LogError($"QuizSystemManager: No grade found matching {currentGrade}");
            // Try to use first available grade as fallback
            foreach (QuizHandler quizHandler in quizHandlers)
            {
                if (quizHandler.gradesQuiz != null && quizHandler.gradesQuiz.Count > 0)
                {
                    grade = quizHandler.gradesQuiz[0];
                    currentGrade = grade.grade;
                    Debug.LogWarning($"Using fallback grade: {grade.grade}");
                    break;
                }
            }
        }

        if (grade == null)
        {
            Debug.LogError("QuizSystemManager: No grade data available at all!");
        }
    }

	public void ForceRefreshData()
	{
		Debug.Log("QuizSystemManager: ForceRefreshData called");
		InitializeForCurrentGrade();
	}
}