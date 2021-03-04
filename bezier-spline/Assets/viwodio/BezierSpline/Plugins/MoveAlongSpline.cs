using UnityEngine;
using System.Collections;

namespace viwodio.BezierSpline
{
    public class MoveAlongSpline : MonoBehaviour
    {
        [SerializeField] private bool isMoving = true;
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float pointDetectDistance = .01f;
        [SerializeField] private SplineDrawer splineDrawer;

        private OrientedPoint[] points;
        private int nextPoint;


        void Start()
        {
            points = SplineUtility.MakeBezierPoints(splineDrawer.spline);
            transform.position = points[0].position;
        }

        void Update()
        {
            if (isMoving)
            {
                transform.position = Vector3.Lerp(transform.position, points[nextPoint].position, moveSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, points[nextPoint].rotation, moveSpeed * Time.deltaTime);

                float distance = Vector3.Distance(transform.position, points[nextPoint].position);
                if (distance <= pointDetectDistance)
                {
                    NextPoint();
                }
            }
        }

        private void NextPoint()
        {
            nextPoint++;

            if (nextPoint >= points.Length)
            {
                if (splineDrawer.spline.loop)
                {
                    nextPoint %= points.Length;
                }
                else
                {
                    isMoving = false;
                }
            }
        }
    }
}