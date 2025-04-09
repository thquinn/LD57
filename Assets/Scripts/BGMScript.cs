using UnityEngine;

public class BGMScript : MonoBehaviour
{
    public LevelManagerScript levelManagerScript;
    public AudioSource source;
    public AudioClip[] bgms;

    public int index;

    void Start() {
        source.clip = bgms[0];
        source.Play();
    }

    void Update() {
        if (!source.isPlaying && index < 2) {
            if (index == 1 && levelManagerScript.currentLevel == 0) return;
            index++;
            source.clip = bgms[index];
            source.Play();
            if (index == 2) {
                source.loop = true;
            }
        }
    }
}
