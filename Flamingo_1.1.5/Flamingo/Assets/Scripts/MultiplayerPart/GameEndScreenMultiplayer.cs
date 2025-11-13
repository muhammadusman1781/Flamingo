using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEndScreenMultiplayer : MonoBehaviour
{
    [Header("References")]
    public QuizScreenMultiplayer quizScreen;

    [Header("UI Elements")]
    public Text waitingText;
    public Text winnerNameText;
    public Text resultText; // "You Win" or "You Lose"

    // Private variables
    private string roomSlug;
    private int playerScore;
    private bool isWaitingForResults = true;
    private Coroutine checkResultsCoroutine;

    void OnEnable()
    {
        // Get the slug and score from quiz screen
        if (quizScreen != null)
        {
            roomSlug = quizScreen.roomSlug;
            playerScore = quizScreen.playerScore;
        }

        // Reset UI
        if (waitingText != null) waitingText.gameObject.SetActive(true);
        if (winnerNameText != null) winnerNameText.gameObject.SetActive(false);
        if (resultText != null) resultText.gameObject.SetActive(false);

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
        string jsonData = JsonUtility.ToJson(request);

        Debug.Log("Submitting score: " + playerScore + " to " + apiUrl);

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

        if (waitingText != null)
        {
            waitingText.text = "Waiting for other player to finish...";
            waitingText.gameObject.SetActive(true);
        }

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

        // Hide waiting text
        if (waitingText != null)
        {
            waitingText.gameObject.SetActive(false);
        }

        // Show winner name
        if (winnerNameText != null)
        {
            if(data.result == "win")
            {
                winnerNameText.text = "Winner: " + data.user_name;
            }
            else if(data.result == "lose")
            {
                winnerNameText.text = "Winner: " + data.opponent_name;
            }
            winnerNameText.gameObject.SetActive(true);
        }

        // Show result (win/lose/tie)
        if (resultText != null)
        {
            if (data.result == "win")
            {
                resultText.text = "You Win!";
            }
            else if (data.result == "lose")
            {
                resultText.text = "You Lose!";
            }
            else if (data.result == "tie")
            {
                resultText.text = "It's a Tie!";
            }
            else
            {
                resultText.text = "Game Over";
            }
            resultText.gameObject.SetActive(true);
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
    }
}
