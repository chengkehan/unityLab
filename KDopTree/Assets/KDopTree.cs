using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KDopTree
{
    public const float SMALL_NUMBER = 0.00000001f;

    public const float KINDA_SMALL_NUMBER = 0.0001f;

    public const int NUM_PLANES = 3;

    public List<KDopNode> nodes = null;

    public List<KDopTriangle> triangles = null;

    public void Build(Mesh mesh)
    {
        if(mesh == null || !mesh.isReadable)
        {
            return;
        }

        if(nodes != null)
        {
            nodes.Clear();
        }
        nodes = new List<KDopNode>();

        BuildTriangles(mesh);

        KDopNode rootNode = new KDopNode();
        nodes.Add(rootNode);
        rootNode.SplitTriangleList(0, triangles.Count, this);
        nodes[0] = rootNode;
    }

    private void BuildTriangles(Mesh mesh)
    {
        List<int[]> trisList = new List<int[]>();
        int totalTris = 0;
        for (int meshIndex = 0; meshIndex < mesh.subMeshCount; ++meshIndex)
        {
            int[] tris = mesh.GetTriangles(meshIndex);
            trisList.Add(tris);
            totalTris += tris.Length / 3;
        }

        if(triangles != null)
        {
            triangles.Clear();
        }
        triangles = new List<KDopTriangle>(totalTris);

        Vector3[] vertices = mesh.vertices;
        for(int meshIndex = 0; meshIndex < mesh.subMeshCount; ++meshIndex)
        {
            int[] tris = mesh.GetTriangles(meshIndex);
            for (int triIndex = 0; triIndex < tris.Length; triIndex += 3)
            {
                KDopTriangle tri;

                tri.v0 = vertices[tris[triIndex + 0]];
                tri.v1 = vertices[tris[triIndex + 1]];
                tri.v2 = vertices[tris[triIndex + 2]];

                if (tri.GetNormal().IsUnit())
                {
                    triangles.Add(new KDopTriangle() { v0=tri.v0, v1=tri.v1, v2=tri.v2 });
                }
            }
        }


    }
}

public struct KDopNode
{
    public KDopBounds boundingVolumes;

    public bool isLeaf;

    public KDopNodeData data;

    public KDopBounds SplitTriangleList(int startIndex, int numTriangles, KDopTree kDopTree)
    {
        if(numTriangles > 4)
        {
            isLeaf = false;

            int BestPlane = -1;
            float BestMean = 0.0f;
            float BestVariance = 0.0f;
            for (int nPlane = 0; nPlane < KDopTree.NUM_PLANES; nPlane++)
            {
                float Mean = 0.0f;
                float Variance = 0.0f;
                for (int nTriangle = startIndex; nTriangle < startIndex + numTriangles; nTriangle++)
                {
                    Mean += kDopTree.triangles[nTriangle].GetCentroid()[nPlane];
                }
                Mean /= (float)numTriangles;
                for (int nTriangle = startIndex; nTriangle < startIndex + numTriangles; nTriangle++)
                {
                    float Dot = kDopTree.triangles[nTriangle].GetCentroid()[nPlane];
                    Variance += (Dot - Mean) * (Dot - Mean);
                }
                Variance /= (float)numTriangles;
                if (Variance >= BestVariance)
                {
                    BestPlane = nPlane;
                    BestVariance = Variance;
                    BestMean = Mean;
                }
            }

            int Left = startIndex - 1;
            int Right = startIndex + numTriangles;
            while (Left < Right)
            {
                float Dot;
                do
                {
                    Dot = kDopTree.triangles[++Left].GetCentroid()[BestPlane];
                }
                while (Dot < BestMean && Left < Right);
                do
                {
                    Dot = kDopTree.triangles[--Right].GetCentroid()[BestPlane];
                }
                while (Dot >= BestMean && Right > 0 && Left < Right);
                if (Left < Right)
                {
                    KDopTriangle Temp = kDopTree.triangles[Left];
                    kDopTree.triangles[Left] = kDopTree.triangles[Right];
                    kDopTree.triangles[Right] = Temp;
                }
            }
            if (Left == startIndex + numTriangles || Right == startIndex)
            {
                Left = startIndex + (numTriangles / 2);
            }

            KDopNode leftNode = new KDopNode();
            int leftNodeIndex = kDopTree.nodes.Count;
            kDopTree.nodes.Add(leftNode);
            data.leftNode = leftNodeIndex;

            KDopNode rightNode = new KDopNode();
            int rightNodeIndex = kDopTree.nodes.Count;
            kDopTree.nodes.Add(rightNode);
            data.rightNode = rightNodeIndex;

            KDopBounds leftBoundingVolume = leftNode.SplitTriangleList(startIndex, Left - startIndex, kDopTree);
            KDopBounds rightBoundingVolume = rightNode.SplitTriangleList(Left, startIndex + numTriangles - Left, kDopTree);

            boundingVolumes.Encapsulate(leftBoundingVolume);
            boundingVolumes.Encapsulate(rightBoundingVolume);

            kDopTree.nodes[leftNodeIndex] = leftNode;
            kDopTree.nodes[rightNodeIndex] = rightNode;
        }
        else
        {
            isLeaf = true;

            data.startIndex = startIndex;
            data.numTriangles = numTriangles;

            for (int triIndex = data.startIndex; triIndex < data.startIndex + numTriangles; ++triIndex)
            {
                KDopTriangle tri = kDopTree.triangles[triIndex];
                boundingVolumes.Encapsulate(tri.v0);
                boundingVolumes.Encapsulate(tri.v1);
                boundingVolumes.Encapsulate(tri.v2);
            }
        }

        return boundingVolumes;
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct KDopNodeData
{
    [FieldOffset(0)]
    public int leftNode;
    [FieldOffset(sizeof(int))]
    public int rightNode;

    [FieldOffset(0)]
    public int numTriangles;
    [FieldOffset(sizeof(int))]
    public int startIndex;
}

public struct KDopTriangle
{
    public Vector3 v0;

    public Vector3 v1;

    public Vector3 v2;

    public Vector3 GetCentroid()
    {
        return (v0 + v1 + v2) / 3.0f;
    }

    public Vector4 GetNormalPlane()
    {
        Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).GetSafeNormal();
        Vector4 normalPlane = new Vector4(normal.x, normal.y, normal.z, 0);
        normalPlane.w = Vector3.Dot(v0, normal);
        return normalPlane;
    }

    public Vector3 GetNormal()
    {
        return Vector3.Cross(v1 - v0, v2 - v0).GetSafeNormal();
    }
}

public struct KDopBounds
{
    public Vector3 min;

