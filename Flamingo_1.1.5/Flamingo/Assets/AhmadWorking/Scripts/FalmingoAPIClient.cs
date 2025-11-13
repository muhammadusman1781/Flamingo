using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

[Serializable]
public class UserRegistration
{
    public string email;
    public string password;
}

[Serializable]
public class UserProfile
{
    public string first_name;
    public string last_name;
    public int age;
    public string region;
    public string gender;
}

[Serializable]
public class UserLogin
{
    public string email;
    public string password;
}

[Serializable]
public class RewardsData
{
    public int rewards;
}

[Serializable]
public class PointsData
{
    public int points;
    public string action; // "add" or "subtract"
}

[Serializable]
public class FeathersData
{
    public int feathers;
    public string action; // "add" or "subtract"
}

[Serializable]
public class CoinsData
{
    public int id;
    public string action; // "add" or "subtract"
}

[Serializable]
public class OTPVerify
{
    public string email;
    public string otp;
}

[Serializable]
public class OTPResend
{
    public string email;
}

[Serializable]
public class ForgotPassword
{
    public string email;
}

[Serializable]
public class NewPassword
{
    public string otp;
    public string new_password;
}

[Serializable]
public class FriendRequest
{
    public int receiver_id;
}

public class FalmingoAPIClient : MonoBehaviour
{
    private string baseUrl = "https://flamingoacdmy.com/api/v1"; // Replace with your actual base URL
    private string authToken = "";

    // Call this after successful login to set the token
    public void SetAuthToken(string token)
    {
        authToken = token;
    }

    #region Auth APIs

    public IEnumerator Register(UserRegistration user, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/register/";
        string jsonData = JsonUtility.ToJson(user);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator UpdateUserProfile(UserProfile profile, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/profile/";
        string jsonData = JsonUtility.ToJson(profile);

        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Token {authToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator Login(UserLogin credentials, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/login/";
        string jsonData = JsonUtility.ToJson(credentials);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse the token from response and store it
                var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                if (response != null && response.data != null && !string.IsNullOrEmpty(response.data.token))
                {
                    authToken = response.data.token;
                }
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    [Serializable]
    private class LoginResponse
    {
        public LoginData data;
    }

    [Serializable]
    private class LoginData
    {
        public string token;
    }

    public IEnumerator AddRewards(RewardsData rewards, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/rewards/add/";
        string jsonData = JsonUtility.ToJson(rewards);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Token {authToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator GetUserInfo(Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/user/";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Token {authToken}");
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator UpdatePoints(PointsData points, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/points/update/";
        string jsonData = JsonUtility.ToJson(points);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Token {authToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator UpdateFeathers(FeathersData feathers, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/feathers/update/";
        string jsonData = JsonUtility.ToJson(feathers);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Token {authToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator UpdateCoins(CoinsData coins, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/coins/update/";
        string jsonData = JsonUtility.ToJson(coins);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Token {authToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator GetSpins(Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/spins/";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Token {authToken}");
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator VerifyOTP(OTPVerify otp, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/otp/verify/";
        string jsonData = JsonUtility.ToJson(otp);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator ResendOTP(OTPResend otp, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/otp/resend/";
        string jsonData = JsonUtility.ToJson(otp);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator ForgotPassword(ForgotPassword data, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/forgot/";
        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator ResetPassword(NewPassword data, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/reset/";
        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    #endregion

    #region Friend Request APIs

    public IEnumerator SendFriendRequest(FriendRequest requestData, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/friends/send-request/";
        string jsonData = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Token {authToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator AcceptFriendRequest(int requestId, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/friends/accept-request/{requestId}/";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Token {authToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator RejectFriendRequest(int requestId, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/friends/reject-request/{requestId}/";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Token {authToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator CancelFriendRequest(int requestId, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/friends/cancel-request/{requestId}/";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Token {authToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator RemoveFriend(int friendId, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/friends/remove/{friendId}/";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Token {authToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator GetFriendRequests(Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/friends/requests/";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Token {authToken}");
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator GetFriendsList(Action<bool, string> callback)
    {
        string url = $"{baseUrl}/auth/friends/list/";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Token {authToken}");
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true, request.downloadHandler.text);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator GetUsersList(string query = "", Action<bool, string> callback = null)
    {
        string url = $"{baseUrl}/auth/users/list/";
        if (!string.IsNullOrEmpty(query))
        {
            url += $"?q={UnityWebRequest.EscapeURL(query)}";
        }

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Token {authToken}");
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback?.Invoke(true, request.downloadHandler.text);
            }
            else
            {
                callback?.Invoke(false, request.error);
            }
        }
    }

    #endregion
}