using UnityEngine;

public class BobbingPlatformScript : MonoBehaviour
{
    public Rigidbody rb;
    public Vector3 direction;
    public float distance, period, phase;

    float t;
    Vector3 initialPosition;

    void Start() {
        direction.Normalize();
        t = period * phase;
        initialPosition = rb.transform.position;
    }
    
    void FixedUpdate() {
        t += Time.fixedDeltaTime;
        rb.position = initialPosition + direction * distance * Mathf.Sin(t / period * 2 * Mathf.PI);
    }
}
