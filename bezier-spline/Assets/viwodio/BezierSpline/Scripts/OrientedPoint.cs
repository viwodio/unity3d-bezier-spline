using UnityEngine;
using System.Collections;

namespace viwodio.BezierSpline
{
    [System.Serializable]
    public struct OrientedPoint
    {
        public Vector3 position;
        public Quaternion rotation;

        public OrientedPoint(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public Vector3 up => rotation * Vector3.up;
        public Vector3 right => rotation * Vector3.right;
        public Vector3 forward => rotation * Vector3.forward;

        public Vector3 WorldToLocal(Vector3 position)
        {
            return Quaternion.Inverse(rotation) * (position - this.position);
        }

        public Vector3 LocalToWorld(Vector3 position)
        {
            return this.position + rotation * position;
        }

        public Quaternion WorldToLocalRot(Quaternion rotation)
        {
            return Quaternion.Inverse(this.rotation) * rotation;
        }

        public Quaternion LocalToWorldRot(Quaternion rotation)
        {
            return this.rotation * rotation;
        }
    }
}
