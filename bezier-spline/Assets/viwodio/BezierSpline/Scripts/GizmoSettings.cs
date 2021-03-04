using UnityEngine;
using System.Collections;

namespace viwodio.BezierSpline
{
    [System.Serializable]
    public class GizmoSettings
    {
        public bool drawGizmoAlways = true;
        public bool showTangentPointAlways = true;

        [Range(0.01f, 2f)] public float radius = .1f;
        [Range(0.01f, 10f)] public float thickness = 2f;
    }
}