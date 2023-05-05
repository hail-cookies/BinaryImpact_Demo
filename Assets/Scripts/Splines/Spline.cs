using UnityEngine;

namespace CustomVR
{
    public struct ProjectionLUT
    {
        public float t;
        public Vector3 p;
    }

    public struct ProjectionStep
    {
        public int bestIndex;
        public ProjectionLUT[] luts;
    }

    [System.Serializable]
    public class Spline
    {
        [SerializeField, HideInInspector]
        public int LutResolution = 20;
        public int PointCount { get => points.Length; }
        public int SegmentCount { get => (PointCount - 1) / 3; }
        public float Length { get; private set; } = -1;

        public Spline(Vector3[] points, int lutResolution)
        {
            this.points = points;
            LutResolution = lutResolution;
        }

        [SerializeField]
        public Vector3[] points = new Vector3[] { Vector3.zero, Vector3.forward * 0.2f, Vector3.forward * 0.4f, Vector3.forward * 0.6f };
        public void SetPoints(Vector3[] points, Transform tr)
        {
            this.points = points;
            GetLength(tr);
        }

        #region CubicBezier
        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float _t = 1f - t;
            return
                _t * _t * _t * p0 +
                3 * _t * _t * t * p1 +
                3 * _t * t * t * p2 +
                t * t * t * p3;
        }

