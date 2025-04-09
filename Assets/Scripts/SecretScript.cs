using UnityEngine;

public class SecretScript : MonoBehaviour
{
    public ParticleSystem particles;
    public float followDistance;
    public float followTime;

    Vector3 initialPosition;
    PlayerScript playerScript;
    bool following, collected;
    Vector3 v;

    void Start() {
        initialPosition = transform.position;
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        playerScript.onCheckpoint.AddListener(Collect);
        playerScript.onLevelTransition.AddListener(Collect);
        playerScript.onWin.AddListener(Collect);
        playerScript.onDeath.AddListener(Reset);
    }

    void Update() {
        if (following) {
            float distance = Vector3.Distance(transform.position, playerScript.transform.position);
            if (distance > followDistance) {
                Vector3 targetPosition = Vector3.MoveTowards(transform.position, playerScript.transform.position, distance - followDistance);
                transform.position = Vector3.SmoothDamp(targetPosition, playerScript.transform.position, ref v, followTime);
            }
        } else {
            transform.position = Vector3.SmoothDamp(transform.position, initialPosition, ref v, followTime);
        }
    }

    private void OnTriggerEnter(Collider other) {
        following = true;
    }
    private void Collect() {
        if (!following || collected) return;
        collected = true;
        playerScript.GetSecret();
        particles.Play();
        transform.GetChild(0).gameObject.SetActive(false);
    }
    private void Reset() {
        following = false;
    }
}
