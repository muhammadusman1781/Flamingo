using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// AwaisDev: Manages the stage selection UI, populating it with levels using object pooling.
public class LevelUIScreen : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int totalLevels = 250;

    [Header("Object Pooling")]
    [SerializeField] private GameObject levelItemPrefab;
    [SerializeField] private Transform contentParent; // The 'Content' object of your Scroll View

    private List<GameObject> pooledObjects = new List<GameObject>();

    void Start()
    {
        StartCoroutine(PopulateLevels());
    }

    private IEnumerator PopulateLevels()
    {
        while (LevelManager.Instance == null)
        {
            yield return null;
        }

        for (int i = 0; i < totalLevels; i++)
        {
            int levelNumber = i + 1;
            GameObject levelItemObj = GetPooledObject();
            levelItemObj.transform.SetParent(contentParent, false);

            LevelItem levelItem = levelItemObj.GetComponent<LevelItem>();
            if (levelItem != null)
            {
                bool isCompleted = LevelManager.Instance.IsLevelCompleted(levelNumber);
                levelItem.Setup(levelNumber, isCompleted, OnLevelSelected);
            }
            levelItemObj.SetActive(true);

            // Optional: Wait for the next frame every 20 items to prevent stuttering on load
            if (i % 20 == 0)
            {
                yield return null;
            }
        }
    }

    private GameObject GetPooledObject()
    {
        // Simple pooling: find an inactive object, otherwise create a new one.
        foreach (var obj in pooledObjects)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }

        GameObject newObj = Instantiate(levelItemPrefab, contentParent);
        pooledObjects.Add(newObj);
        return newObj;
    }

    private void OnLevelSelected(int levelNumber)
    {
        Debug.Log($"Level {levelNumber} selected.");

        // Load the level in the quiz controller
        if (QuizUIController.Instance != null)
        {
            QuizUIController.Instance.LoadLevel(levelNumber);
        }

        // Switch to the Quiz screen
        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.Show(ScreenId.Quiz);
        }
    }
}