        public static Vector3 GetTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float _t = 1f - t;
            return
                3 * _t * _t * (p1 - p0) +
                6 * _t * t * (p2 - p1) +
                3 * t * t * (p3 - p2);
        }
        #endregion

        #region Get
        public int GetIndex(ref float t)
        {
            int i;

            if (t >= 1)
            {
                t = 1;
                i = points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * SegmentCount;
                i = (int)t;
                t -= i;
                i *= 3;
            }

            return i;
        }

        public Vector3 SamplePoint(float t, Transform tr) => tr.TransformPoint(SamplePoint(t));
        public Vector3 SamplePoint(float t)
        {
            int i = GetIndex(ref t);
            return GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t);
        }

        public Vector3 SampleVelocity(float t, Transform tr) => tr.TransformPoint(SampleVelocity(t)) - tr.position;
        public Vector3 SampleVelocity(float t)
        {
            int i = GetIndex(ref t);
            return GetTangent(points[i + 0], points[i + 1], points[i + 2], points[i + 3], t);
        }

        public Vector3 SampleDirection(float t, Transform tr, float scale = 1) => SampleVelocity(t, tr).normalized * scale;
        public Vector3 SampleDirection(float t, float scale = 1) => SampleVelocity(t).normalized * scale;

        public float ProjectPoint(Vector3 point, Transform tr, float tolerance = 0.001f, int maxIterations = 100, bool useLut = true)
        {
            point = tr.InverseTransformPoint(point);

            float best = Mathf.Infinity;
            int index = 0;
            ProjectionLUT bestMatch = new ProjectionLUT();
            //Get initial estimate
            if (useLut)
            {
                for (int i = 0; i < projectionLUT.Length; i++)
                {
                    float sd = Vector3.SqrMagnitude(point - projectionLUT[i].p);
                    if (sd < best)
                    {
                        best = sd;
                        index = i;
                    }
                }
            }
            else
            {
                for (int i = 0; i < maxIterations; i++)
                {
                    Vector3 pt = SamplePoint(i / (float)maxIterations);
                    float sd = Vector3.SqrMagnitude(point - pt);
                    if (sd < best)
                    {
                        best = sd;
                        bestMatch.p = pt;
                        bestMatch.t = i / (float)maxIterations;
                        index = i;
                    }
                }
            }

            int safety = maxIterations;
            ProjectionLUT[] l = new ProjectionLUT[5];
            int len = projectionLUT.Length;
            if (useLut)
            {
                l[0] = index < 1 ? projectionLUT[0] : projectionLUT[index - 1];
                l[2] = projectionLUT[index];
                l[4] = index >= len - 1 ? projectionLUT[len - 1] : projectionLUT[index + 1];
            }
            else
            {
                l[0] = index < 1 ? bestMatch : new ProjectionLUT { t = (index - 1) / (float)maxIterations, p = SamplePoint(index / (float)maxIterations) };
                l[2] = bestMatch;
                l[4] = index >= maxIterations ? bestMatch : new ProjectionLUT { t = (index + 1) / (float)maxIterations, p = SamplePoint(index / (float)maxIterations) };
            }

            float interval = 1;
            while (interval > tolerance)
            {
                if (safety-- < 0)
                {
                    Debug.LogError("SAFETY BREAK");
                    break;
                }

                interval = l[4].t - l[0].t;
                float t1 = l[0].t * 0.5f + l[2].t * 0.5f;
                l[1] = new ProjectionLUT { t = t1, p = SamplePoint(t1) };
                float t3 = l[2].t * 0.5f + l[4].t * 0.5f;
                l[3] = new ProjectionLUT { t = t3, p = SamplePoint(t3) };

                ProjectionStep projectionStep = new ProjectionStep { luts = new ProjectionLUT[5] };

                best = Mathf.Infinity;
                index = 0;
                for (int i = 0; i < 5; i++)
                {
                    float sd = Vector3.Magnitude(point - l[i].p);
                    if (sd < best)
                    {
                        best = sd;
                        index = i;
                    }

                    projectionStep.luts[i] = l[i];
                }
                projectionStep.bestIndex = index;

                l[0] = index == 0 ? projectionStep.luts[0] : projectionStep.luts[index - 1];
                l[2] = projectionStep.luts[index];
                l[4] = index == 4 ? projectionStep.luts[4] : projectionStep.luts[index + 1];
            }

            return l[2].t;
        }

        public float ProjectVelocity(float t, Vector3 velocity, Transform tr)
        {
            if (Length < 0)
                GetLength(tr);
            if (Length == 0)
                return 1;

            Vector3 tangent = SampleDirection(t, tr, 1);
            velocity = Vector3.Project(velocity, tangent);
            float dir = Vector3.Dot(velocity, tangent);
            if (dir > 0)
                dir = 1;
            else if (dir < 0)
                dir = -1;
            else
                return t;

            dir = (velocity.magnitude / Length) * dir;
            return Mathf.Clamp01(t + dir);
        }

        float[] lut_t, lut_d;
        public ProjectionLUT[] projectionLUT;
        public float GetLength(Transform tr)
        {
            float step = 1f / LutResolution;
            float d = 0;

            lut_t = new float[LutResolution + 1];
            lut_t[0] = 0;
            lut_d = new float[LutResolution + 1];
            lut_d[0] = 0;

            for (int i = 1; i <= LutResolution; i++)
            {
                float t = (float)i * step;
                d += tr ? 
                    Vector3.Distance(SamplePoint(t, tr), SamplePoint(t - step, tr)) :
                    Vector3.Distance(SamplePoint(t), SamplePoint(t - step));

                lut_t[i] = t;
                lut_d[i] = d;
            }

            Length = d;

            projectionLUT = new ProjectionLUT[LutResolution + 1];
            step = Length / LutResolution;
            for (int i = 0; i <= LutResolution; i++)
            {
                d = i * step;
                float t = DistToProg(d);
                projectionLUT[i] = new ProjectionLUT { t = t, p = SamplePoint(t) };
            }

            return Length;
        }

        public float DistToProg(float d)
        {
            if (d <= 0)
                return 0;
            if (d >= Length)
                return 1;

            int i = 0;
            float bestMatch = Mathf.Infinity;
            for (int ind = 0; ind < lut_d.Length; ind++)
            {
                float match = Mathf.Abs(d - lut_d[ind]);
                if (match < bestMatch)
                {
                    bestMatch = match;
                    i = ind;
                }
            }

            float d_i = lut_d[i];
            if (d_i < d)
            {
                return ((d - d_i) / (lut_d[i + 1] - d_i)) * (lut_t[i + 1] - lut_t[i]) + lut_t[i];
            }
            else if (d_i > d)
            {
                return ((d - lut_d[i - 1]) / (lut_d[i] - lut_d[i - 1]) * (lut_t[i] - lut_t[i - 1])) + lut_t[i - 1];
            }
            else return lut_t[i];
        }
        #endregion

        #region GetSplit
        public Vector3[] GetSplit(float t)
        {
            int i = GetIndex(ref t);

            float _z = t - 1;
            float _z2 = _z * _z;
            float _z3 = _z2 * _z;
            float z2 = t * t;
            float z3 = z2 * t;

            Vector3 p0 = points[i];

            Vector3 p1 =
                z2 * points[i + 1] -
                _z * points[i];

            Vector3 p2 =
                z2 * points[i + 2] -
                2 * z2 * _z * points[i + 1] +
                _z2 * points[i];

            Vector3 p3 =
                z3 * points[i + 3] -
                3 * z2 * _z * points[i + 2] +
                3 * t * _z2 * points[i + 1] -
                _z3 * points[i];

            Vector3 p4 =
                z3 * points[i + 3] -
                z2 * _z * points[i + 2] +
                3 * t * _z2 * points[i + 1] -
                _z3 * points[i];

            Vector3 p5 =
                z2 * points[i + 3] -
                2 * t * _z * points[i + 2] +
                _z2 * points[i + 1];

            Vector3 p6 =
                t * points[i + 3] -
                _z * points[i + 2];

            Vector3 p7 = points[i + 3];

            return new Vector3[]
            {
                p0, p1, p2, p3,
                p4, p5, p6, p7
            };
        }
        #endregion
    }
}
