using UnityEngine;

public class BreakableScript : CollideWithPlayerScript
{
    public float breakVelocity;

    public override void CollideWithPlayer(Rigidbody playerRB) {
        if (playerRB.linearVelocity.y <= -breakVelocity) {
            foreach (Transform child in transform.parent) {
                if (child.gameObject.activeSelf) {
                    child.gameObject.SetActive(false);
                } else {
                    child.gameObject.SetActive(true);
                    child.GetComponent<Rigidbody>().AddForce(playerRB.linearVelocity, ForceMode.VelocityChange);
                }
            }
        }
    }
}
