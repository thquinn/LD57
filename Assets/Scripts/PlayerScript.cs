using Assets.Code;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    public Rigidbody rb;
    public int numCasts;
    public float forceMult, forceMultCube, forceHorizontalFactor, forceMove, forceJump, forceDrop;
    public float collisionDampingFactor, bounceFactor, walljumpVerticalFactor, walljumpForceMultiplier;
    public float horizontalMaxSpeed, horizontalDampStationary, horizontalDampCube, horizontalDampShotDisableTime;
    public InputActionReference inputMove, inputJump, inputDrop;
    public MeshFilter meshFilter;
    public LayerMask layerMaskCollision;

    Camera cam;
    List<Vector3> originalVertices;
    Dictionary<Vector3, HashSet<Vector3>> adjacentVertices;
    bool inputJumped, inputDropped;
    float cubeFactor, vCubeFactor;
    Vector3 spawnPosition, respawnPosition;
    float shotCooldown;
    bool resetRB;

    void Start() {
        cam = Camera.main;
        originalVertices = new();
        meshFilter.mesh.GetVertices(originalVertices);
        adjacentVertices = new();
        foreach (Vector3 vertex in originalVertices) {
            if (!adjacentVertices.ContainsKey(vertex)) {
                adjacentVertices.Add(vertex, new());
            }
        }
        int[] triangles = meshFilter.mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3) {
            int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
            adjacentVertices[originalVertices[a]].Add(originalVertices[b]);
            adjacentVertices[originalVertices[b]].Add(originalVertices[a]);
            adjacentVertices[originalVertices[a]].Add(originalVertices[c]);
            adjacentVertices[originalVertices[c]].Add(originalVertices[a]);
            adjacentVertices[originalVertices[b]].Add(originalVertices[c]);
            adjacentVertices[originalVertices[c]].Add(originalVertices[b]);
        }
        spawnPosition = transform.position;
    }

    void Update() {
        inputJumped |= inputJump.action.triggered && shotCooldown <= 0;
        inputDropped = inputDrop.action.ReadValue<float>() > 0.5f;
        cubeFactor = Mathf.SmoothDamp(cubeFactor, inputDropped ? 1 : 0, ref vCubeFactor, 0.05f);
        shotCooldown = Mathf.Max(0, shotCooldown - Time.deltaTime);
        UpdateMesh();
    }

    void UpdateMesh() {
        Dictionary<Vector3, float> scales = new();
        foreach (Vector3 originalVertex in originalVertices) {
            if (scales.ContainsKey(originalVertex)) continue;
            Vector3 direction = transform.rotation * originalVertex;
            RaycastHit hit;
            Ray ray = new Ray(transform.position, direction);
            float maxDistance = Mathf.Lerp(0.5f, CubeRadiusInDirection(direction.normalized), cubeFactor);
            if (Physics.Raycast(ray, out hit, maxDistance, layerMaskCollision)) {
                scales.Add(originalVertex, hit.distance * 2);
            } else {
                scales.Add(originalVertex, maxDistance * 2);
            }
        }
        List<Vector3> newVertices = new();
        foreach (Vector3 vertex in originalVertices) {
            float scaleSelf = scales[vertex];
            float scaleOtherTotal = 0;
            int otherCount = 0;
            foreach (Vector3 neighbor in adjacentVertices[vertex]) {
                scaleOtherTotal += scales[neighbor];
                otherCount++;
            }
            float scaleOtherAverage = scaleOtherTotal / otherCount;
            float scale = scaleSelf * .25f + scaleOtherAverage * .75f;
            newVertices.Add(vertex * scale);
        }
        meshFilter.mesh.SetVertices(newVertices);
    }
    public static float CubeRadiusInDirection(Vector3 dir) {
        // ChatGPT did this
        float xDist = 0.5f / Mathf.Abs(dir.x == 0f ? float.Epsilon : dir.x);
        float yDist = 0.5f / Mathf.Abs(dir.y == 0f ? float.Epsilon : dir.y);
        float zDist = 0.5f / Mathf.Abs(dir.z == 0f ? float.Epsilon : dir.z);

        return Mathf.Min(xDist, yDist, zDist);
    }

    void FixedUpdate() {
        // Level transitions.
        if (resetRB) {
            rb.transform.position = spawnPosition + new Vector3(0, 2, 0);
            rb.linearVelocity *= .9f;
            resetRB = false;
        }

        // Respawning.
        if (rb.position.y < GameObject.FindGameObjectWithTag("Exit")?.transform.position.y - 5) {
            rb.position = respawnPosition;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

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
            // Make sure contact checks are symmetrical, or else the asymmetry will push the player in a direction even on a flat surface.
            for (int mx = -1; mx <= 1; mx += 2) {
                for (int mz = -1; mz <= 1; mz += 2) {
                    Vector3 direction = new Vector3(x * mx, y, z * mz);
                    RaycastHit hit;
                    Ray ray = new Ray(transform.position, direction);
                    if (Physics.Raycast(ray, out hit, 0.5f, layerMaskCollision)) {
                        numContacts++;
                        float normalizedDistance = Mathf.InverseLerp(0.5f, 0.166f, hit.distance);
                        float strength = normalizedDistance * Mathf.Lerp(forceMult, forceMultCube, cubeFactor);
                        strength /= numCasts;
                        strength *= Mathf.Lerp(Mathf.Abs(direction.y * direction.y), 1, forceHorizontalFactor);
                        Vector3 force = -direction * normalizedDistance * strength;
                        totalUndampedForce += -direction * strength;
                        // If the rigidbody is already moving in the direction of the force, we can damp.
                        float dot = Vector3.Dot(rb.linearVelocity, force);
                        float dampingFactor = Mathf.InverseLerp(1, 0, dot);
                        force *= Mathf.Lerp(1, dampingFactor, collisionDampingFactor);
                        rb.AddForce(force, ForceMode.Acceleration);
                        hit.rigidbody?.GetComponent<CollideWithPlayerScript>()?.CollideWithPlayer(rb);
                    }
                }
            }
        }

        // Input.
        Vector2 moveInputVector = inputMove.action.ReadValue<Vector2>();
        if (shotCooldown > 0) moveInputVector = Vector2.zero;
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
            // Non-vertical jumps get extra oomph to make walljumping easier.
            float jumpForce = forceJump;
            jumpForce *= 1 + jumpDirection.xz().magnitude * walljumpForceMultiplier;
            rb.AddForce(jumpDirection * jumpForce, ForceMode.VelocityChange);
            inputJumped = false;
        }
        if (inputDropped) {
            rb.AddForce(Vector3.down * forceDrop, ForceMode.Acceleration);
        }

        // Horizontal damping.
        if (shotCooldown <= 0) {
            Vector2 horizontalVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > horizontalMaxSpeed) {
                horizontalVelocity /= horizontalVelocity.magnitude / horizontalMaxSpeed;
            }
            if (numContacts > 2) {
                float horizontalDamping = 0;
                if (moveInputVector == Vector2.zero) {
                    horizontalDamping = horizontalDampStationary;
                }
                horizontalDamping = Mathf.Lerp(horizontalDamping, horizontalDampCube, cubeFactor);
                horizontalDamping = Mathf.Pow(1 - horizontalDamping, Time.fixedDeltaTime);
                horizontalVelocity *= horizontalDamping;
            }
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.y);
        }
    }

    public void SetRespawnPosition(Vector3 position) {
        respawnPosition = position;
    }
    public void LevelTransition() {
        resetRB = true;
    }
    public void GetShot(Vector3 force) {
        rb.AddForce(force, ForceMode.VelocityChange);
        shotCooldown = horizontalDampShotDisableTime;
    }
}
