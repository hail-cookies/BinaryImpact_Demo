using UnityEngine;
using System;
using System.Collections.Generic;
//using Unity.EditorCoroutines.Editor;

namespace CustomVR
{ 
public enum PointMode { Free, Aligned, Mirrored, Linear, Error }
public enum PathMode { Path, Loop }

    [AddComponentMenu("Spline")]
    public class SplineComponent : MonoBehaviour
    {
        public static Dictionary<GameObject, SplineComponent> cache = new Dictionary<GameObject, SplineComponent>();
        [SerializeField]
        public Spline spline;

        public Vector3[] Points 
        { 
            get
            {
                if (spline.points == null)
                    spline.SetPoints(new Vector3[]
                    {
                        Vector3.zero,
                        Vector3.forward * 0.2f,
                        Vector3.forward * 0.4f,
                        Vector3.forward * 0.6f
                    }, transform);

                return spline.points;
            }
        }

        [SerializeField]
        PointMode[] pointModes;

        [SerializeField]
        PathMode pathMode = PathMode.Path;

        public PathMode PathMode
        {
            get { return pathMode; }
            set
            {
                pathMode = value;
                if (value == PathMode.Loop)
                {
                    pointModes[pointModes.Length - 1] = pointModes[0];
                    SetControlPoint(0, Points[0]);
                }
            }
        }

        #region Editor
        /// <summary>
        /// The full length of this spline
        /// </summary>
        bool CheckIndex(int index)
        {
            if (index < 0 || index > Points.Length - 1)
            {
                Debug.LogError("Index " + index + " out of Range! " + Points.Length);
                return false;
            }
            else
                return true;
        }

        public void EnforceMode(int index)
        {
            if (!CheckIndex(index))
                return;

            int modeIndex = (index + 1) / 3;
            PointMode mode = pointModes[modeIndex];
            if (mode == PointMode.Free || mode == PointMode.Error)
                return;

            int middleIndex = modeIndex * 3;
            if (pathMode != PathMode.Loop && mode == PointMode.Linear)
            {
                if (modeIndex > 0)
                {
                    Vector3 dir = (Points[middleIndex] - Points[middleIndex - 3]) * 0.3f;
                    Points[middleIndex - 1] = Points[middleIndex] - dir;

                    if (pointModes[modeIndex - 1] == PointMode.Linear)
                        Points[middleIndex - 2] = Points[middleIndex - 3] + dir;
                }

                if (modeIndex < pointModes.Length - 1)
                {
                    Vector3 dir = (Points[middleIndex] - Points[middleIndex + 3]) * 0.3f;
                    Points[middleIndex + 1] = Points[middleIndex] - dir;

                    if (pointModes[modeIndex + 1] == PointMode.Linear)
                        Points[middleIndex + 2] = Points[middleIndex + 3] + dir;
                }
            }
            else
            {
                if ((modeIndex == 0 || modeIndex == pointModes.Length - 1) && pathMode != PathMode.Loop)
                    return;

                int enforcedIndex, fixedIndex;
                if (index <= middleIndex)
                {
                    fixedIndex = middleIndex - 1;
                    if (pathMode == PathMode.Loop && fixedIndex < 0)
                        fixedIndex = Points.Length - 2;

                    enforcedIndex = middleIndex + 1;
                    if (pathMode == PathMode.Loop && enforcedIndex >= Points.Length)
                        enforcedIndex = 1;
                }
                else
                {
                    fixedIndex = middleIndex + 1;
                    if (fixedIndex >= Points.Length)
                        fixedIndex = 1;

                    enforcedIndex = middleIndex - 1;
                    if (enforcedIndex < 0)
                        enforcedIndex = Points.Length - 2;
                }

                Vector3 middle = Points[middleIndex];
                Vector3 enforcedTangent = middle - Points[fixedIndex];

                if (mode == PointMode.Aligned)
                    enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, Points[enforcedIndex]);

                Points[enforcedIndex] = middle + enforcedTangent;
            }
        }

        public PointMode GetPointMode(int index)
        {
            if (pointModes == null)
                pointModes = new PointMode[] { PointMode.Aligned, PointMode.Aligned };

            if (!CheckIndex(index))
                return PointMode.Error;

            return pointModes[(index + 1) / 3];
        }

