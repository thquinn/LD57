using UnityEngine;

public class SFXScript : MonoBehaviour {
    public static SFXScript instance;

    public AudioSource sfxContact, sfxTracking, sfxOneShot;
    public AudioClip[] sfxHitHard, sfxHitSoft, sfxJump;
    public AudioClip sfxDrip, sfxPodium, sfxShatter, sfxLaser;

    int lastHitHard = -1, lastHitSoft = -1, lastJump = -1;
    float lastTrackingUpdate, vTrackingVolume;

    void Start() {
        instance = this;
    }
    void Update() {
        if (Time.time > lastTrackingUpdate + .03f) {
            sfxTracking.volume = Mathf.SmoothDamp(sfxTracking.volume, 0, ref vTrackingVolume, .05f);
        }
    }

    public void SetContactVolume(float volume) {
        sfxContact.volume = volume;
    }
    public void SetTrackingVolumeAndPitch(float volume, float pitch) {
        sfxTracking.volume = volume;
        sfxTracking.pitch = pitch;
        lastTrackingUpdate = Time.time;
        vTrackingVolume = 0;
    }
    public void SFXHitHard(float volume) {
        int nextIndex = lastHitHard;
        while (nextIndex == lastHitHard) {
            nextIndex = Random.Range(0, sfxHitHard.Length);
        }
        sfxOneShot.PlayOneShot(sfxHitHard[nextIndex], volume);
        lastHitHard = nextIndex;
    }
    public void SFXHitSoft(float volume) {
        int nextIndex = lastHitSoft;
        while (nextIndex == lastHitSoft) {
            nextIndex = Random.Range(0, sfxHitSoft.Length);
        }
        sfxOneShot.PlayOneShot(sfxHitSoft[nextIndex], volume);
        lastHitSoft = nextIndex;
    }
    public void SFXJump(float volume) {
        int nextIndex = lastJump;
        while (nextIndex == lastJump) {
            nextIndex = Random.Range(0, sfxJump.Length);
        }
        sfxOneShot.PlayOneShot(sfxJump[nextIndex], volume);
        lastJump = nextIndex;
    }
    public void SFXDrip(float volume) {
        sfxOneShot.PlayOneShot(sfxDrip, volume);
    }
    public void SFXPodium(float volume) {
        sfxOneShot.PlayOneShot(sfxPodium, volume);
    }
    public void SFXShatter(float volume) {
        sfxOneShot.PlayOneShot(sfxShatter, volume);
    }
    public void SFXLaser(float volume) {
        sfxOneShot.PlayOneShot(sfxLaser, volume);
    }
}
