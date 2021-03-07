using UnityEngine;
using System.Collections;

namespace viwodio.BezierSpline
{
    public class MoveAlongSpline : MonoBehaviour
    {
        [SerializeField] private bool isMoving = true;
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private SplineDrawer splineDrawer;

        float t = 0;

        int previousIndex = -1;
        SplinePoint previousPoint;
        SplinePoint nextPoint;
        float currentPathLength = 0;

        void Awake()
        {
            OnPointPassed();
        }

        void Update()
        {
            if (isMoving)
            {
                OrientedPoint point = SplineUtility.Interpolation(previousPoint, nextPoint, t);

                transform.position = point.position;
                transform.rotation = point.rotation;

                t += moveSpeed / currentPathLength * Time.deltaTime;

                if (t >= 1f)
                {
                    OnPointPassed();
                }
            }
        }

        void OnPointPassed()
        {
            t %= 1f;
            previousIndex++;

            if (!splineDrawer.spline.loop)
            {
                if (previousIndex >= splineDrawer.spline.PointCount - 1)
                {
                    isMoving = false;
                    return;
                }
            }
            else
            {
                previousIndex %= splineDrawer.spline.PointCount;
            }

            previousPoint = splineDrawer.spline.GetSplinePointByIndex(previousIndex);
            nextPoint = splineDrawer.spline.GetSplinePointByIndex(previousIndex + 1);
            currentPathLength = SplineUtility.SplineLength(previousPoint, nextPoint, splineDrawer.spline.segmentCount);
        }
    }
}