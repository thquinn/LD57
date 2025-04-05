using UnityEngine;

public class BreakableScript : CollideWithPlayerScript
{
    public float breakVelocity;

    public override void CollideWithPlayer(Rigidbody playerRB) {
        if (Mathf.Abs(playerRB.linearVelocity.y) >= breakVelocity) {
            Destroy(gameObject);
        }
    }
}
