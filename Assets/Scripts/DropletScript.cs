using Assets.Code;
using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DropletScript : MonoBehaviour
{
    public MeshFilter meshFilter;

    List<Vector3> originalVertices;
    Dictionary<Vector3, HashSet<Vector3>> adjacentVertices;
    List<float> scaleHistory, vVertices;

    void Start() {
        originalVertices = new();
        meshFilter.mesh.GetVertices(originalVertices);
        adjacentVertices = new();
        foreach (Vector3 vertex in originalVertices) {
            if (!adjacentVertices.ContainsKey(vertex)) {
                adjacentVertices.Add(vertex, new());
            }
        }
        int[] triangles = meshFilter.mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3) {
            int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
            adjacentVertices[originalVertices[a]].Add(originalVertices[b]);
            adjacentVertices[originalVertices[b]].Add(originalVertices[a]);
            adjacentVertices[originalVertices[a]].Add(originalVertices[c]);
            adjacentVertices[originalVertices[c]].Add(originalVertices[a]);
            adjacentVertices[originalVertices[b]].Add(originalVertices[c]);
            adjacentVertices[originalVertices[c]].Add(originalVertices[b]);
        }
        scaleHistory = Enumerable.Repeat(1f, originalVertices.Count).ToList();
        vVertices = Enumerable.Repeat(0f, originalVertices.Count).ToList();
    }

    void Update() {
        UpdateMesh();
    }

    void UpdateMesh() {
        Dictionary<Vector3, float> scales = new();
        foreach (Vector3 originalVertex in originalVertices) {
            if (scales.ContainsKey(originalVertex)) continue;
            Vector3 direction = transform.rotation * originalVertex;
            RaycastHit hit;
            Ray ray = new Ray(transform.position, direction);
            float maxDistance = 0.5f;
            if (Physics.Raycast(ray, out hit, maxDistance)) {
                scales.Add(originalVertex, hit.distance * 2);
            } else {
                scales.Add(originalVertex, maxDistance * 2);
            }
        }
        List<Vector3> newVertices = new();
        for (int i = 0; i < originalVertices.Count; i++) {
            Vector3 vertex = originalVertices[i];
            float scaleSelf = scales[vertex];
            float scaleOtherTotal = 0;
            int otherCount = 0;
            foreach (Vector3 neighbor in adjacentVertices[vertex]) {
                scaleOtherTotal += scales[neighbor];
                otherCount++;
            }
            float scaleOtherAverage = scaleOtherTotal / otherCount;
            float scale = scaleOtherAverage;
            float v = vVertices[i];
            scale = Util.SpringDamper(scaleHistory[i], scale, ref v, 0.25f, 4 * Mathf.PI);
            scaleHistory[i] = scale;
            vVertices[i] = v;
            newVertices.Add(vertex * scale);
        }
        meshFilter.mesh.SetVertices(newVertices);
    }
}
