using UnityEngine;

public class QuadraticBezier
{
    public Vector3[] points;
    float[] lut_t, lut_d;

    public float Length { get; private set; }

    public QuadraticBezier()
    {

    }

    public QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        points = new Vector3[3];
        points[0] = p0;
        points[1] = p1;
        points[2] = p2;
    }

    public float MeasureLength(int steps)
    {
        float step = 1f / (float)steps;
        float d = 0;

        lut_t = new float[steps + 1];
        lut_t[0] = 0;
        lut_d = new float[steps + 1];
        lut_d[0] = 0;

        //string coords = "coordinates {";
        //string xTick = "xtick={0";
        //string ytick = "ytick={0";
        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i * step;
            d += Vector3.Distance(SamplePoint(t), SamplePoint(t - step));

            lut_t[i] = t;
            lut_d[i] = d;

            //string tString = t.ToString(new CultureInfo("en-US"));
            //string dString = d.ToString(new CultureInfo("en-US"));
            //coords += "(" + tString + "," + dString + ")";
            //xTick += "," + tString;
            //ytick += "," + dString;
        }

        //Debug.Log(coords);
        //Debug.Log(xTick);
        //Debug.Log(ytick);

        Length = d;
        return d;
        //projectionLUT = new ProjectionLUT[steps + 1];
        //step = Length / steps;
        //for (int i = 0; i <= steps; i++)
        //{
        //    d = i * step;
        //    float t = DistToProg(d);
        //    projectionLUT[i] = new ProjectionLUT { t = t, p = transform.InverseTransformPoint(SamplePoint(t)) };
        //}
    }

    public Vector3 SamplePoint(float t)
    {
        if (points == null)
            return Vector3.zero;

        return SamplePoint(points, t);
    }

    public Vector3 SampleVelocity(float t)
    {
        if (points == null)
            return Vector3.zero;

        return SampleVelocity(points, t);
    }

    public Vector3 SampleDirection(float t)
    {
        return SampleVelocity(points, t).normalized;

    }

    public static Vector3 SamplePoint(Vector3[] points, float t)
    {
        if (points.Length < 3)
            return Vector3.zero;

        float _t = 1 - t;
        return _t * _t * points[0] + 2f * t * _t * points[1] + t * t * points[2];
    }

    public static Vector3 SampleVelocity(Vector3[] points, float t)
    {
        if (points.Length < 3)
            return Vector3.zero;

        float _t = 1 - t;
        return 2f * _t * (points[1] - points[0]) + 2f * t * (points[2] - points[1]);
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
}
