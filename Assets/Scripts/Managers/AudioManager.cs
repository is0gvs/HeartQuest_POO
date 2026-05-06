using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public List<AudioClip> clipList = new List<AudioClip>();
    public AudioSource audioSource;
    [HideInInspector]
    public static AudioManager instance;
    void Awake()
    {
        instance = this;
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void takingDamage()
    {
        if (audioSource != null && clipList.Count > 0 && clipList[0] != null)
        {
            audioSource.clip = clipList[0];
            audioSource.Play();
        }
    }

    public void Hovering()
    {
        if (audioSource != null && clipList.Count > 1 && clipList[1] != null)
        {
            audioSource.clip = clipList[1];
            audioSource.Play();
        }
    }

    public void Selecting()
    {
        if (audioSource != null && clipList.Count > 2 && clipList[2] != null)
        {
            audioSource.clip = clipList[2];
            audioSource.Play();
        }
    }

    public void Slashing()
    {
        if (audioSource != null && clipList.Count > 3 && clipList[3] != null)
        {
            audioSource.clip = clipList[3];
            audioSource.Play();
        }
    }
}
