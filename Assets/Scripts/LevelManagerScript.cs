using UnityEngine;

public class LevelManagerScript : MonoBehaviour
{
    public PlayerScript playerScript;
    public CameraScript cameraScript;
    public bool transitioning;

    int currentLevel;
    float transitionTimer;

    void Start() {
        foreach (Transform child in transform) {
            if (child.gameObject.activeSelf) {
                break;
            }
            currentLevel++;
        }
    }

    void Update() {
        transitioning = GameObject.FindGameObjectWithTag("Exit") == null;
        if (!transitioning) return;
        Time.timeScale = 0.2f;
        transitionTimer += Time.unscaledDeltaTime;
        if (transitionTimer >= 3) {
            transform.GetChild(currentLevel).gameObject.SetActive(false);
            currentLevel++;
            transform.GetChild(currentLevel).gameObject.SetActive(true);
            playerScript.LevelTransition();
            cameraScript.ResetAngles();
            Time.timeScale = 1f;
            transitionTimer = 0;
        }
    }
}
