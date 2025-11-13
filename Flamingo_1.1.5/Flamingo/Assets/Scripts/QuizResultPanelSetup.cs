using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script to quickly set up the Quiz Result Panel UI
/// Attach this to your result panel GameObject and use the SetupUI() method
/// </summary>
public class QuizResultPanelSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = false;
    
    [Header("UI References (Auto-assigned)")]
    public Text scoreText;
    public Text timeText;
    public Text correctAnswersText;
    public Text wrongAnswersText;
    public Text accuracyText;
    public Text gradeText;
    public Text performanceMessageText;
    public Text totalQuestionsText;
    
    public Image accuracyBar;
    public Image[] starImages = new Image[5];
    
    public Button playAgainButton;
    public Button nextLevelButton;
    public Button mainMenuButton;
    public Button shareButton;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupUI();
        }
    }
    
    /// <summary>
    /// Automatically find and assign UI components
    /// </summary>
    [ContextMenu("Setup UI Components")]
    public void SetupUI()
    {
        // Find all Text components
        Text[] allTexts = GetComponentsInChildren<Text>(true);
        
        // Find all Image components
        Image[] allImages = GetComponentsInChildren<Image>(true);
        
        // Find all Button components
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        
        // Auto-assign based on common naming patterns
        foreach (Text text in allTexts)
        {
            string name = text.name.ToLower();
            if (name.Contains("score")) scoreText = text;
            else if (name.Contains("time")) timeText = text;
            else if (name.Contains("correct")) correctAnswersText = text;
            else if (name.Contains("wrong")) wrongAnswersText = text;
            else if (name.Contains("accuracy")) accuracyText = text;
            else if (name.Contains("grade")) gradeText = text;
            else if (name.Contains("performance") || name.Contains("message")) performanceMessageText = text;
            else if (name.Contains("total") || name.Contains("questions")) totalQuestionsText = text;
        }
        
        // Find accuracy bar (usually a filled image)
        foreach (Image img in allImages)
        {
            if (img.type == Image.Type.Filled && img.name.ToLower().Contains("accuracy"))
            {
                accuracyBar = img;
                break;
            }
        }
        
        // Find star images
        int starIndex = 0;
        foreach (Image img in allImages)
        {
            if (img.name.ToLower().Contains("star") && starIndex < starImages.Length)
            {
                starImages[starIndex] = img;
                starIndex++;
            }
        }
        
        // Find buttons
        foreach (Button btn in allButtons)
        {
            string name = btn.name.ToLower();
            if (name.Contains("play") && name.Contains("again")) playAgainButton = btn;
            else if (name.Contains("next") && name.Contains("level")) nextLevelButton = btn;
            else if (name.Contains("main") && name.Contains("menu")) mainMenuButton = btn;
            else if (name.Contains("share")) shareButton = btn;
        }
        
        Debug.Log("QuizResultPanelSetup: UI components auto-assigned. Please verify in inspector.");
    }
    
    /// <summary>
    /// Create a basic result panel UI structure
    /// </summary>
    [ContextMenu("Create Basic UI Structure")]
    public void CreateBasicUIStructure()
    {
        // This method can be used to programmatically create a basic UI structure
        // For now, it just logs instructions
        Debug.Log("To create a basic UI structure:");
        Debug.Log("1. Create a Canvas");
        Debug.Log("2. Create a Panel as child of Canvas");
        Debug.Log("3. Add Text components for: Score, Time, Correct, Wrong, Accuracy, Grade");
        Debug.Log("4. Add an Image for accuracy bar (set type to Filled)");
        Debug.Log("5. Add 5 Image components for stars");
        Debug.Log("6. Add 4 Button components for actions");
        Debug.Log("7. Use the 'Setup UI Components' context menu to auto-assign");
    }
}
