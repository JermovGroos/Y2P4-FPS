using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISounds : MonoBehaviour
{
    public AudioSource audio;
    public AudioClip basicSound;

    public void PlayAudioClip(AudioClip clip)
    {
        audio.PlayOneShot(clip, 1);
    }

    public void playStandardUISound()
    {
        audio.PlayOneShot(basicSound, 1);
    }
}
