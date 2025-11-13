using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizHandler : MonoBehaviour
{
    public List<Grade> gradesQuiz;
    [Header("QuizUI Items")]
    public GameObject quizUI;
    public GameObject contentTransform;
    public GameObject stageItem;
    public void SetUpQuizAccordingToGrade()
    {
        GameObject stgItm = Instantiate(stageItem, Vector3.zero, Quaternion.identity);
        stgItm.transform.SetParent(contentTransform.transform);
        stgItm.transform.position = stageItem.transform.position;
        stgItm.transform.rotation = stageItem.transform.rotation;
        stgItm.transform.localScale = stageItem.transform.localScale;
    }
}

[System.Serializable]
public class Grade
{
    public int grade;
    public List<Level> levels;
}
[System.Serializable]
public class Level
{
    public int levelNumber;
    public List<Question> questions;
}

[System.Serializable]
public class Question
{
    public string question;
    public List<string> answer;
    public string rightAnswer;
}