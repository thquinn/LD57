using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class WinScreenScript : MonoBehaviour
{
    public PlayerScript playerScript;
    public CanvasGroup canvasGroup;
    public InputActionReference inputJump, inputStart;
    public TextMeshProUGUI tmp;

    bool triggered;
    float vAlpha;
    double lastTime;

    public void Trigger() {
        if (triggered) return;
        triggered = true;

        int numSecrets = FindObjectsByType<SecretScript>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        TimeSpan timeSpan = TimeSpan.FromSeconds(Time.unscaledTimeAsDouble - lastTime);
        string timeString = timeSpan.Hours > 0 ? timeSpan.ToString(@"h\:mm\:ss") : timeSpan.ToString(@"m\:ss");
        tmp.text = $"{timeString}\n" +
                   $"{playerScript.deaths}\n" +
                   $"{playerScript.secrets}/{numSecrets}";
        lastTime = Time.unscaledTimeAsDouble;
    }

    void Update() {
        if (!triggered) return;
        if (inputJump.action.triggered) Application.Quit();
        if (inputStart.action.triggered) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        canvasGroup.alpha = Mathf.SmoothDamp(canvasGroup.alpha, 1, ref vAlpha, .5f);
    }
}
