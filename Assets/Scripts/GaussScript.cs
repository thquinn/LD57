using UnityEngine;
using UnityEngine.Rendering;

public class GaussScript : MonoBehaviour
{
    public GameObject reticle, laser;
    public SpriteRenderer reticleInRenderer;
    public GameObject[] reticleOuts;
    public SpriteRenderer[] reticleOutRenderers;
    public MeshRenderer laserRenderer;
    public LayerMask layerMaskPlayerAndTerrain;
    public float followFactor, followFactorLocked, maxRange, lockDistanceMin, lockDistanceMax, lockSpeed, shootTime, shootDelay, shootPower;

    GameObject player;
    Camera cam;
    Vector3 reticlePosition, vReticle;
    float vInAlpha;
    float lockOn, shootTimer, laserAlpha;

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        cam = Camera.main;
        reticlePosition = transform.position;
    }

    void Update() {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        RaycastHit hit = new RaycastHit();
        bool didHit = Physics.Raycast(transform.position, direction, out hit, maxRange, layerMaskPlayerAndTerrain) && hit.rigidbody?.gameObject.layer == LayerMask.NameToLayer("Player");
        Color c = reticleInRenderer.color;
        c.a = Mathf.SmoothDamp(c.a, didHit ? 1 : 0, ref vInAlpha, 0.2f);
        reticleInRenderer.color = c;
        Vector3 reticleTarget = didHit ? player.transform.position : transform.position;
        float calculatedFollowFactor = followFactor;
        if (shootTimer > 0) {
            calculatedFollowFactor = shootTimer < shootDelay ? followFactorLocked : 0;
        }
        reticlePosition = Vector3.SmoothDamp(reticlePosition, reticleTarget, ref vReticle, 1 - calculatedFollowFactor);
        reticle.transform.position = reticlePosition;
        reticle.transform.LookAt(cam.transform);
        if (shootTimer > 0) {
            float nextShootTimer = shootTimer + Time.deltaTime;
            if (shootTimer < shootDelay && nextShootTimer >= shootDelay) {
                // Check if the player is still in the direction of the reticle.
                hit = new RaycastHit();
                Vector3 reticleDirection = (reticlePosition - transform.position).normalized;
                didHit = Physics.Raycast(transform.position, reticleDirection, out hit, maxRange, layerMaskPlayerAndTerrain) && hit.rigidbody?.gameObject.layer == LayerMask.NameToLayer("Player");
                if (didHit) {
                    player.GetComponent<PlayerScript>().GetShot(direction * shootPower);
                }
                float laserLength = hit.collider == null ? maxRange : hit.distance;
                laser.transform.position = transform.position + reticleDirection * laserLength / 2;
                Vector3 laserScale = laser.transform.localScale;
                laserScale.z = laserLength;
                laser.transform.localScale = laserScale;
                laser.transform.rotation = Quaternion.LookRotation(reticleDirection);
                laserAlpha = 1;
                if (hit.collider?.gameObject != null) {
                    ParticleSystem particles = GameObject.FindGameObjectWithTag("Laser Impact").GetComponent<ParticleSystem>();
                    particles.transform.position = hit.point;
                    particles.Play();

                }
                GameObject.FindGameObjectWithTag("Laser Light Line").GetComponent<LaserLightLineScript>().Show(laser.transform.position, laser.transform.rotation, laserLength);
                SFXScript.instance.SFXLaser(0.5f);
            }
            shootTimer = nextShootTimer;
            if (shootTimer > shootTime) {
                lockOn = 0;
                shootTimer = 0;
            }
        }
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        laserRenderer.GetPropertyBlock(block);
        block.SetFloat("_Alpha", Mathf.Clamp01(laserAlpha));
        laserRenderer.SetPropertyBlock(block);
        laserAlpha -= Time.deltaTime * 10;
        if (shootTimer > 0) return;
        float reticleDistanceToPlayer = didHit ? Vector3.Distance(reticlePosition, player.transform.position) : 999;
        lockOn += Mathf.Lerp(-Time.deltaTime * lockSpeed, Time.deltaTime * lockSpeed, Mathf.InverseLerp(lockDistanceMax, lockDistanceMin, reticleDistanceToPlayer));
        lockOn = Mathf.Clamp01(lockOn);
        foreach (SpriteRenderer sr in reticleOutRenderers) {
            c = sr.color;
            c.a = lockOn;
            sr.color = c;
        }
        float outX = Mathf.Lerp(2, 0, lockOn * 1.5f);
        foreach (GameObject o in reticleOuts) {
            o.transform.localPosition = new Vector3(outX, 0, 0);
        }
        if (lockOn >= 1) {
            shootTimer = Time.deltaTime;
            vReticle = Vector3.zero;
        }

        if (lockOn > 0 || didHit) {
            float volume = 0.05f + 0.1f * Mathf.InverseLerp(3, 1, Vector3.Distance(reticlePosition, player.transform.position));
            float pitch = 1 + lockOn;
            SFXScript.instance.SetTrackingVolumeAndPitch(volume, pitch);
        }
    }
}
