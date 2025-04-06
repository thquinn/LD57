using TMPro;
using UnityEngine;

public class TutorialTextScript : MonoBehaviour
{
    public TextMeshProUGUI tmp;
    public CanvasGroup canvasGroup;

    bool tutorialActive;
    float vAlpha;

    public void Enter(string text) {
        tutorialActive = true;
        tmp.text = text;
    }
    public void Leave() {
        tutorialActive = false;
    }

    void Update() {
        canvasGroup.alpha = Mathf.SmoothDamp(canvasGroup.alpha, tutorialActive ? 1 : 0, ref vAlpha, .2f);
    }
}
