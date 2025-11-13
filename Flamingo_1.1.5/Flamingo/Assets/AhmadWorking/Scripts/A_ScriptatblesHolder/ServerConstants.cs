using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ServerConstants", menuName = "AhmadWorking/Server/ServerConstants")]
public class ServerConstants : ScriptableObject
{
    [Header("Auth APIs")]
    public string baseUrl;
    // Please update this with the correct login endpoint from your API documentation.
    public string loginApiURL;
    public string registerApiURL;
    public string verifyOTPURL;
    public string otpResendURL;
    public string updateProfileURL;
    [Header("Spin APIs")]
    public string spinApiURL;
    #region userData

    [Header("User Profile")] public userProfileData UserProfileData;

    #endregion

    [Header("Config Version")]
    public string version = "1.0";
}

[System.Serializable]
public class userProfileData
{
    public string token;
    public string email;
    public string first_name;
    public string last_name;
    public int age;
    public string region;
    public string gender;
    public int[] rewards;
    public int points;
}
