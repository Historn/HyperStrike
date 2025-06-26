using UnityEngine;

public class PlayBallSound : MonoBehaviour
{
    public AudioClip glassSFX;
    public AudioClip metalSFX;
    public AudioClip grassSFX;

    private AudioSource myAudioSource;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Glass":
                myAudioSource.volume = Random.Range(0.8f, 1.0f);
                myAudioSource.pitch = Random.Range(0.8f, 1.0f);

                myAudioSource.PlayOneShot(glassSFX);
                break;

            case "Metal":
                myAudioSource.volume = Random.Range(0.8f, 1.0f);
                myAudioSource.pitch = Random.Range(0.8f, 1.0f);

                myAudioSource.PlayOneShot(metalSFX);
                break;

            case "Grass":
                myAudioSource.volume = Random.Range(0.8f, 1.0f);
                myAudioSource.pitch = Random.Range(0.8f, 1.0f);

                myAudioSource.PlayOneShot(grassSFX);
                break;

            default:
                break;
        }
    }
}