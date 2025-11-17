using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RTLTMPro;

public class FeathersScreen : MonoBehaviour
{
   [Header("UI References")]
    public Button backButton;
    private UserProfileData userProfile;


    public RTLTextMeshPro bronzeFeatherCountText;
    public RTLTextMeshPro silverFeatherCountText;
    public RTLTextMeshPro goldFeatherCountText;
    public RTLTextMeshPro platinumFeatherCountText;
    public RTLTextMeshPro titaniumFeatherCountText;
    public RTLTextMeshPro diamondFeatherCountText;
    public RTLTextMeshPro mercuryFeatherCountText;
    public RTLTextMeshPro rubyFeatherCountText;
    public RTLTextMeshPro emeraldFeatherCountText;
    public RTLTextMeshPro legendaryFeatherCountText;

    public Button bronzeFeatherButton;
    public Button silverFeatherButton;
    public Button goldFeatherButton;
    public Button platinumFeatherButton;
    public Button titaniumFeatherButton;
    public Button diamondFeatherButton;
    public Button mercuryFeatherButton;
    public Button rubyFeatherButton;
    public Button emeraldFeatherButton;
    public Button legendaryFeatherButton;

    public GameObject bronzeFeatherPanel;
    public GameObject silverFeatherPanel;
    public GameObject goldFeatherPanel;
    public GameObject platinumFeatherPanel;
    public GameObject titaniumFeatherPanel;
    public GameObject diamondFeatherPanel;
    public GameObject mercuryFeatherPanel;
    public GameObject rubyFeatherPanel;
    public GameObject emeraldFeatherPanel;
    public GameObject legendaryFeatherPanel;

    private void OnEnable()
    {
        UpdateFeatherCounts();
    }
    
    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        if (bronzeFeatherButton != null)
        {
            bronzeFeatherButton.onClick.AddListener(OnBronzeFeatherButtonClicked);
        }
        if (silverFeatherButton != null)
        {
            silverFeatherButton.onClick.AddListener(OnSilverFeatherButtonClicked);
        }
        if (goldFeatherButton != null)
        {
            goldFeatherButton.onClick.AddListener(OnGoldFeatherButtonClicked);
        }
        if (platinumFeatherButton != null)
        {
            platinumFeatherButton.onClick.AddListener(OnPlatinumFeatherButtonClicked);
        }
        if (titaniumFeatherButton != null)
        {
            titaniumFeatherButton.onClick.AddListener(OnTitaniumFeatherButtonClicked);
        }
        if (diamondFeatherButton != null)
        {
            diamondFeatherButton.onClick.AddListener(OnDiamondFeatherButtonClicked);
        }
        if (mercuryFeatherButton != null)
        {
            mercuryFeatherButton.onClick.AddListener(OnMercuryFeatherButtonClicked);
        }
        if (rubyFeatherButton != null)
        {
            rubyFeatherButton.onClick.AddListener(OnRubyFeatherButtonClicked);
        }
        if (emeraldFeatherButton != null)
        {
            emeraldFeatherButton.onClick.AddListener(OnEmeraldFeatherButtonClicked);
        }
        if (legendaryFeatherButton != null)
        {
            legendaryFeatherButton.onClick.AddListener(OnLegendaryFeatherButtonClicked);
        }

    }

    private void OnBronzeFeatherButtonClicked()
    {
        bronzeFeatherPanel.SetActive(true);
    }
    private void OnSilverFeatherButtonClicked()
    {
        silverFeatherPanel.SetActive(true);
    }
    private void OnGoldFeatherButtonClicked()
    {
        goldFeatherPanel.SetActive(true);
    }
    private void OnPlatinumFeatherButtonClicked()
    {
        platinumFeatherPanel.SetActive(true);
    }
    private void OnTitaniumFeatherButtonClicked()
    {
        titaniumFeatherPanel.SetActive(true);
    }
    private void OnDiamondFeatherButtonClicked()
    {
        diamondFeatherPanel.SetActive(true);
    }
    private void OnMercuryFeatherButtonClicked()
    {
        mercuryFeatherPanel.SetActive(true);
    }
    private void OnRubyFeatherButtonClicked()
    {
        rubyFeatherPanel.SetActive(true);
    }
    private void OnEmeraldFeatherButtonClicked()
    {
        emeraldFeatherPanel.SetActive(true);
    }
    private void OnLegendaryFeatherButtonClicked()
    {
        legendaryFeatherPanel.SetActive(true);
    }
    private void OnBackButtonClicked()
    {
        gameObject.SetActive(false);
    }
    
    public void SetUserProfile(UserProfileData profile)
    {
        userProfile = profile;
        
    }

    private void UpdateFeatherCounts()
    {
        bronzeFeatherCountText.text = userProfile.user_feathers.Find(feather => feather.feather_type == "Bronze")?.feather.ToString() ?? "0";
        silverFeatherCountText.text = userProfile.user_feathers.Find(feather => feather.feather_type == "Silver")?.feather.ToString() ?? "0";
        goldFeatherCountText.text = userProfile.user_feathers.Find(feather => feather.feather_type == "Gold")?.feather.ToString() ?? "0";
        platinumFeatherCountText.text = userProfile.user_feathers.Find(feather => feather.feather_type == "Platinum")?.feather.ToString() ?? "0";
        titaniumFeatherCountText.text = userProfile.user_feathers.Find(feather => feather.feather_type == "Titanium")?.feather.ToString() ?? "0";
        diamondFeatherCountText.text = userProfile.user_feathers.Find(feather => feather.feather_type == "Diamond")?.feather.ToString() ?? "0";
        mercuryFeatherCountText.text = userProfile.user_feathers.Find(feather => feather.feather_type == "Mercury")?.feather.ToString() ?? "0";
        rubyFeatherCountText.text = userProfile.user_feathers.Find(feather => feather.feather_type == "Ruby")?.feather.ToString() ?? "0";
        emeraldFeatherCountText.text = userProfile.user_feathers.Find(feather => feather.feather_type == "Emerald")?.feather.ToString() ?? "0";
        legendaryFeatherCountText.text = userProfile.user_feathers.Find(feather => feather.feather_type == "Legendary")?.feather.ToString() ?? "0";
    }
    
}