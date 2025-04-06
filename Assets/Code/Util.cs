using UnityEngine;

namespace Assets.Code
{
    public static class VectorExtensions {
        public static Vector2 xz(this Vector3 v) {
            return new Vector2(v.x, v.z);
        }
    }
}
