using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audioHelper : MonoBehaviour
{
    // Start is called before the first frame update
    public static AudioSource PlayClip2D(AudioClip clip, float volume)
    {
        // create our new AudioSource
        GameObject audioObject = new GameObject("2DAudio");
        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        // configure to be 2D
        audioSource.clip = clip;
        audioSource.volume = volume;

        audioSource.Play();
        //destroy when done
        Object.Destroy(audioObject, clip.length);
        //return it
        return audioSource;
    }
}
