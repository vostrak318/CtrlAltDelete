using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;

    [SerializeField]
    private AudioSource soundFXObject;

    [SerializeField]
    private AudioClip[] bgSongs;

    public AudioClip femaleJumpClip;
    public AudioClip maleJumpClip;
    public AudioClip walkClip;
    public AudioClip runClip;
    public AudioClip switchToAimCamClip;
    public AudioClip switchToDefaultCamClip;
    public AudioClip grabClip;
    public AudioClip throwClip;
    public AudioClip ragdollClip;
    public AudioClip femaleDeathClip;
    public AudioClip maleDeathClip;

    private AudioSource currentAudioSource;

    private AudioSource loopingAudioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClips, GameObject player, float volume)
    {
        int rnd = Random.Range(0, audioClips.Length);
        AudioSource audioSource = player.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = player.AddComponent<AudioSource>();
        }
        audioSource.clip = audioClips[rnd];
        audioSource.volume = volume;
        audioSource.Play();
    }

    public void PlayLoopingSoundFX(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        if (loopingAudioSource == null)
        {
            loopingAudioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
            loopingAudioSource.loop = true;
        }

        if (loopingAudioSource.clip != audioClip)
        {
            loopingAudioSource.clip = audioClip;
            loopingAudioSource.volume = volume;
            loopingAudioSource.Play();
        }
    }

    public void StopLoopingSoundFX()
    {
        if (loopingAudioSource != null)
        {
            loopingAudioSource.Stop();
            Destroy(loopingAudioSource.gameObject);
            loopingAudioSource = null;
        }
    }


    public bool IsPlaying(AudioClip clip)
    {
        return loopingAudioSource != null && loopingAudioSource.isPlaying && loopingAudioSource.clip == clip;
    }
}