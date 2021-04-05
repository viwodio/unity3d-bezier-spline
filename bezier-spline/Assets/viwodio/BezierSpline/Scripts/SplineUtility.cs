using UnityEngine;
using System.Collections.Generic;

namespace viwodio.BezierSpline
{
    public class SplineUtility
    {
        public static float BezierLength(Vector3 startPos, Vector3 startTangent, Vector3 endTangent, Vector3 endPos, int segmentCount)
        {
            float length = 0;
            OrientedPoint[] points = MakeBezierPoints(startPos, startTangent, endTangent, endPos, segmentCount);

            for (int i = 0; i < points.Length - 1; i++)
                length += Vector3.Distance(points[i].position, points[i + 1].position);

            return length;
        }

        public static OrientedPoint[] MakeBezierPoints(Vector3 startPos, Vector3 startTangent, Vector3 endTangent, Vector3 endPos, int segmentCount)
        {
            segmentCount++;

            OrientedPoint[] points = new OrientedPoint[segmentCount];

            for (int i = 0; i < points.Length; i++)
            {
                float t = (float)i / (points.Length - 1);
                points[i] = Interpolation(startPos, startTangent, endTangent, endPos, t);
            }

            return points;
        }

        public static OrientedPoint Interpolation(Vector3 startPos, Vector3 startTangent, Vector3 endTangent, Vector3 endPos, float t)
        {
            Vector3 a = Vector3.Lerp(startPos, startTangent, t);
            Vector3 b = Vector3.Lerp(startTangent, endTangent, t);
            Vector3 c = Vector3.Lerp(endTangent, endPos, t);

            Vector3 d = Vector3.Lerp(a, b, t);
            Vector3 e = Vector3.Lerp(b, c, t);

            Vector3 position = Vector3.Lerp(d, e, t);
            Vector3 forward = (e - d).normalized;
            Quaternion rotation = Quaternion.LookRotation(forward);

            return new OrientedPoint(position, rotation);
        }
    }
}