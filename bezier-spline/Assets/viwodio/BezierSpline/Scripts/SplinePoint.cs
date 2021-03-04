using UnityEngine;
using System.Collections;

namespace viwodio.BezierSpline
{
    [System.Serializable]
    public class SplinePoint
    {
        [SerializeField] private Vector3 _localLeftTangent;
        [SerializeField] private Vector3 _localRightTangent;
        [SerializeField] private TangentMode tangentMode;
        public OrientedPoint point;

        public TangentMode TangentMode => tangentMode;

        public Vector3 localLeftTangent
        {
            get => _localLeftTangent;
            set
            {
                _localLeftTangent = value;

                if (tangentMode == TangentMode.Mirrored)
                    _localRightTangent = -_localLeftTangent;
            }
        }

        public Vector3 localRightTangent
        {
            get => _localRightTangent;
            set
            {
                _localRightTangent = value;

                if (tangentMode == TangentMode.Mirrored)
                    _localLeftTangent = -_localRightTangent;
            }
        }

        public Vector3 position
        {
            get => point.position;
            set => point.position = value;
        }

        public Quaternion rotation
        {
            get => point.rotation;
            set => point.rotation = value;
        }

        public Vector3 rightTangent
        {
            get => point.LocalToWorld(localRightTangent);
            set => localRightTangent = point.WorldToLocal(value);
        }

        public Vector3 leftTangent
        {
            get => point.LocalToWorld(_localLeftTangent);
            set => localLeftTangent = point.WorldToLocal(value);
        }

        public void SetTangentMode(TangentMode tangentMode)
        {
            switch (tangentMode)
            {
                case TangentMode.Mirrored:
                    _localRightTangent = -_localLeftTangent;
                    break;
            }

            this.tangentMode = tangentMode;
        }

        public Vector3 GetLocalLinearLeftTangent(SplinePoint previousPoint)
        {
            return (position - previousPoint.position).normalized;
        }

        public Vector3 GetLinearLeftTangent(SplinePoint previousPoint)
        {
            return position - GetLocalLinearLeftTangent(previousPoint);
        }

        public Vector3 GetLocalLinearRightTangent(SplinePoint nextPoint)
        {
            return (nextPoint.position - position).normalized;
        }

        public Vector3 GetLinearRightTangent(SplinePoint nextPoint)
        {
            return position + GetLocalLinearRightTangent(nextPoint);
        }

        public SplinePoint(Vector3 position, Quaternion rotation)
        {
            point = new OrientedPoint(position, rotation);
            tangentMode = TangentMode.Mirrored;
            localLeftTangent = Vector3.left * 1f;
            localRightTangent = Vector3.right * 1f;
        }
    }
}
