using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace viwodio.BezierSpline.Component
{
    public class GenerateAlongSpline : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private SplineDrawer splineDrawer;
        [SerializeField] private float distance = 1f;

        [SerializeField, HideInInspector] private List<GameObject> instances = new List<GameObject>();

        public GameObject[] GetInstances()
        {
            return instances.ToArray();
        }

        void OnValidate()
        {
            distance = Mathf.Max(0.001f, distance);
        }

        public void Clear()
        {
            foreach (var instance in instances)
            {
                if (Application.isEditor && !Application.isPlaying)
                {
#if UNITY_EDITOR
                    DestroyImmediate(instance);
#endif
                }
                else
                {
                    Destroy(instance);
                }
            }

            instances.Clear();
        }

        public void Generate()
        {
            if (splineDrawer == null) return;
            if (prefab == null) return;

            Clear();

            OrientedPoint[] instancePoints = splineDrawer.spline.MakeBezierPointsByDistance(distance);

            foreach (var point in instancePoints)
            {
                GameObject instance = null;

                if (Application.isEditor && !Application.isPlaying)
                {
#if UNITY_EDITOR
                    instance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform) as GameObject;
#endif
                }
                else
                {
                    instance = Instantiate(prefab, transform);
                }

                instance.transform.position = point.position;
                instance.transform.rotation = point.rotation;
                instances.Add(instance);
            }
        }
    }
}