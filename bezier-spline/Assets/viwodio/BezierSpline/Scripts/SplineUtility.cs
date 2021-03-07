using UnityEngine;
using System.Collections.Generic;

namespace viwodio.BezierSpline
{
    public class SplineUtility
    {
        public static float SplineLength(Spline spline)
        {
            float length = 0;

            spline.EachLine((start, end) => {
                length += SplineLength(start, end, spline.segmentCount);
            });

            return length;
        }

        public static float SplineLength(SplinePoint startPoint, SplinePoint endPoint, int segmentCount)
        {
            float length = 0;
            OrientedPoint[] points = MakeBezierPoints(startPoint, endPoint, segmentCount);

            for (int i = 0; i < points.Length - 1; i++)
            {
                length += Vector3.Distance(points[i].position, points[i + 1].position);
            }

            return length;
        }

        public static OrientedPoint[] MakeBezierPoints(Spline spline)
        {
            return MakeBezierPoints(spline, spline.segmentCount);
        }

        public static OrientedPoint[] MakeBezierPoints(Spline spline, int segmentCount)
        {
            List<OrientedPoint> points = new List<OrientedPoint>();
            spline.EachLine((start, end) => {
                points.AddRange(MakeBezierPoints(start, end, segmentCount));
            });

            // Listeden çıkarmak için yinelenen noktaları buluyoruz
            for (int i = segmentCount; i < points.Count - 1; i += segmentCount)
            {
                points.RemoveAt(i);
            }

            return points.ToArray();
        }

        public static OrientedPoint[] MakeBezierPoints(SplinePoint startPoint, SplinePoint endPoint, int segmentCount)
        {
            segmentCount++;

            OrientedPoint[] points = new OrientedPoint[segmentCount];

            for (int i = 0; i < points.Length; i++)
            {
                float t = (float)i / (points.Length - 1);
                points[i] = Interpolation(startPoint, endPoint, t);
            }

            return points;
        }

        public static OrientedPoint Interpolation(SplinePoint startPoint, SplinePoint endPoint, float t)
        {
            Vector3 startPosition = startPoint.position;
            Vector3 startTangent = startPoint.rightTangent;

            if (startPoint.TangentMode == TangentMode.Linear)
                startTangent = startPoint.GetLinearRightTangent(endPoint);

            Vector3 endPosition = endPoint.position;
            Vector3 endTangent = endPoint.leftTangent;

            if (endPoint.TangentMode == TangentMode.Linear)
                endTangent = endPoint.GetLinearLeftTangent(startPoint);

            Vector3 a = Vector3.Lerp(startPosition, startTangent, t);
            Vector3 b = Vector3.Lerp(startTangent, endTangent, t);
            Vector3 c = Vector3.Lerp(endTangent, endPosition, t);

            Vector3 d = Vector3.Lerp(a, b, t);
            Vector3 e = Vector3.Lerp(b, c, t);

            Vector3 position = Vector3.Lerp(d, e, t);
            Vector3 forward = (e - d).normalized;
            Quaternion rotation = Quaternion.LookRotation(forward);

            return new OrientedPoint(position, rotation);
        }
    }
}