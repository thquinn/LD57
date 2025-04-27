using Assets.Code;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    static float MOUSE_SENSITIVITY = 0.5f;

    public GameObject player;
    public InputActionReference inputLook;
    public float sensitivityX, sensitivityY;
    public float bumpDistance;
    public Vector2 yAngleLimit;
    public float introTime;
    public LayerMask layerMaskCameraCollision;

    Vector3 aimPosition, v, vPos;
    float distance, verticalAngle, horizontalAngle;
    float introT;
    Vector3 introPosition;
    Quaternion introRotation;

    void Start() {
        aimPosition = player.transform.position;
        distance = 5;
        ResetAngles();
        introPosition = transform.position;
        introRotation = transform.rotation;
        if (GameObject.FindGameObjectWithTag("Intro") == null) {
            introT = introTime;
        }
    }
    public void ResetAngles() {
        horizontalAngle = 180;
        verticalAngle = 30;
    }

    void Update() {
        if (GameObject.FindGameObjectWithTag("Intro")) {
            return;
        }
        introT += Time.deltaTime;
        bool introDone = introT > introTime;

        Vector2 inputVector = inputLook.action.ReadValue<Vector2>();
        inputVector.x += Input.mousePositionDelta.x * MOUSE_SENSITIVITY;
        inputVector.y += Input.mousePositionDelta.y * MOUSE_SENSITIVITY;
        if (!introDone) inputVector = Vector2.zero;
        horizontalAngle += inputVector.x * sensitivityX * Time.deltaTime;
        verticalAngle += inputVector.y * sensitivityY * Time.deltaTime;
        verticalAngle = Mathf.Clamp(verticalAngle, yAngleLimit.x, yAngleLimit.y);

        if (Vector3.Distance(aimPosition, player.transform.position) > 10) {
            aimPosition = player.transform.position;
            v = Vector3.zero;
        } else {
            aimPosition = Vector3.SmoothDamp(aimPosition, player.transform.position, ref v, 0.1f);
        }
        float xzDistance = distance * Mathf.Cos(verticalAngle * Mathf.Deg2Rad);
        float x = Mathf.Cos(horizontalAngle * Mathf.Deg2Rad) * xzDistance;
        float y = Mathf.Sin(verticalAngle * Mathf.Deg2Rad) * distance;
        float z = Mathf.Sin(horizontalAngle * Mathf.Deg2Rad) * xzDistance;
        Vector3 targetPosition = aimPosition + new Vector3(x, y, z);

        // Push the camera forward if it would go through something.
        RaycastHit hit = new RaycastHit();
        Vector3 delta = targetPosition - aimPosition;
        if (Physics.Raycast(aimPosition, delta.normalized, out hit, delta.magnitude, layerMaskCameraCollision)) {
            targetPosition = aimPosition + delta.normalized * Mathf.Max(0, hit.distance - bumpDistance);
        }

        if (introDone) {
            float distance = Vector3.Distance(transform.localPosition, targetPosition);
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref vPos, 0.05f / Mathf.Max(1, distance));
            transform.LookAt(aimPosition);
        } else {
            Quaternion targetRotation = Quaternion.Euler(30, 90, 0);
            float t = introT / introTime;
            t = EasingFunctions.EaseInOutCubic(0, 1, t);
            transform.localPosition = Vector3.Lerp(introPosition, targetPosition, t);
            transform.localRotation = Quaternion.Lerp(introRotation, targetRotation, t);
        }
    }
}
