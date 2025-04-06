using UnityEngine;

public class BackgroundScript : MonoBehaviour
{
    public Transform playerTransform;

    void Update() {
        transform.position = playerTransform.position;
    }
}
