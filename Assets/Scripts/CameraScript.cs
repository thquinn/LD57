using Assets.Code;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    public GameObject player;
    public InputActionReference inputLook;
    public float sensitivityX, sensitivityY;
    public Vector2 yAngleLimit;
    public float introTime;

    Vector3 aimPosition, v;
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
        if (introDone) {
            transform.localPosition = targetPosition;
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
