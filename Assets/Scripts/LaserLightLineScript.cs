using System.Collections.Generic;
using UnityEngine;

public class LaserLightLineScript : MonoBehaviour
{
    public float spacing, fadeRate;

    List<Light> lights;
    float startingIntensity;

    void Start() {
        lights = new List<Light>();
        lights.Add(GetComponentInChildren<Light>());
        startingIntensity = lights[0].intensity;
    }

    public void Show(Vector3 center, Quaternion rotation, float length) {
        Vector3 direction = rotation * Vector3.forward;
        Vector3 position = center + direction * length / -2;
        int i = 0;
        for (float f = 0; f < length - 1; f += spacing) {
            while (i >= lights.Count) {
                lights.Add(Instantiate(lights[0].gameObject, transform).GetComponent<Light>());
            }
            lights[i].transform.position = position;
            lights[i].enabled = true;
            lights[i].intensity = startingIntensity;
            position += direction * spacing;
            i++;
        }
    }

    void Update() {
        foreach (Light light in lights) {
            if (light.enabled) {
                light.intensity -= Time.deltaTime * fadeRate * startingIntensity;
                if (light.intensity <= 0) light.enabled = false;
            }
        }
    }
}
