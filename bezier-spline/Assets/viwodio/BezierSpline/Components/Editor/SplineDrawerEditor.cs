using UnityEngine;
using UnityEditor;
using System;

namespace viwodio.BezierSpline.Component
{
    [CustomEditor(typeof(SplineDrawer))]
    public class SplineDrawerEditor : Editor
    {
        private static event Action OnScriptReload;

        private SplineDrawer splineDrawer;
        private int selectedPointIndex;
        private SerializedProperty propGizmoSettings;

        private static bool showTangentHandles = false;
        private static InsertType insertType = InsertType.Raycast;
        private static bool isExpandedSplineSettings = false;

        private SplinePoint GetSelectedPoint()
        {
            return splineDrawer.spline.GetSplinePointByIndex(selectedPointIndex);
        }

        void OnEnable()
        {
            splineDrawer = target as SplineDrawer;
            propGizmoSettings = serializedObject.FindProperty(nameof(splineDrawer.gizmoSettings));
            SelectPoint(selectedPointIndex, false);
            OnScriptReload += OnReload;
        }

        void OnDisable()
        {
            OnScriptReload -= OnReload; 
        }

        private void OnReload()
        {
            SelectPoint(selectedPointIndex, false);
        }

        private void SelectPoint(int index, bool focus)
        {
            selectedPointIndex = Mathf.Clamp(index, 0, splineDrawer.spline.PointCount - 1);

            if (focus)
            {
                FocusPoint(GetSelectedPoint());
            }

            Repaint();
            SceneView.RepaintAll();
        }

        private void FocusSelectedPoint()
        {
            FocusPoint(GetSelectedPoint());
        }

        private void FocusPoint(SplinePoint point)
        {
            if (point == null) return;

            Bounds bounds = new Bounds();
            bounds.center = point.position;
            bounds.size = Vector3.one * splineDrawer.gizmoSettings.radius * 10;
            SceneView.lastActiveSceneView.Frame(bounds, false);
        }

        private void OnSceneGUI()
        {
            splineDrawer.spline.EachLine((start, end) => {

                SplineHandles.DrawDirectionalBezierLine(
                    start,
                    end,
                    splineDrawer.spline.segmentCount,
                    splineDrawer.gizmoSettings.radius,
                    splineDrawer.gizmoSettings.thickness);

            });

            splineDrawer.spline.EachPointWithIndex((point, index) => {

                bool pointIsClicked = SplineHandles.DrawPointButton(point, splineDrawer.gizmoSettings.radius);
                bool tangentIsClicked = SplineHandles.DrawTangentsButton(point, splineDrawer.gizmoSettings.radius, splineDrawer.gizmoSettings.thickness);

                if (pointIsClicked || tangentIsClicked)
                {
                    SelectPoint(index, false);
                }

            });

            DrawSelectedPointSceneGUI();
            InsertPoint();
            Shortcuts();
        }

        private void DrawSelectedPointSceneGUI()
        {
            SplinePoint selectedPoint = GetSelectedPoint();
            if (selectedPoint == null) return;

            EditorGUI.BeginChangeCheck();
            Vector3 pointPosition = selectedPoint.position;
            Quaternion pointRotation = selectedPoint.rotation;

            if (Tools.current == Tool.Rotate)
            {
                pointRotation = Handles.RotationHandle(selectedPoint.rotation, selectedPoint.position);
            }
            else
            {
                pointPosition = SplineHandles.PositionHandle(selectedPoint.position, selectedPoint.rotation);
            }

            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(splineDrawer, "Spline Point Transform Change");
                selectedPoint.position = pointPosition;
                selectedPoint.rotation = pointRotation;
            }

            if (!splineDrawer.gizmoSettings.showTangentPointAlways)
            {
                SplineHandles.DrawTangentsButton(selectedPoint,
                    splineDrawer.gizmoSettings.radius,
                    splineDrawer.gizmoSettings.thickness);
            }

            if (showTangentHandles)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 leftTangentPosition = SplineHandles.PositionHandle(selectedPoint.leftTangent, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(splineDrawer, "Tangent Position Change");
                    selectedPoint.leftTangent = leftTangentPosition;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 rightTangentPosition = SplineHandles.PositionHandle(selectedPoint.rightTangent, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(splineDrawer, "Tangent Position Change");
                    selectedPoint.rightTangent = rightTangentPosition;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawGizmoSettingsGUI();
            DrawSplineSettingsGUI();
            DrawInsertSettingsGUI();
            DrawSelectedPointIndexGUI();
            DrawSelectedPointGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGizmoSettingsGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Space(12);
                EditorGUILayout.PropertyField(propGizmoSettings);
            }
        }

        private void DrawSplineSettingsGUI()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(12);
                    isExpandedSplineSettings = EditorGUILayout.Foldout(isExpandedSplineSettings, "Spline Settings", true);
                }

