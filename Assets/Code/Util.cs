using UnityEngine;

namespace Assets.Code
{
    public static class Util {
        // by ChatGPT
        public static Quaternion SmoothDamp(Quaternion current, Quaternion target, ref Vector3 currentAngularVelocity, float smoothTime) {
            // Convert to angle-axis representation
            if (Time.deltaTime < Mathf.Epsilon) return current;

            // Ensure the shortest path is taken
            if (Quaternion.Dot(current, target) < 0f) {
                target = new Quaternion(-target.x, -target.y, -target.z, -target.w);
            }

            // Convert from Quaternion to Euler angles
            Vector3 currentEuler = current.eulerAngles;
            Vector3 targetEuler = target.eulerAngles;

            Vector3 resultEuler = new Vector3(
                Mathf.SmoothDampAngle(currentEuler.x, targetEuler.x, ref currentAngularVelocity.x, smoothTime),
                Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref currentAngularVelocity.y, smoothTime),
                Mathf.SmoothDampAngle(currentEuler.z, targetEuler.z, ref currentAngularVelocity.z, smoothTime)
            );

            return Quaternion.Euler(resultEuler);
        }

        public static float SpringDamper(
            float current,
            float target,
            ref float velocity,
            float dampingRatio,
            float angularFrequency)
        {
            float dt = Time.deltaTime;

            if (dt <= 0f || angularFrequency <= 0f)
                return current;

            float f = 1f + 2f * dt * dampingRatio * angularFrequency;
            float oo = angularFrequency * angularFrequency;
            float hoo = dt * oo;
            float hhoo = dt * hoo;
            float detInv = 1f / (f + hhoo);

            float detX = f * current + dt * velocity + hhoo * target;
            float detV = velocity + hoo * (target - current);

            float newX = detX * detInv;
            float newV = detV * detInv;

            velocity = newV;
            return newX;
        }
    }

    public static class VectorExtensions {
        public static Vector2 xz(this Vector3 v) {
            return new Vector2(v.x, v.z);
        }
    }
}
