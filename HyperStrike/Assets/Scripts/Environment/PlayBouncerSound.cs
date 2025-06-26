using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayBouncerSound : MonoBehaviour
{
    public AudioClip bouncerSFX;

    private AudioSource myAudioSource;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Ball":
            case "Player":
                myAudioSource.volume = Random.Range(0.8f, 1.0f);
                myAudioSource.pitch = Random.Range(0.8f, 1.0f);

                myAudioSource.PlayOneShot(bouncerSFX);
                break;

            default:
                break;
        }
    }
}