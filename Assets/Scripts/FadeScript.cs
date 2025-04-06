using UnityEngine;

public class FadeScript : MonoBehaviour
{
    public LevelManagerScript levelManagerScript;
    public CanvasGroup canvasGroup;

    float vAlpha;

    void Start() {
        if (!Application.isEditor) canvasGroup.alpha = 1;
    }

    void Update() {
        canvasGroup.alpha = Mathf.SmoothDamp(canvasGroup.alpha, levelManagerScript.transitioning ? 1 : 0, ref vAlpha, 0.5f, 999, Time.unscaledDeltaTime);
    }
}
