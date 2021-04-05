using System;
using System.Linq;
using UnityEngine;

namespace viwodio.BezierSpline.Component
{
    public class MoveAlongSpline : MonoBehaviour
    {
        public float moveSpeed = 2f;
        public float lookSpeed = 1f;

        [SerializeField] private SplineDrawer splineDrawer;

        public event Action onSplineFinish;
        public bool isMove;

        private OrientedPoint[] waypoints;
        private int targetWaypointIndex;
        private OrientedPoint targetWaypoint => waypoints[targetWaypointIndex];

        void Awake()
        {
            if (splineDrawer != null)
                SetSplineDrawer(splineDrawer);
            else
                isMove = false;
        }

        public void SetSplineDrawer(SplineDrawer splineDrawer)
        {
            this.splineDrawer = splineDrawer;
            targetWaypointIndex = 0;
            GenerateWaypoints();
        }

        private void GenerateWaypoints()
        {
            waypoints = splineDrawer.spline.MakeBezierPointsBySegments();
        }

        void Update()
        {
            if (isMove)
            {
                if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
                {
                    ReachedTargetPoint();
                }

                transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, moveSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetWaypoint.rotation, lookSpeed * Time.deltaTime);
            }
        }

        public void JumpToStartPoint()
        {
            targetWaypointIndex = 0;
            JumpToTargetPoint();
        }

        public void JumpToTargetPoint()
        {
            transform.position = targetWaypoint.position;
            transform.rotation = targetWaypoint.rotation;
        }

        private void ReachedTargetPoint()
        {
            if (targetWaypointIndex == waypoints.Length - 1)
            {
                if (!splineDrawer.spline.loop)
                {
                    isMove = false;
                    onSplineFinish?.Invoke();
                    return;
                }

                onSplineFinish?.Invoke();
            }

            targetWaypointIndex += 1;
            targetWaypointIndex %= waypoints.Length;
        }
    }
}