using UnityEditor;
using UnityEngine;
using System;
using CustomVR;

public enum ProxyMode { Free, Aligned, Mirrored, Linear }

[CustomEditor(typeof(SplineComponent))]
public class SplineEditor : Editor
{
    SplineComponent spline;
    Transform transform;
    Quaternion rotation;

    Color[] modeColors =
    {
        Color.green,
        Color.white,
        Color.cyan,
        Color.gray,
        Color.red
    };

    bool[] segments = new bool[] { true, true };

    int directionLines = 0;
    float directionScale = 0.07f;

    float handleSize = 0.04f;
    float picksize = 0.07f;
    int selectedIndex = -1;

    private void OnSceneGUI()
    {
        if(spline == null)
            spline = target as SplineComponent;

        transform = spline.transform;
        rotation = Tools.pivotRotation == PivotRotation.Local ? 
            transform.rotation : Quaternion.identity;

        Vector3 p0 = ShowPoint(0);
        spline.SetControlPoint(0, transform.InverseTransformPoint(p0));
        for (int i = 1; i < spline.PointCount; i += 3)
        {
            Vector3 p1 = ShowPoint(i);
            Vector3 p2 = ShowPoint(i + 1);
            Vector3 p3 = ShowPoint(i + 2);
            
            Handles.color = Color.gray;
            if (spline.GetPointMode(i) != PointMode.Linear)
                Handles.DrawLine(p0, p1);
            if (spline.GetPointMode(i + 1) != PointMode.Linear)
                Handles.DrawLine(p2, p3);

            spline.SetControlPoint(i, transform.InverseTransformPoint(p1));
            spline.SetControlPoint(i + 1, transform.InverseTransformPoint(p2));
            spline.SetControlPoint(i + 2, transform.InverseTransformPoint(p3));

            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 5);

            p0 = p3;
        }

        ShowDirections();
    }

    public override void OnInspectorGUI()
    {
        if (spline == null)
            spline = target as SplineComponent;

        EditorGUI.BeginChangeCheck();


        int segmentCount = (spline.PointCount - 1) / 3;
        if(segmentCount != segments.Length)
            Array.Resize(ref segments, segmentCount);

        directionLines = EditorGUILayout.IntSlider("Show Direction Vectors", directionLines, 0, 20);
        if (directionLines > 0)
            directionScale = EditorGUILayout.Slider("Direction Vector Scale", directionScale, 0, 1);

        Undo.RecordObject(spline, "Loop");
        spline.PathMode = (PathMode)EditorGUILayout.EnumPopup("Mode", spline.PathMode);
        spline.spline.LutResolution = EditorGUILayout.IntField("LUT Resolution", spline.spline.LutResolution);
        EditorGUILayout.Space();

        EditorGUI.indentLevel++;
        ProxyMode mode;
        for (int i = 3; i < spline.PointCount; i ++)
        {
            int segment = (i / 3) - 1;
            if (i % 3 == 0)
            {
                segments[segment] = EditorGUILayout.Foldout(segments[segment], "Segment " + segment, true);

                if (segments[segment])
                {
                    Undo.RecordObject(spline, "Segment Changed");
                    if (spline.GetPointMode(i) != PointMode.Linear)
                    {
                        mode = (ProxyMode)spline.GetPointMode(i - 3);
                        mode = (ProxyMode)EditorGUILayout.EnumPopup(mode);
                        spline.SetPointMode(i - 3, (PointMode)mode);

                        EditorGUILayout.LabelField("Element " + (i - 3));
                        spline.SetControlPoint(i - 3, EditorGUILayout.Vector3Field("", spline.GetControlPoint(i - 3)));

                        EditorGUILayout.LabelField("Element " + (i - 2));
                        spline.SetControlPoint(i - 2, EditorGUILayout.Vector3Field("", spline.GetControlPoint(i - 2)));

                        EditorGUILayout.LabelField("Element " + (i - 1));
                        spline.SetControlPoint(i - 1, EditorGUILayout.Vector3Field("", spline.GetControlPoint(i - 1)));
                    }

                    EditorGUILayout.LabelField("Element " + i);
                    spline.SetControlPoint(i, EditorGUILayout.Vector3Field("", spline.GetControlPoint(i)));

                    mode = (ProxyMode)spline.GetPointMode(i);
                    mode = (ProxyMode)EditorGUILayout.EnumPopup(mode);
                    spline.SetPointMode(i, (PointMode)mode);
                }
            }
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        if (GUILayout.Button("Add Segment"))
        {
            Undo.RecordObject(spline, "Segment added");
            spline.AddSegment();
            Array.Resize(ref segments, segments.Length + 1);
            segments[segments.Length - 1] = true;
        }

        if (GUILayout.Button("Remove Segment"))
        {
            Undo.RecordObject(spline, "Segment removed");
            if(spline.RemoveSegment())
                Array.Resize(ref segments, segments.Length - 1);
        }
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(spline);

        SceneView.RepaintAll();
    }

    public Vector3 ShowPoint(int index)
    {
        Vector3 point = transform.TransformPoint(spline.GetControlPoint(index));

        EditorGUI.BeginChangeCheck();
        Undo.RecordObject(spline, "Changed Handle");
        float size = HandleUtility.GetHandleSize(point);
        if (index % 3 == 0)
        {
            if (spline.GetPointMode(index) == PointMode.Linear)
                Handles.color = Color.white;
            else
                Handles.color = Color.gray;
        }
        else
        {
            if (spline.GetPointMode(index) == PointMode.Linear)
                return point;
            else
                Handles.color = modeColors[(int)spline.GetPointMode(index)];
        }

        if (Handles.Button(point, rotation, handleSize * size, picksize * size, Handles.DotHandleCap))
        {
            selectedIndex = index;
            Repaint();
        }

        if (spline.GetPointMode(index) == PointMode.Error)
            return point;

        if (selectedIndex == index)
        {
            point = Handles.DoPositionHandle(point, rotation);
        }
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(spline);

        return point;
    }

    void ShowDirections()
    {
        if (directionLines <= 0)
            return;

        Vector3 p0 = spline.SamplePoint(0f);
        Handles.color = Color.green;
        Handles.DrawLine(p0, p0 + spline.SampleDirection(0f, directionScale));

        int lines = spline.SegmentCount * directionLines;
        for (int i = 0; i <= lines; i++)
        {
            float progress = i / (float)lines;
            p0 = spline.SamplePoint(progress);
            Handles.DrawLine(p0, p0 + spline.SampleDirection(progress, directionScale));
        }
    }
}