    public Vector3 max;

    public bool isValid;

    public Vector3 Center
    {
        get
        {
            if(isValid)
            {
                return min + (max - min) * 0.5f;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    public Vector3 Size
    {
        get
        {
            if(isValid)
            {
                return max - min;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    public void Encapsulate(KDopBounds otherBounds)
    {
        if(isValid && otherBounds.isValid)
        {
            min.x = Mathf.Min(min.x, otherBounds.min.x);
            min.y = Mathf.Min(min.y, otherBounds.min.y);
            min.z = Mathf.Min(min.z, otherBounds.min.z);

            max.x = Mathf.Max(max.x, otherBounds.max.x);
            max.y = Mathf.Max(max.y, otherBounds.max.y);
            max.z = Mathf.Max(max.z, otherBounds.max.z);
        }
        else if(otherBounds.isValid)
        {
            min = otherBounds.min;
            max = otherBounds.max;
            isValid = true;
        }
        else
        {
            // Do nothing
        }
    }

    public void Encapsulate(Vector3 otherPoint)
    {
        if(isValid)
        {
            min.x = Mathf.Min(min.x, otherPoint.x);
            min.y = Mathf.Min(min.y, otherPoint.y);
            min.z = Mathf.Min(min.z, otherPoint.z);

            max.x = Mathf.Max(max.x, otherPoint.x);
            max.y = Mathf.Max(max.y, otherPoint.y);
            max.z = Mathf.Max(max.z, otherPoint.z);
        }
        else
        {
            min = otherPoint;
            max = otherPoint;
            isValid = true;
        }
    }

    public override string ToString()
    {
        return 
            "[" + GetType().Name + 
            " min:" + min.x + "," + min.y + "," + min.z + 
            " max:" + max.x + "," + max.y + "," + max.z + "]";
    }
}

public static class KDopTreeVectorExtension
{
    public static Vector3 GetSafeNormal(this Vector3 v)
    {
        float squareSum = v.x * v.x + v.y * v.y + v.z * v.z;
        if (squareSum > KDopTree.SMALL_NUMBER)
        {
            float scale = Mathf.Sqrt(1.0f / squareSum);
            v.x = v.x * scale;
            v.y = v.y * scale;
            v.z = v.z * scale;
            return v;
        }
        v.x = v.y = v.z = 0.0f;
        return v;
    }

    public static string ToStringRaw(this Vector3 v)
    {
        return v.x + " " + v.y + " " + v.z;
    }

    public static bool IsUnit(this Vector3 v)
    {
        return Mathf.Abs(1.0f - v.sqrMagnitude) < KDopTree.KINDA_SMALL_NUMBER;
    }
}