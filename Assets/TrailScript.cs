using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TrailScript : MonoBehaviour
{
    public DecalProjector decalProjector;
    public float fadeRate;
    public AnimationCurve fadeCurve;

    float fade;

    public void Activate(float size) {
        gameObject.SetActive(true);
        decalProjector.size = new Vector3(size, size, 1);
        fade = 1;
    }

    void Update() {
        if (!gameObject.activeSelf) return;
        fade = Mathf.Max(0, fade - Time.deltaTime * fadeRate);
        decalProjector.fadeFactor = fadeCurve.Evaluate(fade);
        if (fade == 0) {
            gameObject.SetActive(false);
        }
    }
}
