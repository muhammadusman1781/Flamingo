using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RTLTMPro;

public class FeatherDetailScreen : MonoBehaviour
{
    [Header("UI References")]
    public Image featherImage; // Large feather image
    public RTLTextMeshPro featherNameText; // Feather name
    public RTLTextMeshPro featherDescriptionText; // Feather description
    public RTLTextMeshPro featherCountText; // Count of this feather
    
    [Header("Buttons")]
    public Button backButton;
    
    private UserFeather currentFeather;
    
    private void Start()
    {
        // Setup back button
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }
    
    public void SetFeatherData(UserFeather feather)
    {
        currentFeather = feather;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (currentFeather == null)
            return;
        
        // Update feather name
        if (featherNameText != null)
            featherNameText.text = currentFeather.feather_type;
        
        // Update feather count
        if (featherCountText != null)
            featherCountText.text = $"x{currentFeather.feather}";
        
        // Update feather image
        if (featherImage != null)
        {
            Sprite featherSprite = GetFeatherSprite(currentFeather.feather_type);
            if (featherSprite != null)
                featherImage.sprite = featherSprite;
        }
        
        // Update feather description
        if (featherDescriptionText != null)
        {
            string description = GetFeatherDescription(currentFeather.feather_type);
            featherDescriptionText.text = description;
        }
    }
    
    private Sprite GetFeatherSprite(string featherType)
    {
        // TODO: Implement sprite loading based on feather type
        // You can use Resources.Load or have a sprite dictionary
        // Example: return Resources.Load<Sprite>($"Feathers/{featherType}");
        return null;
    }
    
    private string GetFeatherDescription(string featherType)
    {
        // Return description based on feather type
        // You can customize these descriptions
        switch (featherType)
        {
            case "Legendary":
                return "أندر الريش وأقواها. يمنح قوة استثنائية في المعارك.";
            
            case "Emerald":
                return "ريش زمردي نادر. يمثل القوة والحكمة.";
            
            case "Ruby":
                return "ريش ياقوتي قوي. يرمز للشجاعة والعزيمة.";
            
            case "Mercury":
                return "ريش زئبقي سريع. يمنح سرعة فائقة في الإجابة.";
            
            case "Diamond":
                return "ريش ألماسي صلب. لا يقهر في المعارك.";
            
            case "Titanium":
                return "ريش تيتانيوم متين. يمنح قوة دفاعية كبيرة.";
            
            case "Platinum":
                return "ريش بلاتيني ثمين. يمثل التميز والنجاح.";
            
            case "Golden":
                return "ريش ذهبي براق. يمنح قوة ومكانة عالية.";
            
            case "Silver":
                return "ريش فضي لامع. يمثل المهارة والإتقان.";
            
            case "Bronze":
                return "ريش برونزي قوي. بداية الطريق نحو النجاح.";
            
            default:
                return "ريش قوي يمنحك ميزة في المعارك.";
        }
    }
    
    private void OnBackButtonClicked()
    {
        // Hide this screen
        gameObject.SetActive(false);
    }
}

