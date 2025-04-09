using UnityEngine;

public class PodiumScript : MonoBehaviour
{
    public GameObject icon, glow;
    public MeshRenderer iconMeshRenderer;
    public bool playSFX;

    float vIconGlow;

    void Start() {
        
    }

    void Update() {
        if (glow.activeSelf) {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            iconMeshRenderer.GetPropertyBlock(block);
            float iconGlow = block.GetFloat("_Glow");
            block.SetFloat("_Glow", Mathf.SmoothDamp(iconGlow, .5f, ref vIconGlow, .5f));
            iconMeshRenderer.SetPropertyBlock(block);
        }
    }

    private void OnTriggerEnter(Collider other) {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>().TriggerCheckpoint(transform.position + new Vector3(0, .5f, 0));
        if (glow.activeSelf) return;
        if (playSFX) SFXScript.instance.SFXPodium(0.5f);
        glow.SetActive(true);
    }
}
