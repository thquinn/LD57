using UnityEngine;

public class RandomSpinScript : MonoBehaviour {
    // ChatGPT did this
    public float rotationSpeed = 10f;
    public float axisChangeSpeed = 0.2f;

    private Vector3 currentAxis;
    private Vector3 targetAxis;

    void Start() {
        currentAxis = Random.onUnitSphere;
        targetAxis = Random.onUnitSphere;
    }

    void Update() {
        // Smoothly interpolate the current axis toward a new target axis
        currentAxis = Vector3.Slerp(currentAxis, targetAxis, axisChangeSpeed * Time.deltaTime).normalized;

        // Occasionally pick a new random target axis
        if (Vector3.Distance(currentAxis, targetAxis) < 0.1f) {
            targetAxis = Random.onUnitSphere;
        }

        // Rotate the object around the current axis
        transform.Rotate(currentAxis, rotationSpeed * Time.deltaTime, Space.World);
    }
}
