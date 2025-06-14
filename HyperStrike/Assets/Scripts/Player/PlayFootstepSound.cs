using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayFootstepSound : MonoBehaviour
{
    public AudioClip[] footstepsOnGrass;
    public AudioClip[] footstepsOnMetal;
    public AudioClip[] footstepsOnTile;

    private AudioSource myAudioSource;
    private string material;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    void WhenPlayingFootstepSound()
    {
        myAudioSource.volume = Random.Range(0.8f, 1.0f);
        myAudioSource.pitch = Random.Range(0.8f, 1.0f);

        switch (material)
        {
            case "Grass":
                myAudioSource.PlayOneShot(footstepsOnGrass[Random.Range(0, footstepsOnGrass.Length)]);
                break;

            case "Metal":
                myAudioSource.PlayOneShot(footstepsOnMetal[Random.Range(0, footstepsOnMetal.Length)]);
                break;

            case "Tile":
                myAudioSource.PlayOneShot(footstepsOnTile[Random.Range(0, footstepsOnTile.Length)]);
                break;

            default:
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Grass":
            case "Metal":
            case "Tile":
                material = collision.gameObject.tag;
                WhenPlayingFootstepSound();
                break;

            default:
                break;
        }
    }
}

