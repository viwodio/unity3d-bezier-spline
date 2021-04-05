using System.Linq;
using UnityEngine;

namespace viwodio.BezierSpline.Component
{
    [RequireComponent(typeof(LineRenderer))]
    public class SplineRenderer : MonoBehaviour
    {
        [SerializeField] private SplineDrawer splineDrawer;
        [SerializeField] private Vector3 offset = Vector3.up * 0.01f;

        private LineRenderer _lineRenderer;
        private LineRenderer lineRenderer
        {
            get
            {
                if (_lineRenderer == null)
                    _lineRenderer = GetComponent<LineRenderer>();

                return _lineRenderer;
            }
        }

        void OnEnable()
        {
            splineDrawer.spline.onInsertPoint += OnInsertPoint;
            splineDrawer.spline.onRemovePoint += OnRemovePoint;

            UpdateLine();
        }

        private void OnRemovePoint(SplinePoint splinePoint)
        {
            UpdateLine();
        }

        private void OnInsertPoint(SplinePoint insertedPoint)
        {
            UpdateLine();
        }

        void OnDisable()
        {
            splineDrawer.spline.onInsertPoint -= OnInsertPoint;
            splineDrawer.spline.onRemovePoint -= OnRemovePoint;
        }

        [ContextMenu("Update Line")]
        private void UpdateLine()
        {
            OrientedPoint[] points = splineDrawer.spline.MakeBezierPointsBySegments();
            lineRenderer.positionCount = points.Length;

            for (int i = 0; i < points.Length; i++)
                lineRenderer.SetPosition(i, points[i].LocalToWorld(offset));
        }
    }
}