using UnityEngine;

public class FanScript : MonoBehaviour
{
    public Rigidbody rbHub;
    public float speed;

    float t;

    void FixedUpdate() {
        t += Time.fixedDeltaTime;
        rbHub.MoveRotation(Quaternion.Euler(0, speed * t, 0));
    }
}
