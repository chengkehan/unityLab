using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KDopTree
{
    public const float SMALL_NUMBER = 0.00000001f;

    public const float KINDA_SMALL_NUMBER = 0.0001f;

    public List<KDopNode> nodes = null;

    public List<KDopTriangle> triangles = null;

    public void Build(Mesh mesh)
    {
        if(mesh == null || !mesh.isReadable)
        {
            return;
        }

        if(nodes == null)
        {
            nodes = new List<KDopNode>();
        }
        nodes.Clear();

        if (triangles == null)
        {
            triangles = new List<KDopTriangle>();
        }
        triangles.Clear();

        BuildTriangles(mesh);

        // Add Root Node
        nodes.Add(new KDopNode());
        nodes[0].SplitTriangleList(0, triangles.Count, this);
    }

    private void BuildTriangles(Mesh mesh)
    {
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
    public Bounds boundingVolumes;

    public bool isLeaf;

    public KDopNodeData data;

    public void SplitTriangleList(int startTriangleIndex, int numTriangles, KDopTree kDopTree)
    {

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

    public static bool IsUnit(this Vector3 v)
    {
        return Mathf.Abs(1.0f - v.sqrMagnitude) < KDopTree.KINDA_SMALL_NUMBER;
    }
}