using TMPro;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    public PlayerScript playerScript;
    public CanvasGroup cgPickup;
    public TextMeshProUGUI tmpPickup;

    float vAlphaPickup;

    void Start() {
        cgPickup.alpha = 0;
    }

    void Update() {
        cgPickup.alpha = Mathf.SmoothDamp(cgPickup.alpha, playerScript.GetPickupType() == PickupType.None ? 0 : 1, ref vAlphaPickup, .2f);
        int pickupSeconds = playerScript.GetPickupSecondsLeft();
        if (pickupSeconds > 0) {
            tmpPickup.text = pickupSeconds.ToString();
        }
    }
}
