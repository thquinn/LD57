using UnityEngine;

public class LevelManagerScript : MonoBehaviour
{
    public PlayerScript playerScript;

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
        if (GameObject.FindGameObjectWithTag("Exit") != null) return;
        Time.timeScale = 0.2f;
        transitionTimer += Time.unscaledDeltaTime;
        if (transitionTimer >= 4) {
            transform.GetChild(currentLevel).gameObject.SetActive(false);
            currentLevel++;
            transform.GetChild(currentLevel).gameObject.SetActive(true);
            playerScript.LevelTransition();
            Time.timeScale = 1f;
            transitionTimer = 0;
        }
    }
}
