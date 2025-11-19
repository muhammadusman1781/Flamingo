using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    public static SoundsManager Instance;
    public AudioSource SoundsSource;

    public AudioClip AnserTrueSound;
    public AudioClip AnserFalseSound;
    public AudioClip LevelCompleteSound;
    public AudioClip LevelFailedSound;
    public AudioClip TimeSound;
    public AudioClip CoinsWinSound;
    public AudioClip BattleStartSound;
    public AudioClip FortuneWheelSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public void PlayAnswerTrueSound()
    {
        SoundsSource.PlayOneShot(AnserTrueSound);
    }
    public void PlayAnswerFalseSound()
    {
        SoundsSource.PlayOneShot(AnserFalseSound);
    }
    public void PlayLevelCompleteSound()
    {
        SoundsSource.PlayOneShot(LevelCompleteSound);
    }
    public void PlayLevelFailedSound()
    {
        SoundsSource.PlayOneShot(LevelFailedSound);
    }
    public void PlayTimeSound()
    {
        SoundsSource.PlayOneShot(TimeSound);
    }
    public void PlayCoinsWinSound()
    {
        SoundsSource.PlayOneShot(CoinsWinSound);
    }
    public void PlayBattleStartSound()
    {
        SoundsSource.PlayOneShot(BattleStartSound);
    }
    public void PlayFortuneWheelSound()
    {
        SoundsSource.PlayOneShot(FortuneWheelSound);
    }
}
