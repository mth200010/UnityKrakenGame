using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip _startingSong;

    public static AudioManager Instance = null;
    AudioSource audioSource;
   
    

    private void Awake()
    {
        #region Singleton Pattern (Simple)
        if (Instance == null)
        {
            // doesn't exist yet, this is now our singleton
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // fill references
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
        #endregion
    }


    public void Start()
    {
        if (_startingSong != null)
        {
            AudioManager.Instance.PlaySong(_startingSong);
        }       
            
    }
    
    private void PlaySong(AudioClip clip)
    {
        clip = _startingSong;
        audioSource.Play();
    }
    
}
