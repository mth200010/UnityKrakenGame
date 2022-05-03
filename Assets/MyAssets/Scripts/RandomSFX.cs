using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSFX : MonoBehaviour
{
    AudioSource randomSFX;
    public AudioClip[] clips;

    public void startRandomSFX()
    {
        Debug.Log("activated");
        CallAudio();
    }

    void CallAudio()
    {
        Invoke("random", 10);       
    }

    void random()
    {
        Debug.Log("play");
        randomSFX.clip = clips[Random.Range(0, clips.Length)];
        audioHelper.PlayClip2D(randomSFX.clip, 1);
        CallAudio();
    }
}
