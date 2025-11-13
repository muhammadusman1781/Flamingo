using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using TMPro;

public class QuizScreenMultiplayer : MonoBehaviour
{
    [Header("References")]
    public GameObject endScreen;

    [Header("Room Settings")]
    public string roomSlug; // Assign this manually before enabling

    [Header("UI Elements")]
    public RTLTextMeshPro questionText;
    public RTLTextMeshPro answerAText;
    public RTLTextMeshPro answerBText;
    public RTLTextMeshPro answerCText;
    public RTLTextMeshPro answerDText;
    public Button answerAButton;
    public Button answerBButton;
    public Button answerCButton;
    public Button answerDButton;
    public Text timerText;
    public Text scoreText; // On end screen

    [Header("Quiz Settings")]
    public float timePerQuestion = 30f; // Time limit for each question

    // Private variables
    public List<QuestionMultiplayer> questions;
    private int currentQuestionIndex = 0;
    public int playerScore = 0;
    private float currentTimer;
    private bool isAnswered = false;
    private bool questionsLoaded = false;

    void Start()
    {
        // Hide end screen initially
        if (endScreen != null)
        {
            endScreen.SetActive(false);
        }

        // Add button listeners
        if (answerAButton != null) answerAButton.onClick.AddListener(() => OnAnswerSelected("A"));
        if (answerBButton != null) answerBButton.onClick.AddListener(() => OnAnswerSelected("B"));
        if (answerCButton != null) answerCButton.onClick.AddListener(() => OnAnswerSelected("C"));
        if (answerDButton != null) answerDButton.onClick.AddListener(() => OnAnswerSelected("D"));

        // Fetch questions from server
        FetchQuestions();
    }

    void Update()
    {
        // Update timer if questions are loaded and quiz is active
        if (questionsLoaded && !isAnswered && questions != null && currentQuestionIndex < questions.Count)
        {
            currentTimer -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = Mathf.Max(0, Mathf.Ceil(currentTimer)).ToString();
            }

            // Time's up, move to next question
            if (currentTimer <= 0)
            {
                OnTimeUp();
            }
        }
    }

    void FetchQuestions()
    {
        if (string.IsNullOrEmpty(roomSlug))
        {
            Debug.LogError("Room slug is not assigned!");
            return;
        }

        string apiUrl = NetworkingHandler.instance.serverConstants.baseUrl + "/multiplayer/rooms/" + roomSlug + "/questions/";
        Debug.Log("Fetching questions from: " + apiUrl);

        NetworkingHandler.instance.getMessage(apiUrl, true, OnQuestionsSuccess, OnQuestionsFail);
    }

    void OnQuestionsSuccess(string response)
    {
        Debug.Log("Questions fetched successfully: " + response);

        try
        {
            QuestionsResponse questionsResponse = JsonUtility.FromJson<QuestionsResponse>(response);

            if (questionsResponse.status == "success" && questionsResponse.data != null)
            {
                questions = questionsResponse.data.questions;
                questionsLoaded = true;

                Debug.Log("Loaded " + questions.Count + " questions");

                // Start displaying questions
                if (questions.Count > 0)
                {
                    DisplayCurrentQuestion();
                }
                else
                {
                    Debug.LogError("No questions available!");
                }
            }
            else
            {
                Debug.LogError("Failed to parse questions response");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing questions: " + e.Message);
        }
    }

    void OnQuestionsFail(string error)
    {
        Debug.LogError("Failed to fetch questions: " + error);
    }

    void DisplayCurrentQuestion()
    {
        if (questions == null || currentQuestionIndex >= questions.Count)
        {
            ShowEndScreen();
            return;
        }

        QuestionMultiplayer currentQuestion = questions[currentQuestionIndex];

        // Reset state
        isAnswered = false;
        currentTimer = timePerQuestion;

        // Enable answer buttons
        SetButtonsInteractable(true);

        // Display question and answers
        if (questionText != null)
        {
            questionText.text = currentQuestion.question;
        }

        if (answerAText != null)
        {
            answerAText.text = currentQuestion.option_a;
        }

        if (answerBText != null)
        {
            answerBText.text = currentQuestion.option_b;
        }

        if (answerCText != null)
        {
            answerCText.text = currentQuestion.option_c;
        }

        if (answerDText != null)
        {
            answerDText.text = currentQuestion.option_d;
        }

        Debug.Log("Displaying question " + (currentQuestionIndex + 1) + " of " + questions.Count);
    }

    void OnAnswerSelected(string selectedAnswer)
    {
        if (isAnswered) return;

        isAnswered = true;

        // Disable buttons to prevent multiple clicks
        SetButtonsInteractable(false);

        QuestionMultiplayer currentQuestion = questions[currentQuestionIndex];

        // Get the selected answer text based on which button was clicked
        string selectedAnswerText = "";
        if (selectedAnswer == "A") selectedAnswerText = currentQuestion.option_a;
        else if (selectedAnswer == "B") selectedAnswerText = currentQuestion.option_b;
        else if (selectedAnswer == "C") selectedAnswerText = currentQuestion.option_c;
        else if (selectedAnswer == "D") selectedAnswerText = currentQuestion.option_d;

        // Debug logs
        Debug.Log("=== ANSWER SELECTED ===");
        Debug.Log("Selected Button: " + selectedAnswer);
        Debug.Log("Selected Answer Text: " + selectedAnswerText);
        Debug.Log("Correct Answer: " + currentQuestion.right_answer);

        // Check if answer is correct by comparing the answer text
        if (selectedAnswerText == currentQuestion.right_answer)
        {
            playerScore++;
            Debug.Log("✓ CORRECT! Score: " + playerScore + " / " + questions.Count);
        }
        else
        {
            Debug.Log("✗ WRONG! Score remains: " + playerScore + " / " + questions.Count);
        }
        Debug.Log("======================");

        // Move to next question after a short delay
        StartCoroutine(MoveToNextQuestionAfterDelay(1.5f));
    }

    void OnTimeUp()
    {
        if (isAnswered) return;

        isAnswered = true;
        Debug.Log("Time's up! No points awarded.");

        // Disable buttons
        SetButtonsInteractable(false);

        // Move to next question after a short delay
        StartCoroutine(MoveToNextQuestionAfterDelay(1.5f));
    }

    IEnumerator MoveToNextQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Count)
        {
            DisplayCurrentQuestion();
        }
        else
        {
            ShowEndScreen();
        }
    }

    void ShowEndScreen()
    {
        Debug.Log("Quiz completed! Final score: " + playerScore + " / " + questions.Count);

        // Hide quiz UI
        gameObject.SetActive(false);

        // Show end screen
        if (endScreen != null)
        {
            endScreen.SetActive(true);

            // Display score on end screen
            if (scoreText != null)
            {
                scoreText.text = playerScore + " / " + questions.Count;
            }
        }
    }

    void SetButtonsInteractable(bool interactable)
    {
        if (answerAButton != null) answerAButton.interactable = interactable;
        if (answerBButton != null) answerBButton.interactable = interactable;
        if (answerCButton != null) answerCButton.interactable = interactable;
        if (answerDButton != null) answerDButton.interactable = interactable;
    }

    void OnDestroy()
    {
        // Clean up button listeners
        if (answerAButton != null) answerAButton.onClick.RemoveAllListeners();
        if (answerBButton != null) answerBButton.onClick.RemoveAllListeners();
        if (answerCButton != null) answerCButton.onClick.RemoveAllListeners();
        if (answerDButton != null) answerDButton.onClick.RemoveAllListeners();
    }
}
