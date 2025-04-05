using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    public Rigidbody rb;
    public int numCasts;
    public float forceMult, forceConst, forceHorizontalFactor, forceMove, forceJump;
    public float collisionDampingFactor;
    public Camera cam;
    public InputActionReference inputMove, inputJump;

    bool inputJumped;

    void Start() {
        cam = Camera.main;
    }

    void Update() {
        inputJumped |= inputJump.action.triggered;
    }

    void FixedUpdate() {
        // Soft "collisions."
        float phi = Mathf.PI * (3f - Mathf.Sqrt(5f));
        Vector3 totalUndampedForce = Vector3.zero;
        for (int i = 0; i < numCasts; i++) {
            float y = 1f - (i / (float)(numCasts - 1)) * 2f;
            float radius = Mathf.Sqrt(1f - y * y);
            float theta = phi * i;
            float x = Mathf.Cos(theta) * radius;
            float z = Mathf.Sin(theta) * radius;
            for (int mx = -1; mx <= 1; mx += 2) {
                for (int mz = -1; mz <= 1; mz += 2) {
                    Vector3 direction = new Vector3(x * mx, y, z * mz);
                    RaycastHit hit;
                    Ray ray = new Ray(transform.position, direction);
                    Debug.DrawLine(transform.position, transform.position + direction * 0.5f, Color.white, 0.01f);
                    if (Physics.Raycast(ray, out hit, 0.5f)) {
                        float normalizedDistance = Mathf.InverseLerp(0.5f, 0.166f, hit.distance);
                        float strength = normalizedDistance * forceMult + forceConst;
                        strength *= Mathf.Lerp(Mathf.Abs(direction.y * direction.y), 1, forceHorizontalFactor);
                        strength /= numCasts;
                        Vector3 force = -direction * normalizedDistance * strength;
                        totalUndampedForce += force;
                        // If the rigidbody is already moving in the direction of the force, we can damp.
                        float dot = Vector3.Dot(rb.linearVelocity, force);
                        float dampingFactor = Mathf.InverseLerp(1, 0, dot);
                        force *= dampingFactor;
                        rb.AddForce(force, ForceMode.Acceleration);
                    }
                }
            }
        }

        // Input.
        Vector2 moveInputVector = inputMove.action.ReadValue<Vector2>();
        Vector3 moveVector = new Vector3(moveInputVector.x, 0, moveInputVector.y);
        moveVector = Quaternion.AngleAxis(cam.transform.localRotation.eulerAngles.y, Vector3.up) * moveVector;
        rb.AddForce(moveVector * forceMove, ForceMode.Acceleration);
        if (inputJumped && totalUndampedForce != Vector3.zero) {
            Vector3 jumpDirection = (totalUndampedForce.normalized + Vector3.up).normalized;
            rb.AddForce(jumpDirection * forceJump, ForceMode.VelocityChange);
            inputJumped = false;
        }
    }
}
