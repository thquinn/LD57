using Assets.Code;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    public GameObject player;
    public InputActionReference inputLook;
    public float sensitivityX, sensitivityY;
    public float introTime;

    Vector3 aimPosition, v;
    float distance, verticalAngle, horizontalAngle;
    float introT;
    Vector3 introPosition;
    Quaternion introRotation;

    void Start() {
        aimPosition = player.transform.position;
        distance = 5;
        horizontalAngle = 180 * Mathf.Deg2Rad;
        verticalAngle =  30 * Mathf.Deg2Rad;
        introPosition = transform.position;
        introRotation = transform.rotation;
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

        aimPosition = Vector3.SmoothDamp(aimPosition, player.transform.position, ref v, 0.1f);
        float xzDistance = distance * Mathf.Cos(verticalAngle);
        float x = Mathf.Cos(horizontalAngle) * xzDistance;
        float y = Mathf.Sin(verticalAngle) * distance;
        float z = Mathf.Sin(horizontalAngle) * xzDistance;
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
