using UnityEngine;
using System.Collections;
using UnityEditor;

namespace viwodio.BezierSpline
{
    public class SplineHandles
    {
        public static Color lineColor = Color.white;
        public static Color pointColor = Color.green;
        public static Color tangentPointColor = Color.cyan;
        public static Color tangentLineColor = Color.cyan;

        private static readonly Vector3 arrowLeftPoint = new Vector3(-1, 0, -1).normalized;
        private static readonly Vector3 arrowRightPoint = new Vector3(1, 0, -1).normalized;

        public static void DrawDirectionalBezierLine(SplinePoint start, SplinePoint end, int segmentCount, float arrowWidth = .2f, float thickness = 2f)
        {
            using (new Handles.DrawingScope(lineColor))
            {
                OrientedPoint[] bezierPoints = SplineUtility.MakeBezierPoints(start, end, segmentCount);

                for (int i = 0; i < bezierPoints.Length - 1; i++)
                {
                    Vector3 startPosition = bezierPoints[i].position;
                    Vector3 endPosition = bezierPoints[i + 1].position;
                    Handles.DrawAAPolyLine(thickness, startPosition, endPosition);
                }

                OrientedPoint arrowPoint = SplineUtility.Interpolation(start, end, .5f);//SplineUtility.MakeBezierPoints(start, end, arrowCount);
                Handles.DrawAAPolyLine(
                    arrowPoint.LocalToWorld(arrowLeftPoint * arrowWidth),
                    arrowPoint.position,
                    arrowPoint.LocalToWorld(arrowRightPoint * arrowWidth));
            }
        }

        public static void DrawBezierLine(SplinePoint start, SplinePoint end, int segmentCount, float thickness = 2f)
        {
            using (new Handles.DrawingScope(lineColor))
            {
                OrientedPoint[] bezierPoints = SplineUtility.MakeBezierPoints(start, end, segmentCount);

                for (int i = 0; i < bezierPoints.Length - 1; i++)
                {
                    Vector3 startPosition = bezierPoints[i].position;
                    Vector3 endPosition = bezierPoints[i + 1].position;
                    Handles.DrawAAPolyLine(thickness, startPosition, endPosition);
                }
            }            
        }

        public static bool DrawTangentsButton(SplinePoint point, float radius = .2f, float thickness = 2f)
        {
            bool isClicked = false;

            if (point.TangentMode != TangentMode.Linear)
            {
                using (new Handles.DrawingScope(tangentLineColor))
                {
                    Handles.DrawAAPolyLine(thickness, point.leftTangent, point.position, point.rightTangent);
                }

                using (new Handles.DrawingScope(tangentPointColor))
                {
                    int controlID = GUIUtility.GetControlID(point.GetHashCode(), FocusType.Passive);

                    if (EventTypeIsLayoutOrRepaint())
                    {
                        Handles.SphereHandleCap(controlID, point.leftTangent, Quaternion.identity, radius, Event.current.type);
                        Handles.SphereHandleCap(controlID, point.rightTangent, Quaternion.identity, radius, Event.current.type);
                    }
                    else
                    {
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            if (HandleUtility.nearestControl == controlID)
                            {
                                isClicked = true;
                            }
                        }
                    }
                }
            }           

            return isClicked;
        }

        public static bool DrawPointButton(SplinePoint point, float radius = .2f)
        {
            bool isClicked = false;

            using (new Handles.DrawingScope(pointColor))
            {
                int controlID = GUIUtility.GetControlID(point.GetHashCode(), FocusType.Passive);

                if (EventTypeIsLayoutOrRepaint())
                {
                    Handles.SphereHandleCap(controlID, point.position, Quaternion.identity, radius, Event.current.type);
                }
                else
                {
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        if (HandleUtility.nearestControl == controlID)
                        {
                            isClicked = true;
                        }
                    }
                }
            }

            return isClicked;
        }

        public static Vector3 PositionHandle(Vector3 position, Quaternion rotation)
        {
            if (Tools.pivotRotation == PivotRotation.Global)
            {
                return Handles.PositionHandle(position, Quaternion.identity);
            }
            else
            {   
                return Handles.PositionHandle(position, rotation);
            }
        }

        public static bool EventTypeIsLayoutOrRepaint()
        {
            return Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint;
        }
    }
}