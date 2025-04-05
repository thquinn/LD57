using UnityEngine;

public class PodiumScript : MonoBehaviour
{
    public GameObject icon, glow;
    public MeshRenderer iconMeshRenderer;

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
        glow.SetActive(true);
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>().SetRespawnPosition(transform.position + new Vector3(0, .5f, 0));
    }
}
