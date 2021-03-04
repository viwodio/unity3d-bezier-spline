using UnityEngine;
using System.Collections;
using System;

namespace viwodio.BezierSpline
{
    public class SplinePointNotFoundException : Exception
    {
        public SplinePointNotFoundException() : base()
        {

        }

        public SplinePointNotFoundException(int maxIndexValue) :
            base("Spline point not found. Enter a value between 0 and " + maxIndexValue)
        {
        }
    }
}