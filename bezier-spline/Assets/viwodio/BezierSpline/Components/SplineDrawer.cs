using UnityEngine;

namespace viwodio.BezierSpline.Component
{
    [ExecuteAlways]
    public class SplineDrawer : MonoBehaviour
    {
        public GizmoSettings gizmoSettings;

        public Spline spline;

        [SerializeField, HideInInspector] private Vector3 lastPosition;
        [SerializeField, HideInInspector] private Quaternion lastRotation;

        void Awake()
        {
            RecordLastPosRot();
        }

        void Update()
        {
            UpdateSplinePosRot();
        }

        public void RecordLastPosRot()
        {
            lastPosition = transform.position;
            lastRotation = transform.rotation;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.RecordObject(this, "Spline Drawer");
            }
#endif
        }

        public void UpdateSplinePosRot()
        {
            if (transform.position != lastPosition)
            {
                spline.MoveAllPoints(transform.position - lastPosition);
            }

            if (transform.rotation != lastRotation)
            {
                spline.EachPoint((point) =>
                {

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

            if (transform.position != lastPosition || transform.rotation != lastRotation)
            {
                RecordLastPosRot();
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (UnityEditor.Selection.Contains(gameObject.GetInstanceID())) return;
            else if (!gizmoSettings.drawGizmoAlways) return;

            spline.EachLine((start, end) =>
            {

                Gizmos.DrawSphere(end.position, gizmoSettings.radius * .5f);
                Gizmos.DrawSphere(start.position, gizmoSettings.radius * .5f);

                SplineHandles.DrawBezierLine(start, end, spline.segmentCount, gizmoSettings.thickness);
            });
        }
#endif
    }
}