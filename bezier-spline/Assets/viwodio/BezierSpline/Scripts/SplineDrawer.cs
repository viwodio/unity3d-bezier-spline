using UnityEngine;
using System.Collections;
using UnityEditor;

namespace viwodio.BezierSpline
{
    [ExecuteAlways]
    public class SplineDrawer : MonoBehaviour
    {
        public GizmoSettings gizmoSettings;

        public Spline spline;

        private Vector3 lastPosition;
        private Quaternion lastRotation;

        void Update()
        {
            if (transform.position != lastPosition)
            {
                spline.MoveAllPoints(transform.position - lastPosition);
            }

            if (transform.rotation != lastRotation)
            {
                spline.EachPoint((point) => {

                    // World to Local
                    Vector3 localPos = Quaternion.Inverse(lastRotation) * (point.position - transform.position);
                    // Local to World
                    point.position = transform.position + transform.rotation * localPos;


                    // World to Local
                    Quaternion localRot = Quaternion.Inverse(lastRotation) * point.rotation;
                    // Local to World
                    point.rotation = (transform.rotation * localRot).normalized;
                });
            }

            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (Selection.Contains(gameObject.GetInstanceID())) return;
            else if (!gizmoSettings.drawGizmoAlways) return;

            spline.EachLine((start, end) => {

                Gizmos.DrawSphere(end.position, gizmoSettings.radius * .5f);
                Gizmos.DrawSphere(start.position, gizmoSettings.radius * .5f);

                SplineHandles.DrawBezierLine(start, end, spline.segmentCount, gizmoSettings.thickness);
            });

            /*
            spline.EachPoint((point) => {

                Gizmos.DrawSphere(point.leftTangent, gizmoSettings.radius * .5f);
                Gizmos.DrawSphere(point.rightTangent, gizmoSettings.radius * .5f);

                Handles.DrawAAPolyLine(gizmoSettings.thickness, point.position, point.rightTangent);
                Handles.DrawAAPolyLine(gizmoSettings.thickness, point.leftTangent, point.position);
            });
            */
        }
#endif
    }
}