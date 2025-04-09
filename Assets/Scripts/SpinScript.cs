using UnityEngine;

public class SpinScript : MonoBehaviour
{
    public float rate;
    public bool relativeToSelf;

    void Update() {
        transform.Rotate(0, rate * Time.deltaTime, 0, relativeToSelf ? Space.Self : Space.World);
    }
}
