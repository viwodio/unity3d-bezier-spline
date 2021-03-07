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
        float splineLength = 0;

        void Awake()
        {
            splineLength = splineDrawer.spline.GetLength();
        }

        void Update()
        {
            if (isMoving)
            {
                OrientedPoint point = SplineUtility.Interpolation(splineDrawer.spline, t);

                transform.position = point.position;
                transform.rotation = point.rotation;

                t += moveSpeed / splineLength * Time.deltaTime;

                if (t >= 1f)
                {
                    if (splineDrawer.spline.loop)
                    {
                        t %= 1f;
                    }
                    else
                    {
                        isMoving = false;
                    }
                }
            }
        }
    }
}