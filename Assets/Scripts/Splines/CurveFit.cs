using UnityEngine;
using MathExtended.Matrices;

namespace CustomVR
{
    public class CurveFit
    {
        [Range(0.0001f, 0.01f)]
        public static float safety = 0.001f;

        public static void FitCurveSegment(Vector3[] targetsInLocalSpace, float t1 = 0.33f, float t2 = 0.67f, bool pomax = false)
        {
            if (targetsInLocalSpace == null)
                return;
            if (targetsInLocalSpace.Length != 4)
                return;

            Matrix P = SetupMatrix(new float[4, 3]{
                { targetsInLocalSpace[0].x, targetsInLocalSpace[0].y, targetsInLocalSpace[0].z },
                { targetsInLocalSpace[1].x, targetsInLocalSpace[1].y, targetsInLocalSpace[1].z },
                { targetsInLocalSpace[2].x, targetsInLocalSpace[2].y, targetsInLocalSpace[2].z },
                { targetsInLocalSpace[3].x, targetsInLocalSpace[3].y, targetsInLocalSpace[3].z },
            });

            if (pomax)
            {
                Matrix M = SetupMatrix(new float[4, 4]{
                { 1, 0, 0, 0 },
                { -3, 3, 0, 0 },
                { 3, -6, 3, 0 },
                { -1, 3, -3, 1 }
            });
                Matrix M_inv = !M;

                Matrix T = SetupMatrix(new float[4, 4]{
                { 1,  0,                0,                0 },
                { 1, t1, Mathf.Pow(t1, 2), Mathf.Pow(t1, 3) },
                { 1, t2, Mathf.Pow(t2, 2), Mathf.Pow(t2, 3) },
                { 1,  1,                1,                1 },
                });

                Matrix T_trn = T.Duplicate(); T_trn.Transpose();
                Matrix T_trn_by_T = T_trn * T; T_trn_by_T.Inverse();

                Matrix C = M_inv * T_trn_by_T * T_trn * P;

                for (int i = 0; i < 4; i++)
                    targetsInLocalSpace[i] = RowToVector3(C, i);
            }
            else
            {
                Matrix M = SetupMatrix(new float[4, 4]{
                { 6, 0, 0, 0 },
                { -5, 18, -9, 2 },
                { 2, -9, 18, -5 },
                { 0, 0, 0, 6 }
            });

                Matrix controls = M * P;
                controls = 1 / 6f * controls;

                for (int i = 0; i < 4; i++)
                    targetsInLocalSpace[i] = RowToVector3(controls, i);
            }
        }

        public static Matrix SetupMatrix(float[,] content)
        {
            Matrix result = new Matrix(content.GetLength(0), content.GetLength(1));
            for (int i = 0; i < result.Rows; i++)
            {
                for (int j = 0; j < result.Columns; j++)
                {
                    result[i + 1, j + 1] = content[i, j];
                }
            }

            return result;
        }

        public static Vector3 RowToVector3(Matrix m, int index)
        {
            index += 1;
            return new Vector3(
                (float)m[index, 1],
                (float)m[index, 2],
                (float)m[index, 3]);
        }

        public static string PrintMatrix(Matrix m, bool writeToLog = true)
        {
            string output = "";
            for (int i = 1; i <= m.Rows; i++)
            {
                output += "\n[";

                for (int j = 1; j <= m.Columns; j++)
                {
                    output += j == 1 ? " " + m[i, j] : ",\t" + m[i, j];
                }

                output += "]\n";
            }

            if (writeToLog)
                Debug.Log(output);

            return output;
        }
    }
}
