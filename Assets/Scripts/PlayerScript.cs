using Assets.Code;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    public Rigidbody rb;
    public int numCasts;
    public float forceMult, forceMultCube, forceHorizontalFactor, forceVerticalDrag, forceMove, forceJump, forceDrop;
    public float collisionDampingFactor, collisionMaxForce, bounceFactor, walljumpVerticalFactor, walljumpForceMultiplier;
    public Vector2 walljumpVerticalSpeedRange;
    public float horizontalMaxSpeed, horizontalMaxSpeedGlide, horizontalDampStationary, horizontalDampCube, horizontalDampShotDisableTime;
    public float glideUpwardDrag, glideDownwardDrag, glideLift, glideSpeedMax;
    public InputActionReference inputMove, inputJump, inputDrop, inputStart;
    public MeshFilter meshFilter;
    public LayerMask layerMaskCollision;
    public TutorialTextScript tutorialTextScript;
    public UnityEvent onCheckpoint, onLevelTransition, onDeath, onWin;
    public GameObject prefabTrail;
    public Transform trailsContainer;

    Camera cam;
    List<Vector3> originalVertices;
    Dictionary<Vector3, HashSet<Vector3>> adjacentVertices;
    float inputJumpSeconds;
    bool inputDropped;
    float cubeFactor, vCubeFactor;
    Vector3 spawnPosition, respawnPosition;
    float shotCooldown;
    bool resetRB;
    PickupScript pickup;
    float pickupTimer, hitSFXTimer;
    public int secrets, deaths;
    List<TrailScript> trailDecals;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
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
        trailDecals = new();
    }

    void Update() {
        if (inputJump.action.triggered && shotCooldown <= 0) {
            inputJumpSeconds = .1f;
        } else {
            inputJumpSeconds -= Time.unscaledDeltaTime;
        }
        inputDropped = inputDrop.action.ReadValue<float>() > 0.5f;
        cubeFactor = Mathf.SmoothDamp(cubeFactor, inputDropped ? 1 : 0, ref vCubeFactor, 0.05f);
        shotCooldown = Mathf.Max(0, shotCooldown - Time.deltaTime);
        UpdateMesh();
        if (pickup) {
            pickupTimer -= Time.deltaTime;
            if (pickupTimer <= 0) {
                pickupTimer = 0;
                pickup.Done();
                pickup = null;
            }
        }
    }

    void UpdateMesh() {
        PickupType pickupType = GetPickupType();
        Dictionary<Vector3, float> scales = new();
        foreach (Vector3 originalVertex in originalVertices) {
            if (scales.ContainsKey(originalVertex)) continue;
            Vector3 direction = (transform.rotation * originalVertex).normalized;
            RaycastHit hit;
            Ray ray = new Ray(transform.position, direction);
            float modifiedDistance = 0;
            if (pickupType == PickupType.Glide) {
                modifiedDistance = Mathf.Lerp(1.5f, 0.1f, Mathf.Pow(Mathf.Abs(direction.y), 0.2f));
            } else {
                modifiedDistance = CubeRadiusInDirection(direction);
            }
            float maxDistance = Mathf.Lerp(0.5f, modifiedDistance, cubeFactor);
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
        PickupType pickupType = GetPickupType();

        // Level transitions.
        if (resetRB) {
            rb.transform.position = spawnPosition + new Vector3(0, 2, 0);
            rb.linearVelocity *= .9f;
            resetRB = false;
        }

        // Respawning.
        GameObject voidObject = GameObject.FindGameObjectWithTag("Void Y") ?? GameObject.FindGameObjectWithTag("Exit");
        if (voidObject != null && rb.position.y < voidObject.transform.position.y - 5) {
            rb.position = respawnPosition;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            shotCooldown = 0;
            deaths++;
            onDeath.Invoke();
            FinishPickup();
        }

        // Soft "collisions."
        float phi = Mathf.PI * (3f - Mathf.Sqrt(5f));
        Vector3 totalUndampedForce = Vector3.zero;
        float maxForcePerCast = collisionMaxForce / numCasts;
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
                        if (force.magnitude > maxForcePerCast) {
                            force /= force.magnitude / maxForcePerCast;
                        }
                        rb.AddForce(force, ForceMode.Acceleration);
                        if (hit.rigidbody?.gameObject.tag == "Prop") {
                            hit.rigidbody?.AddForce(-force, ForceMode.Force);
                        }
                        hit.rigidbody?.GetComponent<CollideWithPlayerScript>()?.CollideWithPlayer(rb);
                    }
                }
            }
        }
        if (numContacts > 2) {
            LeaveTrail(-totalUndampedForce, totalUndampedForce.magnitude);
        }
        float collisionDot = Vector3.Dot(totalUndampedForce, rb.linearVelocity);
        if (hitSFXTimer > 0) {
            hitSFXTimer -= Time.deltaTime;
        } else if (collisionDot < -300) {
            SFXScript.instance.SFXHitHard(1f + 1f * Mathf.InverseLerp(-300, -400, collisionDot));
            hitSFXTimer = .5f;
        } else if (collisionDot < -100) {
            SFXScript.instance.SFXHitSoft(1f + 1f * Mathf.InverseLerp(-100, -300, collisionDot));
            hitSFXTimer = .5f;
        }

        // Input.
        Vector2 horizontalVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
        float horizontalSpeed = horizontalVelocity.magnitude;
        Vector2 moveInputVector = inputMove.action.ReadValue<Vector2>();
        if (shotCooldown > 0) moveInputVector = Vector2.zero;
        if (GameObject.FindGameObjectWithTag("Intro")) return;
        if (inputDropped && (pickupType != PickupType.Glide || numContacts > 2)) moveInputVector = Vector2.zero;
        Vector3 moveVector = new Vector3(moveInputVector.x, 0, moveInputVector.y);
        moveVector = Quaternion.AngleAxis(cam.transform.localRotation.eulerAngles.y, Vector3.up) * moveVector;
        horizontalVelocity = Quaternion.AngleAxis(cam.transform.localRotation.eulerAngles.y, Vector3.up) * moveVector;
        Vector3 moveForce = moveVector * forceMove;
        if (inputDropped && pickupType == PickupType.Glide) {
            float dot = Vector2.Dot(moveVector.normalized, horizontalVelocity.normalized);
            float sidewaysness = 1 - Mathf.Abs(dot);
            moveForce *= sidewaysness;
        }
        rb.AddForce(moveForce, ForceMode.Acceleration);
        if (numContacts > 2) {
            Debug.DrawLine(transform.position, transform.position + totalUndampedForce.normalized * 0.5f, Color.white, 0.01f);
        }
        if (inputJumpSeconds > 0 && numContacts > 2 && !inputDropped) {
            float calculatedVerticalFactor = walljumpVerticalFactor * Mathf.InverseLerp(walljumpVerticalSpeedRange.x, walljumpVerticalSpeedRange.y, rb.linearVelocity.y);
            Vector3 jumpDirection = (totalUndampedForce.normalized + Vector3.up * calculatedVerticalFactor).normalized;
            // Damp player movement in the opposite direction of the jump.
            float dot = Vector3.Dot(rb.linearVelocity, jumpDirection);
            if (dot < 0) {
                rb.AddForce(-dot * jumpDirection * bounceFactor, ForceMode.VelocityChange);
            }
            // Non-vertical jumps get extra oomph to make walljumping easier.
            float jumpForce = forceJump;
            jumpForce *= 1 + jumpDirection.xz().magnitude * walljumpForceMultiplier;
            rb.AddForce(jumpDirection * jumpForce, ForceMode.VelocityChange);
            SFXScript.instance.SFXJump(0.4f * Mathf.InverseLerp(1, 5, jumpForce));
            inputJumpSeconds = 0;
        }

        horizontalVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
        horizontalSpeed = horizontalVelocity.magnitude;

        if (inputDropped) {
            if (pickupType == PickupType.Glide) {
                float drag = Mathf.Pow(rb.linearVelocity.y, 2) * rb.linearVelocity.y > 0 ? glideUpwardDrag : glideDownwardDrag;
                rb.AddForce((rb.linearVelocity.y > 0 ? Vector3.down : Vector3.up) * drag, ForceMode.Acceleration);
                float lift = Mathf.InverseLerp(0, glideSpeedMax, horizontalSpeed) * glideLift;
                rb.AddForce(Vector3.up * lift, ForceMode.Acceleration);
            } else {
                rb.AddForce(Vector3.down * forceDrop, ForceMode.Acceleration);
            }
        }

        // Horizontal damping.
        if (shotCooldown <= 0) {
            float calculatedHorizontalMaxSpeed = inputDropped && pickupType == PickupType.Glide ? horizontalMaxSpeedGlide : horizontalMaxSpeed;
            if (horizontalSpeed > calculatedHorizontalMaxSpeed) {
                horizontalVelocity /= horizontalSpeed / calculatedHorizontalMaxSpeed;
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
        // Vertical damping.
        rb.AddForce(Vector3.down * rb.linearVelocity.y * forceVerticalDrag, ForceMode.Acceleration);

        // SFX.
        SFXScript.instance.SetContactVolume(0.25f * Mathf.InverseLerp(2, 20, numContacts) * Mathf.InverseLerp(1, 8, horizontalSpeed));
    }

    public void TriggerCheckpoint(Vector3 position) {
        respawnPosition = position;
        onCheckpoint.Invoke();
    }
    public void LevelTransition() {
        FinishPickup();
        onLevelTransition.Invoke();
        resetRB = true;
    }
    public void GetShot(Vector3 force) {
        rb.AddForce(force, ForceMode.VelocityChange);
        shotCooldown = horizontalDampShotDisableTime;
    }
    public void Pickup(PickupScript pickup) {
        FinishPickup();
        this.pickup = pickup;
        pickupTimer = 10;
    }
    public PickupType GetPickupType() {
        return pickup?.type ?? PickupType.None;
    }
    public int GetPickupSecondsLeft() {
        return Mathf.CeilToInt(pickupTimer);
    }
    void FinishPickup() {
        pickup?.Done();
        pickupTimer = 0;
    }
    public void GetSecret() {
        secrets++;
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Tutorial Zone") {
            tutorialTextScript.Enter(other.gameObject.name);
        }
    }
    void OnTriggerExit(Collider other) {
        if (other.gameObject.tag == "Tutorial Zone") {
            tutorialTextScript.Leave();
        }
    }

    void LeaveTrail(Vector3 direction, float magnitude) {
        direction.Normalize();
        RaycastHit hit;
        Ray ray = new Ray(rb.position, direction);
        if (Physics.Raycast(ray, out hit, 0.5f, layerMaskCollision)) {
            TrailScript trail = GetTrailDecal();
            float size = .5f + .25f * Mathf.InverseLerp(10, 50, magnitude);
            trail.Activate(size);
            trail.transform.position = hit.point - direction * 0.1f;
            trail.transform.LookAt(hit.point);
        }
    }
    TrailScript GetTrailDecal() {
        foreach (TrailScript script in trailDecals) {
            if (!script.gameObject.activeSelf) return script;
        }
        return Instantiate(prefabTrail, trailsContainer).GetComponent<TrailScript>();
    }
}
