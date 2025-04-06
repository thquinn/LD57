using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    public GameObject player;
    public InputActionReference inputLook;
    public float sensitivityX, sensitivityY;

    Vector3 aimPosition, v;
    float distance, verticalAngle, horizontalAngle;

    void Start() {
        aimPosition = player.transform.position;
        distance = 5;
        horizontalAngle = 180 * Mathf.Deg2Rad;
        verticalAngle =  30 * Mathf.Deg2Rad;
    }

    void Update() {
        Vector2 inputVector = inputLook.action.ReadValue<Vector2>();
        horizontalAngle += inputVector.x * sensitivityX * Time.deltaTime;

        aimPosition = Vector3.SmoothDamp(aimPosition, player.transform.position, ref v, 0.1f);
        float xzDistance = distance * Mathf.Cos(verticalAngle);
        float x = Mathf.Cos(horizontalAngle) * xzDistance;
        float y = Mathf.Sin(verticalAngle) * distance;
        float z = Mathf.Sin(horizontalAngle) * xzDistance;
        transform.localPosition = aimPosition + new Vector3(x, y, z);
        transform.LookAt(aimPosition);
    }
}
