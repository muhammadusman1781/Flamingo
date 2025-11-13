using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthAPIBodyConstants
{

}

#region OTPSENDINGBOdies

public class onlyEmailBody
{
    public string email;

}
public class verifyOTPPostingBody
{
    public string email;
    public string otp;

}
#endregion

#region LoginAPIBODIES
public class LoginPostBody
{
    public string email;
    public string password;
}

// New class for registration payload
public class RegisterPostBody
{
    public string email;
    public string password;
}

[System.Serializable]
public class LoginApiResponse
{
    public string status;
    public string message;
    public Data data;
}

[System.Serializable]
public class Data
{
    public string token;
    public User user;
}

[System.Serializable]
public class User
{
    public int id;
    public string email;
    public string mobile;
    public string first_name;
    public string last_name;
    public int[] rewards;
    public int points;
}



#endregion

#region UpdateProfileAPIBODIEs

public class UpdateProfilePostBody
{
    public string first_name;
    public string last_name;
    public int age;
    public string region;
    public string gender;
}

#endregion
