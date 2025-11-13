using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkingHandler : MonoBehaviour
{
    public static NetworkingHandler instance;
    [Header("-----ServerConstants-----")]
    public ServerConstants serverConstants;
    [Header("Networking Options")]
    public bool allowInsecureCertificates = false;
    public int requestTimeoutSeconds = 40;
    public int retryCount = 1;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        yield return null;
    }

    public void postMessage(string apiUrl, string jsonToSend, bool isTokenNeeded, Action<string> onSuccess = null, Action<string> onFail = null)
    {
        StartCoroutine(PostAPIMessage(apiUrl, jsonToSend, isTokenNeeded, onSuccess, onFail));
    }

    public void putAPIMessage(string apiUrl, string jsonToSend, bool isTokenNeeded, Action<string> onSuccess = null, Action<string> onFail = null)
    {
        StartCoroutine(PUTAPIMessage(apiUrl, jsonToSend, isTokenNeeded, onSuccess, onFail));
    }

    public void getMessage(string apiUrl, bool isTokenNeeded, Action<string> onSuccess = null, Action<string> onFail = null)
    {
        StartCoroutine(GetAPIMessage(apiUrl, isTokenNeeded, onSuccess, onFail));
    }

    private IEnumerator PostAPIMessage(string apiUrl, string jsonToSend, bool isTokenNeeded, Action<string> onSuccess = null, Action<string> onFail = null)
    {
        Debug.Log($"Posting to: {apiUrl}");

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("No internet connectivity");
            onFail?.Invoke("No internet connectivity");
            yield break;
        }

        int attempts = 0;
        while (attempts <= retryCount)
        {
            var uwr = new UnityWebRequest(apiUrl, "POST");
            Debug.Log($"Making POST request to: {apiUrl}");

            byte[] jsonEncrypted = new UTF8Encoding().GetBytes(jsonToSend);
            uwr.uploadHandler = new UploadHandlerRaw(jsonEncrypted);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.timeout = Mathf.Max(5, requestTimeoutSeconds);

            if (allowInsecureCertificates)
            {
                uwr.certificateHandler = new InsecureCertificateHandler();
            }

            uwr.redirectLimit = 4;
            uwr.SetRequestHeader("User-Agent", "UnityPlayer");

            if (isTokenNeeded)
            {
                uwr.SetRequestHeader("Authorization", $"token {serverConstants.UserProfileData.token}");
            }

            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("accept", "application/json");

            yield return uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
#else
            if (uwr.isNetworkError || uwr.isHttpError)
#endif
            {
                Debug.LogError($"HTTP {(long)uwr.responseCode} POST {apiUrl} -> {uwr.error} | Body: {uwr.downloadHandler.text}");
                if (attempts < retryCount)
                {
                    attempts++;
                    yield return new WaitForSeconds(0.75f * attempts);
                    continue;
                }
                onFail?.Invoke(uwr.downloadHandler.text);
            }
            else
            {
                Debug.Log($"HTTP {(long)uwr.responseCode} POST {apiUrl} | Body: {uwr.downloadHandler.text}");
                onSuccess?.Invoke(uwr.downloadHandler.text);
            }
            break;
        }
    }

    private IEnumerator PUTAPIMessage(string apiUrl, string jsonToSend, bool isTokenNeeded, Action<string> onSuccess = null, Action<string> onFail = null)
    {
        int attempts = 0;
        while (attempts <= retryCount)
        {
            var uwr = new UnityWebRequest(apiUrl, "PUT");
            byte[] jsonEncrypted = new UTF8Encoding().GetBytes(jsonToSend);
            uwr.uploadHandler = new UploadHandlerRaw(jsonEncrypted);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.timeout = Mathf.Max(5, requestTimeoutSeconds);

            if (allowInsecureCertificates)
            {
                uwr.certificateHandler = new InsecureCertificateHandler();
            }

            uwr.redirectLimit = 4;
            uwr.SetRequestHeader("User-Agent", "UnityPlayer");

            if (isTokenNeeded)
            {
                uwr.SetRequestHeader("Authorization", $"token {serverConstants.UserProfileData.token}");
            }

            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("accept", "application/json");

            yield return uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
#else
            if (uwr.isNetworkError || uwr.isHttpError)
#endif
            {
                Debug.LogError($"HTTP {(long)uwr.responseCode} PUT {apiUrl} -> {uwr.error} | Body: {uwr.downloadHandler.text}");
                if (attempts < retryCount)
                {
                    attempts++;
                    yield return new WaitForSeconds(0.75f * attempts);
                    continue;
                }
                onFail?.Invoke(uwr.downloadHandler.text);
            }
            else
            {
                Debug.Log($"HTTP {(long)uwr.responseCode} PUT {apiUrl} | Body: {uwr.downloadHandler.text}");
                onSuccess?.Invoke(uwr.downloadHandler.text);
            }
            break;
        }
    }

    private IEnumerator GetAPIMessage(string apiUrl, bool isTokenNeeded, Action<string> onSuccess = null, Action<string> onFail = null)
    {
        int attempts = 0;
        while (attempts <= retryCount)
        {
            var uwr = new UnityWebRequest(apiUrl, "GET");
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.timeout = Mathf.Max(5, requestTimeoutSeconds);

            if (allowInsecureCertificates)
            {
                uwr.certificateHandler = new InsecureCertificateHandler();
            }

            uwr.redirectLimit = 4;
            uwr.SetRequestHeader("User-Agent", "UnityPlayer");

            if (isTokenNeeded)
            {
                uwr.SetRequestHeader("Authorization", $"token {serverConstants.UserProfileData.token}");
            }

            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("accept", "application/json");

            yield return uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
#else
            if (uwr.isNetworkError || uwr.isHttpError)
#endif
            {
                Debug.LogError($"HTTP {(long)uwr.responseCode} GET {apiUrl} -> {uwr.error} | Body: {uwr.downloadHandler.text}");
                if (attempts < retryCount)
                {
                    attempts++;
                    yield return new WaitForSeconds(0.75f * attempts);
                    continue;
                }
                onFail?.Invoke(uwr.downloadHandler.text);
            }
            else
            {
                Debug.Log($"HTTP {(long)uwr.responseCode} GET {apiUrl} | Body: {uwr.downloadHandler.text}");
                onSuccess?.Invoke(uwr.downloadHandler.text);
            }
            break;
        }
    }

    // Accept all certificates (TESTING ONLY). Do NOT enable in production.
    private class InsecureCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}