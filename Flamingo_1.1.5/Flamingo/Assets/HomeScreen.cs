using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTLTMPro;
public class HomeScreen : MonoBehaviour
{
    public RTLTextMeshPro PlayerName;

    private void OnEnable()
    {
        PlayerName.text = NetworkingHandler.instance.serverConstants.UserProfileData.first_name +" "+ NetworkingHandler.instance.serverConstants.UserProfileData.last_name;
    }
}
