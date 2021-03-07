using UnityEngine;
using System.Collections.Generic;
using System;

namespace viwodio.BezierSpline
{
    [Serializable]
    public class Spline
    {
        public delegate void LineCallback(SplinePoint startPoint, SplinePoint endPoint);
        public delegate void PointCallback(SplinePoint point);
        public delegate void InsertPointEvent(SplinePoint insertedPoint);
        public delegate void RemovePointEvent(SplinePoint splinePoint);

        public bool loop = false;
        public int segmentCount = 16;
        public OrientedPoint point;

        [SerializeField]
        private List<SplinePoint> splinePoints = new List<SplinePoint>() {
            new SplinePoint(Vector3.zero, Quaternion.identity),
            new SplinePoint(Vector3.forward * 2, Quaternion.identity)
        };

        public event InsertPointEvent onInsertPoint;
        public event RemovePointEvent onRemovePoint;

        public int PointCount
        {
            get => splinePoints.Count;
        }

        public void InsertPointLast(Vector3 position, Quaternion rotation)
        {
            InsertPoint(PointCount, position, rotation);
        }

        public void ChangeAllPointsTangentModes(TangentMode tangentMode)
        {
            EachPoint(point => point.SetTangentMode(tangentMode));
        }

        public void InsertPoint(int index, Vector3 position, Quaternion rotation)
        {
            SplinePoint splinePoint = new SplinePoint(position, rotation);
            splinePoints.Insert(index, splinePoint);
            OnInsertPoint(splinePoint);
        }

        public void RemovePoint(int index)
        {
            RemovePoint(GetSplinePointByIndex(index));
        }

        public void RemovePoint(SplinePoint splinePoint)
        {
            bool isRemoved = splinePoints.Remove(splinePoint);

            if (isRemoved)
            {
                onRemovePoint?.Invoke(splinePoint);
            }
        }

        public int IndexOf(SplinePoint splinePoint)
        {
            return splinePoints.IndexOf(splinePoint);
        }

        public void InsertPointAtStart()
        {
            SplinePoint splineFirstPoint = splinePoints[0];
            Vector3 position = splineFirstPoint.point.LocalToWorld(Vector3.forward * -2);
            Quaternion rotation = splineFirstPoint.rotation;

            InsertPoint(0, position, rotation);
        }

        private void OnInsertPoint(SplinePoint insertedPoint)
        {
            RemoveRepetitivePoints();
            onInsertPoint?.Invoke(insertedPoint);
        }

        private void RemoveRepetitivePoints()
        {
            int pointCount = PointCount;
            for (int i = 0; i < pointCount - 1; i++)
            {
                SplinePoint currentPoint = GetSplinePointByIndex(i);
                SplinePoint nextPoint = GetSplinePointByIndex(i + 1);

                if (currentPoint.position == nextPoint.position)
                {
                    RemovePoint(nextPoint);
                    RemoveRepetitivePoints();
                    return;
                }
            }
        }

        public void InsertPointAtLast()
        {
            SplinePoint splineLastPoint = splinePoints[splinePoints.Count - 1];
            Vector3 position = splineLastPoint.point.LocalToWorld(Vector3.forward * 2);
            Quaternion rotation = splineLastPoint.rotation;

            InsertPointLast(position, rotation);
        }

        public bool HasNotIndex(int index)
        {
            return index < 0 || index >= splinePoints.Count;
        }

        public SplinePoint GetSplinePointByIndex(int index)
        {
            if (loop)
            {
                index %= PointCount;
            }

            if (HasNotIndex(index))
                throw new SplinePointNotFoundException(PointCount - 1);

            return splinePoints[index];
        }

        public void Reverse()
        {
            splinePoints.Reverse();
            EachPoint((point) => point.rotation *= Quaternion.Euler(Vector3.up * 180));
        }

        public void MoveAllPoints(Vector3 moveValue)
        {
            foreach (var point in splinePoints)
            {
                point.position += moveValue;
            }
        }

        public void Subdivide(SplinePoint sp0, SplinePoint sp1)
        {
            if (IsConsecutive(sp0, sp1))
            {
                int sp0Index = splinePoints.IndexOf(sp0);
                int sp1Index = splinePoints.IndexOf(sp1);

                int pointIndex = sp0Index < sp1Index ? sp0Index : sp1Index;
                int nextPointIndex = sp0Index > sp1Index ? sp0Index : sp1Index;

                Subdivide(pointIndex, nextPointIndex);
            }
        }

        public void Subdivide(int pointIndex, int nextPointIndex)
        {
            if (nextPointIndex - pointIndex != 1) return;

            SplinePoint splinePoint = GetSplinePointByIndex(pointIndex);
            SplinePoint nextSplinePoint = GetSplinePointByIndex(nextPointIndex);
            OrientedPoint directionalPoint = SplineUtility.Interpolation(splinePoint, nextSplinePoint, .5f);

            InsertPoint(nextPointIndex, directionalPoint.position, directionalPoint.rotation);
        }

        public bool IsConsecutive(SplinePoint firstPoint, SplinePoint secondPoint)
        {
            int firstPointIndex = splinePoints.IndexOf(firstPoint);
            int secondPointIndex = splinePoints.IndexOf(secondPoint);

            if (firstPointIndex == -1 || secondPointIndex == -1) return false;

            return Mathf.Abs(firstPointIndex - secondPointIndex) == 1;
        }

        public void EachLine(LineCallback callback)
        {
            int pointCount = splinePoints.Count;

            if (!loop) pointCount--;

            for (int i = 0; i < pointCount; i++)
            {
                SplinePoint startPoint = GetSplinePointByIndex(i);
                SplinePoint endPoint = GetSplinePointByIndex(i + 1);
                callback?.Invoke(startPoint, endPoint);
            }
        }

        public void EachPoint(PointCallback callback)
        {
            for (int i = 0; i < PointCount; i++)
            {
                callback?.Invoke(GetSplinePointByIndex(i));
            }
        }

        public Vector3[] GetSplinePointPositions()
        {
            Vector3[] positions = new Vector3[splinePoints.Count];

            for (int i = 0; i < positions.Length; i++)
                positions[i] = GetSplinePointByIndex(i).position;

            return positions;
        }

        public float GetLength()
        {
            return SplineUtility.SplineLength(this);
        }
    }
}