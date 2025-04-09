using UnityEngine;

public class WinScript : MonoBehaviour
{
    public MeshRenderer propRenderer;
    public CanvasGroup cgWinFade;
    public WinScreenScript winScreenScript;
    public PlayerScript playerScript;

    public bool won;
    bool winning;
    float t;
    float glow, vGlow;

    void Update() {
        if (!winning) return;
        t += Time.deltaTime;
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        glow = Mathf.SmoothDamp(glow, 100, ref vGlow, 3);
        block.SetFloat("_Glow", glow);
        propRenderer.SetPropertyBlock(block);
        if (t > 2) {
            cgWinFade.alpha += Time.deltaTime;
        }
        won = t > 2.5f;
        if (t > 4f) winScreenScript.Trigger();
    }

    private void OnTriggerEnter(Collider other) {
        if (!winning && other.gameObject == propRenderer.gameObject) {
            winning = true;
            playerScript.onWin.Invoke();
        }
    }
}
