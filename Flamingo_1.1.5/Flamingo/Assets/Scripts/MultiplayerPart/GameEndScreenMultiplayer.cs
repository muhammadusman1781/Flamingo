using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEndScreenMultiplayer : MonoBehaviour
{
    [Header("References")]
    public QuizScreenMultiplayer quizScreen;

    [Header("UI GameObjects")]
    public GameObject waitingForOtherPlayerGameObject;
    public GameObject winGameObject;
    public GameObject loseGameObject;

    [Header("Buttons")]
    public Button continueButton; // On win screen
    public Button restartButton; // On lose screen

    // Private variables
    private string roomSlug;
    private int playerScore;
    private int timeTaken;
    private bool isWaitingForResults = true;
    private Coroutine checkResultsCoroutine;

    void OnEnable()
    {
        // Get the slug, score, and time from quiz screen
        if (quizScreen != null)
        {
            roomSlug = quizScreen.roomSlug;
            playerScore = quizScreen.playerScore;
            timeTaken = quizScreen.totalTimeTaken;
        }

        // Setup button listeners
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        // Show only waiting screen initially
        ShowWaitingScreen();

        // Submit score and start checking for results
        SubmitScore();
    }

    void SubmitScore()
    {
        if (string.IsNullOrEmpty(roomSlug))
        {
            Debug.LogError("Room slug is not assigned!");
            return;
        }

        string apiUrl = NetworkingHandler.instance.serverConstants.baseUrl + "/multiplayer/rooms/" + roomSlug + "/submit-score/";
        
        SubmitScoreRequest request = new SubmitScoreRequest();
        request.score = playerScore;
        request.time = timeTaken;
        string jsonData = JsonUtility.ToJson(request);

        Debug.Log("Submitting score: " + playerScore + " and time: " + timeTaken + " seconds to " + apiUrl);

        NetworkingHandler.instance.postMessage(
            apiUrl,
            jsonData,
            true, // Token needed
            OnScoreSubmitted,
            OnScoreSubmitFailed
        );
    }

    void OnScoreSubmitted(string response)
    {
        Debug.Log("Score submitted successfully: " + response);

        // Start checking for results
        if (checkResultsCoroutine != null)
        {
            StopCoroutine(checkResultsCoroutine);
        }
        checkResultsCoroutine = StartCoroutine(CheckResultsRoutine());
    }

    void OnScoreSubmitFailed(string error)
    {
        Debug.LogError("Failed to submit score: " + error);
        
        // Still try to check results even if submission failed
        if (checkResultsCoroutine != null)
        {
            StopCoroutine(checkResultsCoroutine);
        }
        checkResultsCoroutine = StartCoroutine(CheckResultsRoutine());
    }

    IEnumerator CheckResultsRoutine()
    {
        isWaitingForResults = true;
        ShowWaitingScreen();

        while (isWaitingForResults)
        {
            FetchResults();
            yield return new WaitForSeconds(1f); // Check every second
        }
    }

    void FetchResults()
    {
        if (string.IsNullOrEmpty(roomSlug))
        {
            Debug.LogError("Room slug is not assigned!");
            return;
        }

        string apiUrl = NetworkingHandler.instance.serverConstants.baseUrl + "/multiplayer/rooms/" + roomSlug + "/results/";
        
        NetworkingHandler.instance.getMessage(
            apiUrl,
            true, // Token needed
            OnResultsSuccess,
            OnResultsFailed
        );
    }

    void OnResultsSuccess(string response)
    {
        Debug.Log("Results fetched: " + response);

        try
        {
            GameResultsResponse resultsResponse = JsonUtility.FromJson<GameResultsResponse>(response);

            if (resultsResponse.status == "success" && resultsResponse.data != null)
            {
                GameResultsData data = resultsResponse.data;

                Debug.Log("Both scores submitted: " + data.both_scores_submitted);

                if (data.both_scores_submitted)
                {
                    // Stop checking for results
                    isWaitingForResults = false;
                    if (checkResultsCoroutine != null)
                    {
                        StopCoroutine(checkResultsCoroutine);
                        checkResultsCoroutine = null;
                    }

                    // Display results
                    DisplayResults(data);
                }
                else
                {
                    Debug.Log("Still waiting for opponent...");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing results: " + e.Message);
        }
    }

    void OnResultsFailed(string error)
    {
        Debug.LogError("Failed to fetch results: " + error);
    }

    void DisplayResults(GameResultsData data)
    {
        Debug.Log("=== GAME RESULTS ===");
        Debug.Log("User: " + data.user_name + " - Score: " + data.user_score);
        Debug.Log("Opponent: " + data.opponent_name + " - Score: " + data.opponent_score);
        Debug.Log("Result: " + data.result);
        Debug.Log("Winner: " + data.winner_name);
        Debug.Log("===================");

        // Track achievements based on result
        if (AchievementTracker.Instance != null)
        {
            if (data.result == "win")
            {
                AchievementTracker.Instance.OnMultiplayerWin();
            }
            else if (data.result == "lose")
            {
                AchievementTracker.Instance.OnMultiplayerLoss();
            }
            // Tie doesn't affect win streak
        }

        // Show appropriate screen based on result
        if (data.result == "win")
        {
            ShowWinScreen();
        }
        else if (data.result == "lose")
        {
            ShowLoseScreen();
        }
        else if (data.result == "tie")
        {
            // For tie, show win screen (both players continue)
            ShowWinScreen();
        }
        else
        {
            // Default to lose screen for unknown results
            ShowLoseScreen();
        }
    }

    void ShowWaitingScreen()
    {
        if (waitingForOtherPlayerGameObject != null)
            waitingForOtherPlayerGameObject.SetActive(true);
        
        if (winGameObject != null)
            winGameObject.SetActive(false);
        
        if (loseGameObject != null)
            loseGameObject.SetActive(false);
    }

    void ShowWinScreen()
    {
        if (waitingForOtherPlayerGameObject != null)
            waitingForOtherPlayerGameObject.SetActive(false);
        
        if (winGameObject != null)
            winGameObject.SetActive(true);
        
        if (loseGameObject != null)
            loseGameObject.SetActive(false);
    }

    void ShowLoseScreen()
    {
        if (waitingForOtherPlayerGameObject != null)
            waitingForOtherPlayerGameObject.SetActive(false);
        
        if (winGameObject != null)
            winGameObject.SetActive(false);
        
        if (loseGameObject != null)
            loseGameObject.SetActive(true);
    }

    void OnContinueClicked()
    {
        Debug.Log("Continue button clicked - Going to home screen");
        GoToHomeScreen();
    }

    void OnRestartClicked()
    {
        Debug.Log("Restart button clicked - Going to home screen");
        GoToHomeScreen();
    }

    void GoToHomeScreen()
    {
        // Find UIScreensManager and switch to home/multiplayer games screen
        UIScreensManager uiManager = FindObjectOfType<UIScreensManager>();
        if (uiManager != null)
        {
            uiManager.SwitchToMultiplayerGamesScreen();
        }
        else
        {
            Debug.LogError("UIScreensManager not found!");
        }
    }

    void OnDisable()
    {
        // Stop checking for results when disabled
        if (checkResultsCoroutine != null)
        {
            StopCoroutine(checkResultsCoroutine);
            checkResultsCoroutine = null;
        }
        isWaitingForResults = false;

        // Clean up button listeners
        if (continueButton != null)
            continueButton.onClick.RemoveAllListeners();
        
        if (restartButton != null)
            restartButton.onClick.RemoveAllListeners();
    }
}
