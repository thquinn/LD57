using UnityEngine;

public class RespawningPropScript : MonoBehaviour
{
    public Rigidbody rb;
    public float voidY;

    Vector3 initialPosition;
    Quaternion initialRotation;

    void Start() {
        initialPosition = rb.position;
        initialRotation = rb.rotation;
    }

    void FixedUpdate() {
        if (rb.position.y < voidY) {
            rb.position = initialPosition;
            rb.rotation = initialRotation;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
