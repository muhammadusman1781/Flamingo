using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    [Header("Text References")]
    public Text targetText;
    [Header("Text Options")]
    public string[] textOptions;
    public int startIndex = 0;
    public Image [] targetImage;
    public GameObject[] Screens;
    void Start()
    {
        // Change to red with full opacity
        targetImage[startIndex].color = new Color(0.878f, 0.337f, 0.141f);
        // Initialize with starting text
        if (targetText != null && textOptions.Length > 0)
        {
            //currentIndex = startIndex % textOptions.Length;
            targetText.text = textOptions[startIndex];
        }
        else
        {
            Debug.LogWarning("Text component or text options not assigned!");
        }

        if (Screens.Length > 0)
        {
            Screens[0].SetActive(true);
            Screens[1].SetActive(false);
            /*for (int i = 0; i < Screens.Length; i++)
                {
                Screens[i].SetActive(false);    
                }*/
            
        }
    }

    public void OnButtonClickChangeText()
    {
        if (targetText == null)
        {
            Debug.LogError("No text component assigned!");
            return;
        }

        if (textOptions.Length == 0)
        {
            Debug.LogError("No text options available!");
            return;
        }

        if (startIndex < textOptions.Length-1)
        {
            targetImage[startIndex].color = Color.white;
            startIndex++;
            targetText.text = textOptions[startIndex];
            targetImage[startIndex].color = new Color(0.878f, 0.337f, 0.141f);
        }
        else
        {
            /*for (int i = 0; i < Screens.Length; i++)
            {
                Screens[i].SetActive(true);    
            }*/
            Screens[0].SetActive(false);
            Screens[1].SetActive(true);
        }
    }

   
}
