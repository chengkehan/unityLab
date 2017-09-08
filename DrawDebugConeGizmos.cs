using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Ref: UE4 DrawDebugHelper.cpp DrawDebugCone
public class DrawDebugConeGizmos : MonoBehaviour
{
    public float size = 1.0f;

    [Range(0.0f, 180.0f)]
    public float degreeWidth = 30.0f;

    [Range(0.0f, 180.0f)]
    public float degreeHeight = 30.0f;

    [Range(4, 720)]
    public int numSides = 15;

    private struct Line
    {
        public Vector3 start;
        public Vector3 end;
    }

    private void OnDrawGizmos()
    {
        DrawDebugCone(transform.localToWorldMatrix, size, degreeWidth * Mathf.Deg2Rad, degreeHeight * Mathf.Deg2Rad, numSides, Color.yellow);
    }

    private void DrawDebugCone(Matrix4x4 l2w, float Scale, float AngleWidth, float AngleHeight, int NumSides, Color DrawColor)
    {
        float SMALL_NUMBER = 1.0f * Mathf.Pow(10, -4);
        float PI = 3.1415926535897932f;

        NumSides = Mathf.Max(NumSides, 4);

        float Angle1 = Mathf.Clamp(AngleHeight, (float)SMALL_NUMBER, (float)(PI - SMALL_NUMBER));
        float Angle2 = Mathf.Clamp(AngleWidth, (float)SMALL_NUMBER, (float)(PI - SMALL_NUMBER));

        float SinX_2 = Mathf.Sin(0.5f * Angle1);
        float SinY_2 = Mathf.Sin(0.5f * Angle2);

        float SinSqX_2 = SinX_2 * SinX_2;
        float SinSqY_2 = SinY_2 * SinY_2;

        float TanX_2 = Mathf.Tan(0.5f * Angle1);
        float TanY_2 = Mathf.Tan(0.5f * Angle2);

        Vector3[] ConeVerts = new Vector3[NumSides];

        for (int i = 0; i < NumSides; i++)
        {
            float Fraction = (float)i / (float)(NumSides);
            float Thi = 2.0f * PI * Fraction;
            float Phi = Mathf.Atan2(Mathf.Sin(Thi) * SinY_2, Mathf.Cos(Thi) * SinX_2);
            float SinPhi = Mathf.Sin(Phi);
            float CosPhi = Mathf.Cos(Phi);
            float SinSqPhi = SinPhi * SinPhi;
            float CosSqPhi = CosPhi * CosPhi;

            float RSq = SinSqX_2 * SinSqY_2 / (SinSqX_2 * SinSqPhi + SinSqY_2 * CosSqPhi);
            float R = Mathf.Sqrt(RSq);
            float Sqr = Mathf.Sqrt(1 - RSq);
            float Alpha = R * CosPhi;
            float Beta = R * SinPhi;

            Vector3 v = ConeVerts[i];
            v.x = 2 * Sqr * Beta;
            v.y = 2 * Sqr * Alpha;
            v.z = (1 - 2 * RSq);
            ConeVerts[i] = v;
        }

        List<Line> Lines = new List<Line>();
        Vector3 CurrentPoint = Vector3.zero;
        Vector3 PrevPoint = Vector3.zero;
        Vector3 FirstPoint = Vector3.zero;
        for (int i = 0; i < NumSides; i++)
        {
            CurrentPoint = ConeVerts[i];
            Lines.Add(new Line() { start=Vector3.zero, end=CurrentPoint });

            if(i > 0)
            {
                Lines.Add(new Line() { start = PrevPoint, end = CurrentPoint });
            }
            else
            {
                FirstPoint = CurrentPoint;
            }

            PrevPoint = CurrentPoint;
        }
        Lines.Add(new Line() { start=CurrentPoint, end=FirstPoint });

        {
            Gizmos.matrix = l2w * Matrix4x4.Scale(new Vector3(Scale, Scale, Scale));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero, Vector3.right);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, Vector3.up);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.zero, Vector3.forward);

            Gizmos.color = DrawColor;
            for (int i = 0; i < Lines.Count; ++i)
            {
                Line line = Lines[i];
                Gizmos.DrawLine(line.start, line.end);
            }
        }
    }
}
