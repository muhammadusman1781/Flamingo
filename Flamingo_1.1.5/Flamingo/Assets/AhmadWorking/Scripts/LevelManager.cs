using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }





    private HashSet<int> completedLevels = new HashSet<int>();
    private bool isInitialized = false;

    // Events for level completion
    public System.Action<int> OnLevelCompleted;
    public System.Action<int> OnLevelClicked;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (isInitialized) return;

        Debug.Log("LevelManager: Initializing level system...");

        // Load completed levels
        LoadCompletedLevels();

        isInitialized = true;
        Debug.Log($"LevelManager: Initialized with {completedLevels.Count} completed levels");
    }

    public void HandleLevelClicked(int levelNumber)
    {
        Debug.Log($"LevelManager: Level {levelNumber} clicked");

        // Notify listeners
        OnLevelClicked?.Invoke(levelNumber);
    }

    public void CompleteLevel(int levelNumber)
    {
        if (completedLevels.Contains(levelNumber)) return;

        completedLevels.Add(levelNumber);
        SaveCompletedLevels();

        // Notify listeners
        OnLevelCompleted?.Invoke(levelNumber);

        Debug.Log($"LevelManager: Level {levelNumber} completed!");
    }

    private void SaveCompletedLevels()
    {
        string completedLevelsString = string.Join(",", completedLevels);
        PlayerPrefs.SetString("CompletedLevels", completedLevelsString);
        PlayerPrefs.Save();
    }

    private void LoadCompletedLevels()
    {
        completedLevels.Clear();
        string savedData = PlayerPrefs.GetString("CompletedLevels", "");

        if (!string.IsNullOrEmpty(savedData))
        {
            string[] levelIds = savedData.Split(',');
            foreach (var id in levelIds)
            {
                if (int.TryParse(id, out int levelNum))
                {
                    completedLevels.Add(levelNum);
                }
            }
        }

        Debug.Log($"LevelManager: Loaded {completedLevels.Count} completed levels");
    }

    public void RefreshLevels()
    {
        LoadCompletedLevels();
    }

    // Public getters
    public bool IsLevelCompleted(int levelNumber)
    {
        return completedLevels.Contains(levelNumber);
    }

    public int GetCompletedLevelCount()
    {
        return completedLevels.Count;
    }

    // Debug methods
    [ContextMenu("Reset All Progress")]
    public void ResetAllProgress()
    {
        completedLevels.Clear();
        PlayerPrefs.DeleteKey("CompletedLevels");
        RefreshLevels();
        Debug.Log("LevelManager: All progress reset");
    }

    [ContextMenu("Complete All Levels")]
    public void CompleteAllLevels()
    {
        // Complete levels 1-250 (matching LevelUIScreen configuration)
        for (int i = 1; i <= 250; i++)
        {
            completedLevels.Add(i);
        }
        SaveCompletedLevels();
        RefreshLevels();
        Debug.Log("LevelManager: All levels completed");
    }
}