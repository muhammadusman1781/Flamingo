using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
}
[System.Serializable]
public class Sound
{
   public AudioClip btnClickSound;
}