                if (!isExpandedSplineSettings) return;

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(26);
                    using (new EditorGUILayout.VerticalScope())
                    {
                        DrawSplineLoopToggleGUI();
                        DrawSegmentCountGUI();

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reverse/Flip"))
                        {
                            ReverseSpline();
                        }

                        if (GUILayout.Button("Reset"))
                        {
                            ResetSpline();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Separator();

                        EditorGUILayout.LabelField("Change All Points Tangent Modes", EditorStyles.boldLabel);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("Mirrored"))
                            {
                                ChangeAllPointsTangentModes(TangentMode.Mirrored);
                            }
                            if (GUILayout.Button("Linear"))
                            {
                                ChangeAllPointsTangentModes(TangentMode.Linear);
                            }
                            if (GUILayout.Button("Free"))
                            {
                                ChangeAllPointsTangentModes(TangentMode.Free);
                            }
                        }
                    }
                }                
            }
        }

        private void DrawInsertSettingsGUI()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Insert Settings", EditorStyles.boldLabel);
                insertType = (InsertType)EditorGUILayout.EnumPopup("Insert Type", insertType);
                EditorGUILayout.LabelField("use Cmd+G (⌘G) for insert point to mouse position while scene is active", EditorStyles.wordWrappedMiniLabel);
            }
        }

        private void InsertPoint()
        {
            if (Event.current.type != EventType.KeyDown) return;
            if (Event.current.keyCode != KeyCode.G) return;
            if (!Event.current.command) return;

            Vector3? position = MouseToWorldPosition();

            if (!position.HasValue) return;

            Undo.RecordObject(splineDrawer, "Insert Point");
            splineDrawer.spline.InsertPointLast(position.Value, Quaternion.identity);
            SelectPoint(splineDrawer.spline.PointCount - 1, false);
            Event.current.Use();
        }

        private Vector3? MouseToWorldPosition()
        {
            Vector3 mousePos = HandleUtility.GUIPointToScreenPixelCoordinate(Event.current.mousePosition);
            Camera cam = Camera.current;

            if (insertType == InsertType.Raycast)
            {
                Ray ray = cam.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    return hit.point;
                }
                else
                {
                    return null;
                }
            }

            else if (insertType == InsertType.ScreenToWorld)
            {
                Vector3 dir = splineDrawer.transform.position - cam.transform.position;
                float angle = Vector3.Angle(dir, cam.transform.forward);
                float splineDepth = Mathf.Cos(angle * Mathf.Deg2Rad) * dir.magnitude;

                mousePos.z = splineDepth;

                Vector3 position = cam.ScreenToWorldPoint(mousePos);
                return position;
            }

            return null;
        }

        private void DrawSplineLoopToggleGUI()
        {
            EditorGUI.BeginChangeCheck();
            bool loop = EditorGUILayout.Toggle("Loop", splineDrawer.spline.loop);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(splineDrawer, "Change Segment Count");
                splineDrawer.spline.loop = loop;
                SceneView.RepaintAll();
            }
        }

        private void DrawSegmentCountGUI()
        {
            EditorGUI.BeginChangeCheck();
            int segmentCount = EditorGUILayout.IntSlider("Segments", splineDrawer.spline.segmentCount, 4, 128);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(splineDrawer, "Change Segment Count");
                splineDrawer.spline.segmentCount = segmentCount;
                SceneView.RepaintAll();
            }
        }

        private void DrawSelectedPointIndexGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUI.BeginChangeCheck();
                int index = EditorGUILayout.IntField("Selected Point", selectedPointIndex);
                if (EditorGUI.EndChangeCheck())
                {
                    SelectPoint(index, false);
                }
            }
        }

        private void DrawTangentHandlesToggleGUI()
        {
            if (GUILayout.Button((showTangentHandles ? "Hide" : "Show") + " Tangent Move Handles (Space)"))
            {
                TangentHandlesToggle();
            }
        }

        private void TangentHandlesToggle()
        {
            showTangentHandles = !showTangentHandles;
            Repaint();
            SceneView.RepaintAll();
        }

        private void DrawSelectedPointGUI()
        {
            SplinePoint selectedPoint = GetSelectedPoint();
            if (selectedPoint == null) return;

            Shortcuts();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Selected Point Settings", EditorStyles.boldLabel);

                DrawPointHandles(selectedPoint);
                DrawTangentModeGUI(selectedPoint);
                DrawTangentHandlesToggleGUI();

                if (GUILayout.Button("Remove Selected Point (⌘⇧D)"))
                {
                    RemoveSelectedPoint();
                }

                if (GUILayout.Button("Focus Selected Point (⌘F)"))
                {
                    FocusSelectedPoint();
                }

                if (GUILayout.Button("Select Next Point (⌘→)"))
                {
                    SelectNextPoint();
                }

                if (GUILayout.Button("Select Previous Point (⌘←)"))
                {
                    SelectPreviousPoint();
                }

                if (GUILayout.Button("Subdivide (With Next) (⌘R)"))
                {
                    SubdivideBetweenNextPoint();
                }

                if (GUILayout.Button("Subdivide (With Previous) (⌘⇧R)"))
                {
                    SubdivideBetweenPreviousPoint();
                }
            }
        }

        private void RemoveSelectedPoint()
        {
            RemovePoint(selectedPointIndex);
        }

        private void RemovePoint(int index)
        {
            Undo.RecordObject(splineDrawer, "Remove Point");
            splineDrawer.spline.RemovePoint(index);
            SelectPoint(Mathf.Clamp(index, 0, splineDrawer.spline.PointCount - 1), false);
        }

        private void DrawPointHandles(SplinePoint point)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 position = EditorGUILayout.Vector3Field("Position", point.position);
            Vector3 rotation = EditorGUILayout.Vector3Field("Rotation", point.rotation.eulerAngles);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(splineDrawer, "Spline Point Transform");
                point.position = position;
                point.rotation = Quaternion.Euler(rotation);
                SceneView.RepaintAll();
            }
        }

        private void DrawTangentModeGUI(SplinePoint point)
        {
            EditorGUI.BeginChangeCheck();
            TangentMode tangentMode = (TangentMode)EditorGUILayout.EnumPopup("Tangent Mode", point.TangentMode);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(splineDrawer, "Spline Point Settings");
                point.SetTangentMode(tangentMode);
                SceneView.RepaintAll();
            }
        }

        public void ResetSpline()
        {
            Undo.RecordObject(splineDrawer, "Reset Spline");
            splineDrawer.spline = new Spline();
            SelectPoint(0, false);
        }

        private void ChangeAllPointsTangentModes(TangentMode tangentMode)
        {
            Undo.RecordObject(splineDrawer, "Change Tangent Points");
            splineDrawer.spline.ChangeAllPointsTangentModes(tangentMode);
            Repaint();
            SceneView.RepaintAll();
        }

        private void SelectNextPoint()
        {
            SelectPoint(selectedPointIndex + 1, false);
        }

        private void SelectPreviousPoint()
        {
            SelectPoint(selectedPointIndex - 1, false);
        }

        private void ReverseSpline()
        {
            Undo.RecordObject(this, "Spline Reverse");
            splineDrawer.spline.Reverse();
            SceneView.RepaintAll();
        }

        private void SubdivideBetweenNextPoint()
        {
            Undo.RecordObject(splineDrawer, "Subdivide");
            splineDrawer.spline.Subdivide(selectedPointIndex, selectedPointIndex + 1);
            SelectPoint(selectedPointIndex + 1, false);
        }

        private void SubdivideBetweenPreviousPoint()
        {
            Undo.RecordObject(splineDrawer, "Subdivide");
            splineDrawer.spline.Subdivide(selectedPointIndex - 1, selectedPointIndex);
            SelectPoint(selectedPointIndex, false);
        }

        private void Shortcuts()
        {
            if(Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.F)
                {
                    FocusSpline();
                    Event.current.Use();
                }

                if (Event.current.keyCode == KeyCode.Space)
                {
                    TangentHandlesToggle();
                    Event.current.Use();
                }

                if (Event.current.command)
                {
                    if (Event.current.keyCode == KeyCode.F)
                    {
                        FocusSelectedPoint();
                        Event.current.Use();
                    }

                    if (Event.current.keyCode == KeyCode.RightArrow)
                    {
                        SelectNextPoint();
                        Event.current.Use();
                    }

                    if (Event.current.keyCode == KeyCode.LeftArrow)
                    {
                        SelectPreviousPoint();
                        Event.current.Use();
                    }

                    if (Event.current.keyCode == KeyCode.R)
                    {
                        if (Event.current.shift)
                        {
                            SubdivideBetweenPreviousPoint();
                        }
                        else
                        {
                            SubdivideBetweenNextPoint();
                        }

                        Event.current.Use();
                    }


                    if (Event.current.keyCode == KeyCode.D)
                    {
                        if (Event.current.shift)
                        {
                            RemoveSelectedPoint();
                            Event.current.Use();
                        }
                    }
                }
            }
        }

        private void FocusSpline()
        {
            Vector3[] positions = splineDrawer.spline.GetSplinePointPositions();
            Bounds bounds = GeometryUtility.CalculateBounds(positions, splineDrawer.transform.localToWorldMatrix);
            SceneView.lastActiveSceneView.Frame(bounds, false);
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnReloadEvent()
        {
            OnScriptReload?.Invoke();
        }
    }
}