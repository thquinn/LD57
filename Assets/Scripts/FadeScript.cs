using UnityEngine;

public class FadeScript : MonoBehaviour
{
    public LevelManagerScript levelManagerScript;
    public WinScript winScript;
    public CanvasGroup canvasGroup, cgLogo;

    float vAlpha, vAlphaLogo;

    void Start() {
        if (!Application.isEditor) canvasGroup.alpha = 1;
    }

    void Update() {
        bool fadeUp = levelManagerScript.transitioning || winScript?.won == true;
        if (Time.time > 1) canvasGroup.alpha = Mathf.SmoothDamp(canvasGroup.alpha, fadeUp ? 1 : 0, ref vAlpha, 0.5f, 999, Time.unscaledDeltaTime);
        if (Time.time > 2) cgLogo.alpha = Mathf.SmoothDamp(cgLogo.alpha, Time.time > 6 ? 0 : 1, ref vAlphaLogo, 0.5f);
    }
}
