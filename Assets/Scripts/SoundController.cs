using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioSource fxSource;
    public AudioClip bankSound;
    public AudioClip policeSound;
    public AudioClip powerupSound;

    public AudioClip carMove;

    public void PlaySingle(AudioClip clip) {
        fxSource.clip = clip;
        fxSource.Play();
    }

    public void PlayBankSound() {
        PlaySingle(bankSound);
    }

    public void PlayPoliceSound() {
        PlaySingle(policeSound);
    }

    public void PlayPowerupSound() {
        PlaySingle(powerupSound);
    }

    public void PlayMoveSound() {
        PlaySingle(carMove);
    }
}
