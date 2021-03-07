using System;
using System.Linq;
using UnityEngine;

namespace viwodio.BezierSpline.Component
{
    public class MoveAlongSpline : MonoBehaviour
    {
        public float moveSpeed = 2.5f;
        public float lookSpeed = 8f;
        [SerializeField] private SplineDrawer splineDrawer;

        public event Action onFinishPoint;
        public bool IsMove => this.enabled;

        private OrientedPoint[] waypoints;
        private int targetWaypointIndex;
        private OrientedPoint targetWaypoint => waypoints[targetWaypointIndex];

        void Awake()
        {
            waypoints = SplineUtility.MakeBezierPoints(splineDrawer.spline);
            JumpToTarget();
        }

        void Update()
        {
            Move();
        }

        public void Move()
        {
            if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
                ReachedTargetPoint();

            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetWaypoint.rotation, lookSpeed * Time.deltaTime);
        }

        public void StartMove()
        {
            enabled = true;
        }

        public void StopMove()
        {
            enabled = false;
        }

        public void Reset()
        {
            targetWaypointIndex = 0;
            StopMove();
        }

        public void JumpToTarget()
        {
            transform.position = targetWaypoint.position;
            transform.rotation = targetWaypoint.rotation;
        }

        private void ReachedTargetPoint()
        {
            if (targetWaypointIndex == waypoints.Length - 1)
            {
                onFinishPoint?.Invoke();

                if (!splineDrawer.spline.loop)
                {
                    StopMove();
                    return;
                }
            }

            targetWaypointIndex += 1;
            targetWaypointIndex %= waypoints.Length;
        }
    }
}