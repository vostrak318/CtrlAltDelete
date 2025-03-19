using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayBgMusic : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] bgSongs;
    void Start()
    {
        SoundFXManager.instance.PlayRandomSoundFXClip(bgSongs, gameObject, 0.8f);
    }
}