        public void SetPointMode(int index, PointMode mode)
        {
            if (pointModes == null)
                pointModes = new PointMode[] { PointMode.Aligned, PointMode.Aligned };

            if (!CheckIndex(index))
                return;

            index = (index + 1) / 3;
            pointModes[index] = mode;

            if (pathMode == PathMode.Loop)
            {
                if (index == 0)
                    pointModes[pointModes.Length - 1] = mode;

                if (index == pointModes.Length - 1)
                    pointModes[0] = mode;
            }

            EnforceMode(index);
        }

        public Vector3 GetControlPoint(int index)
        {
            if (!CheckIndex(index))
                return Vector3.zero;

            return Points[index];
        }

        public void SetControlPoint(int index, Vector3 point)
        {
            if (!CheckIndex(index))
                return;
            if (Points[index] == point)
                return;

            if (index % 3 == 0)
            {
                Vector3 delta = point - Points[index];

                if (pathMode == PathMode.Loop)
                {
                    int max = Points.Length - 1;
                    if (index == 0)
                    {
                        Points[1] += delta;
                        Points[max] = point;
                        Points[max - 1] += delta;
                    }
                    else if (index == max)
                    {
                        Points[1] += delta;
                        Points[0] = point;
                        Points[max - 1] += delta;
                    }
                    else
                    {
                        Points[index - 1] += delta;
                        Points[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                        Points[index - 1] += delta;
                    if (index + 1 < Points.Length)
                        Points[index + 1] += delta;
                }
            }

            Points[index] = point;
            EnforceMode(index);
        }

        public void AddSegment()
        {
            int len = Points.Length + 3;
            Vector3 p0 = Points[Points.Length - 1];
            Vector3 vel = p0 - Points[Points.Length - 2];
            float step = vel.magnitude;
            Vector3 dir = vel / step;

            Vector3[] pts = Points;
            Array.Resize(ref pts, len);
            p0 += dir * step;
            pts[len - 3] = p0;
            p0 += dir * step;
            pts[len - 2] = p0;
            p0 += dir * step;
            pts[len - 1] = p0;
            spline.SetPoints(pts, transform);

            EnforceMode(Points.Length - 4);

            Array.Resize(ref pointModes, pointModes.Length + 1);
            pointModes[pointModes.Length - 1] = pointModes[pointModes.Length - 2];

            if (pathMode == PathMode.Loop)
            {
                Points[Points.Length - 1] = Points[0];
                pointModes[pointModes.Length - 1] = pointModes[0];
                EnforceMode(0);
            }
        }

        public bool RemoveSegment()
        {
            int len = Points.Length;
            if (len > 6)
            {
                Vector3[] pts = Points;
                Array.Resize(ref pts, len - 3);
                spline.SetPoints(pts, transform);

                Array.Resize(ref pointModes, pointModes.Length - 1);
                return true;
            }
            else
                return false;
        }

        public void SetPoints(Vector3[] points)
        {
            spline.SetPoints(points, transform);
            pointModes = new PointMode[SegmentCount + 1];
        }
        #endregion

        #region Get
        public int PointCount { get => spline.PointCount; }
        public int SegmentCount { get => spline.SegmentCount; }
        public float Length { get => spline.Length; }
        public Vector3 SamplePoint(float t) => spline.SamplePoint(t, transform);
        public Vector3 SampleVelocity(float t) => spline.SampleVelocity(t, transform);
        public Vector3 SampleDirection(float t, float scale = 1) => spline.SampleDirection(t, transform, scale);
        public float ProjectPoint(Vector3 point, float tolerance = 0.001f, int maxIterations = 100, bool useLut = true) =>
            spline.ProjectPoint(point, transform, tolerance, maxIterations, useLut);
        public float ProjectVelocity(float t, Vector3 velocity) => spline.ProjectVelocity(t, velocity, transform);
        public float GetLength() => spline.GetLength(transform);
        #endregion

        private void Awake()
        {
            if (!cache.ContainsKey(gameObject))
                cache.Add(gameObject, this);

            spline.GetLength(transform);
        }
    }
}
