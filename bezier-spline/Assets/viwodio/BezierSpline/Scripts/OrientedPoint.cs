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

        public Vector3 WorldToLocal(Vector3 worldPos)
        {
            return Quaternion.Inverse(rotation) * (worldPos - position);
        }

        public Vector3 LocalToWorld(Vector3 localPos)
        {
            return position + rotation * localPos;
        }

        public Quaternion WorldToLocal(Quaternion worldRotation)
        {
            return Quaternion.Inverse(rotation) * worldRotation;
        }

        public Quaternion LocalToWorld(Quaternion localRotation)
        {
            return rotation * localRotation;
        }
    }
}
