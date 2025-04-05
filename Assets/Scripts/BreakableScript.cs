using UnityEngine;

public class BreakableScript : CollideWithPlayerScript
{
    public float breakVelocity;

    public override void CollideWithPlayer(Rigidbody playerRB) {
        if (Mathf.Abs(playerRB.linearVelocity.y) >= breakVelocity) {
            Destroy(gameObject);
            foreach (Transform child in transform.parent) {
                child.gameObject.SetActive(true);
                child.GetComponent<Rigidbody>().AddForce(playerRB.linearVelocity, ForceMode.VelocityChange);
            }
        }
    }
}
