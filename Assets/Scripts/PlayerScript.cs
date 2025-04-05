using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    public Rigidbody rb;
    public int numCasts;
    public float forceMult, forceConst, forceHorizontalFactor, forceMove, forceJump;
    public float collisionDampingFactor, bounceFactor, walljumpVerticalFactor;
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
        int numContacts = 0;
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
                    if (Physics.Raycast(ray, out hit, 0.5f)) {
                        numContacts++;
                        float normalizedDistance = Mathf.InverseLerp(0.5f, 0.166f, hit.distance);
                        float strength = normalizedDistance * forceMult + forceConst;
                        strength /= numCasts;
                        strength *= Mathf.Lerp(Mathf.Abs(direction.y * direction.y), 1, forceHorizontalFactor);
                        Vector3 force = -direction * normalizedDistance * strength;
                        totalUndampedForce += -direction * strength;
                        // If the rigidbody is already moving in the direction of the force, we can damp.
                        float dot = Vector3.Dot(rb.linearVelocity, force);
                        float dampingFactor = Mathf.InverseLerp(1, 0, dot);
                        force *= Mathf.Lerp(1, dampingFactor, collisionDampingFactor);
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
        if (numContacts > 2) {
            Debug.DrawLine(transform.position, transform.position + totalUndampedForce.normalized * 0.5f, Color.white, 0.01f);
        }
        if (inputJumped && numContacts > 2) {
            Vector3 jumpDirection = (totalUndampedForce.normalized + Vector3.up * walljumpVerticalFactor).normalized;
            // Damp player movement in the opposite direction of the jump.
            float dot = Vector3.Dot(rb.linearVelocity, jumpDirection);
            if (dot < 0) {
                rb.AddForce(-dot * jumpDirection * bounceFactor, ForceMode.VelocityChange);
            }
            rb.AddForce(jumpDirection * forceJump, ForceMode.VelocityChange);
            inputJumped = false;
        }
    }
}
