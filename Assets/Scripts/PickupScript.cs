using Unity.VisualScripting;
using UnityEngine;

public class PickupScript : MonoBehaviour
{
    public PickupType type;
    public Transform transformVisual;
    public Vector3 direction;
    public float distance, period;

    float t;
    Vector3 initialPosition;

    void Start() {
        direction.Normalize();
        initialPosition = transformVisual.position;
    }
    
    void Update() {
        t += Time.deltaTime;
        transformVisual.position = initialPosition + direction * distance * Mathf.Sin(t / period * 2 * Mathf.PI);
    }

    private void OnTriggerStay(Collider other) {
        if (!transformVisual.gameObject.activeSelf) return;
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>().Pickup(this);
        transformVisual.gameObject.SetActive(false);
    }
    public void Done() {
        transformVisual.gameObject.SetActive(true);
    }
}

public enum PickupType {
    None, Glide
}