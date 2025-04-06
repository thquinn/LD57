using UnityEngine;

public class UpdraftScript : MonoBehaviour
{
    static float DEFAULT_FORCE = 8.5f;
    static float DEFAULT_SIZE = 3.5f;

    public BoxCollider collidre;
    public ParticleSystem particles;
    public float force;

    Rigidbody rbPlayer;

    void Start() {
        rbPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        float forceMult = force / DEFAULT_FORCE;
        float sizeMult = collidre.size.y / DEFAULT_SIZE;
        var velocityOverLifetime = particles.velocityOverLifetime;
        var vY = velocityOverLifetime.y;
        vY.constantMin *= forceMult;
        vY.constantMax *= forceMult;
        velocityOverLifetime.y = vY;
        var main = particles.main;
        var startLifetime = main.startLifetime;
        startLifetime.constant /= forceMult;
        startLifetime.constant *= sizeMult;
        main.startLifetime = startLifetime;
        var shape = particles.shape;
        Vector3 scale = collidre.size;
        scale.y -= 3 * forceMult;
        shape.position = new Vector3(0, collidre.size.y / -2, 0);
        shape.scale = scale;
    }

    private void OnTriggerStay(Collider other) {
        rbPlayer.AddForce(transform.up * force, ForceMode.Acceleration);
    }
}
