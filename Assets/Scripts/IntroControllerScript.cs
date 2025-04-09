using UnityEngine;
using UnityEngine.InputSystem;

public class IntroControllerScript : MonoBehaviour
{
    public GameObject prefabDroplet;
    public int numDrops;
    public float dripSpeed, dripDrama, dripDelay, gravity;
    public InputActionReference inputSkip;

    GameObject droplet;
    int dropletCount = 0;
    Vector3 posSpawn, posTip;
    float dropletT, dropletV;

    void Start() {
        posSpawn = prefabDroplet.transform.position;
        posTip = new Vector3(0, 6, -0.1f);
    }

    void Update() {
        if (inputSkip.action.triggered) {
            Destroy(droplet);
            Done();
        }
        if (droplet == null) {
            if (dropletCount >= numDrops) return;
            droplet = Instantiate(prefabDroplet, transform);
            dropletT = 0;
            dropletV = 0;
            dropletCount++;
        }
        bool postdrip = dropletT >= 1 + dripDelay * dripSpeed;
        dropletT += Time.deltaTime * dripSpeed;
        if (!postdrip && dropletT >= 1 + dripDelay * dripSpeed) {
            SFXScript.instance.SFXDrip(0.25f);
        }
        if (dropletT < 1) {
            droplet.transform.position = Vector3.Lerp(posSpawn, posTip, Mathf.Pow(dropletT, dripDrama));
        } else if (dropletT > 1 + dripDelay * dripSpeed) {
            dropletV += Time.deltaTime * gravity;
            droplet.transform.Translate(0, -dropletV, 0);
            if (droplet.transform.position.y <= 0) {
                Destroy(droplet);
                if (dropletCount == numDrops) {
                    Done();
                }
            }
        }
    }

    void Done() {
        gameObject.tag = "Untagged";
        dropletCount = 3;
    }
}
