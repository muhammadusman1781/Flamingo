using UnityEngine;

// AwaisDev: Binds UIScreen (Quiz) show event to QuizSystemManager grade init
public class QuizScreenBinder : MonoBehaviour
{
    public UIScreen quizScreen;
    public QuizSystemManager quizSystemManager;
    public QuizUIController quizUIController;
    public int overrideGrade = -1; // -1 uses current grade set on QuizSystemManager

    void Reset()
    {
        if (quizScreen == null) quizScreen = GetComponent<UIScreen>();
        if (quizSystemManager == null) quizSystemManager = FindObjectOfType<QuizSystemManager>();
        if (quizUIController == null) quizUIController = FindObjectOfType<QuizUIController>();
    }

    void Awake()
    {
        Debug.Log("QuizScreenBinder: Awake called");
        if (quizScreen == null) quizScreen = GetComponent<UIScreen>();
        if (quizSystemManager == null) quizSystemManager = FindObjectOfType<QuizSystemManager>();
        if (quizUIController == null) quizUIController = FindObjectOfType<QuizUIController>();
        
        Debug.Log($"QuizScreenBinder: quizScreen={quizScreen}, quizSystemManager={quizSystemManager}, quizUIController={quizUIController}");
        
        if (quizScreen != null)
        {
            if (quizScreen.onShown == null) quizScreen.onShown = new UnityEngine.Events.UnityEvent();
            quizScreen.onShown.AddListener(OnQuizScreenShown);
            Debug.Log("QuizScreenBinder: Added listener to onShown event");
        }
        else
        {
            Debug.LogError("QuizScreenBinder: quizScreen is null!");
        }
    }

    private void OnQuizScreenShown()
    {
        Debug.Log("QuizScreenBinder: OnQuizScreenShown called");
        if (quizSystemManager == null) 
        {
            Debug.LogError("QuizScreenBinder: quizSystemManager is null!");
            return;
        }
        
        if (overrideGrade > 0) 
        {
            quizSystemManager.currentGrade = overrideGrade;
            Debug.Log($"QuizScreenBinder: Set override grade to {overrideGrade}");
        }
        
        // Force refresh the data
        quizSystemManager.ForceRefreshData();
        
        // Wait a frame then notify the QuizUIController
        StartCoroutine(NotifyUIControllerAfterDelay());
    }
    
    private System.Collections.IEnumerator NotifyUIControllerAfterDelay()
    {
        yield return null; // Wait one frame
        Debug.Log("QuizScreenBinder: Notifying QuizUIController");
        
        // Also notify the QuizUIController that the screen is shown
        if (quizUIController != null)
        {
            quizUIController.OnQuizScreenShown();
        }
        else
        {
            Debug.LogError("QuizScreenBinder: quizUIController is null!");
        }
    }
}




