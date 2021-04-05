using UnityEngine;
using System.Collections;
using UnityEditor;

namespace viwodio.BezierSpline.Component
{
    [CustomEditor(typeof(GenerateAlongSpline))]
    public class GenerateAlongSplineEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate"))
            {
                (target as GenerateAlongSpline).Generate();
            }
        }
    }
}
