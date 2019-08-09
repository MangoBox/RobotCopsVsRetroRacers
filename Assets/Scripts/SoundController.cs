using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioSource fxSource;

    public AudioSource soundSource;
    public AudioClip bankSound;
    public AudioClip policeSound;
    public AudioClip powerupSound;

    public AudioClip carMove;
    public Soundtrack[] soundtracks;

    public static Soundtrack currentSoundtrack;

    [System.Serializable]
    public class Soundtrack {
        public AudioClip clip;
        [Range(0f,1f)]
        public float volume;
        public float length {
            get {
                return clip.length;
            }
        }
    }

    public void Start() {
        StartCoroutine(StartSoundTrack());
    }

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

    public IEnumerator StartSoundTrack() {
        Soundtrack selected = null;
        if(currentSoundtrack != null) {
            do {
            selected = soundtracks[Random.Range(0,soundtracks.Length)];
            } while (selected == currentSoundtrack);
        } else {
            selected = soundtracks[Random.Range(0,soundtracks.Length)];
        }
        soundSource.clip = selected.clip;
        soundSource.volume = selected.volume;
        soundSource.Play();
        currentSoundtrack = selected;

        yield return new WaitForSeconds(selected.length);
        StartCoroutine(StartSoundTrack());
    }
}
