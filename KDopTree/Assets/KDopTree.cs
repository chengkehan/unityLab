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

    public List<KDopNode> nodes { private set; get; }

    public List<KDopTriangle> triangles { private set; get; }

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

    public void LineCheck(KDopCollisionCheck check)
    {
        if(nodes == null || nodes.Count == 0)
        {
            return;
        }
        nodes[0].LineCheck(check, this);
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

    private static Vector3[] s_trianglePoints = new Vector3[3];

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

    public void LineCheck(KDopCollisionCheck check, KDopTree kDopTree)
    {
        if(isLeaf)
        {
            if (LineCheckBounds(check, kDopTree))
            {
                LineCheckTriangles(check, kDopTree);
            }
        }
        else
        {
            if(LineCheckBounds(check, kDopTree))
            {
                kDopTree.nodes[data.leftNode].LineCheck(check, kDopTree);
                kDopTree.nodes[data.rightNode].LineCheck(check, kDopTree);
            }
        }
    }

    private void LineCheckTriangles(KDopCollisionCheck check, KDopTree kDopTree)
    {
        for(int triIndex = data.startIndex; triIndex < data.startIndex + data.numTriangles; ++triIndex)
        {
            KDopTriangle tri = kDopTree.triangles[triIndex];
            Vector3 trip0 = tri.v0;
            Vector3 trip1 = tri.v1;
            Vector3 trip2 = tri.v2;

            Vector3 linep0 = check.LocalStart;
            Vector3 linep1 = check.LocalEnd;

            // Line is on the same side of triangle-plane
            Vector3 triNormal = Vector3.Cross(trip1 - trip0, trip2 - trip0);
            triNormal.Normalize();
            float triPlaneD = Vector3.Dot(trip0, triNormal);
            Vector4 triPlane = new Vector4(triNormal.x, triNormal.y, triNormal.z, triPlaneD);
            float line0D = Vector3.Dot(linep0, triNormal) - triPlaneD;
            float line1D = Vector3.Dot(linep1, triNormal) - triPlaneD;
            if (line0D * line1D > 0)
            {
                continue;
            }

            // Figure out the hit point(intersection)
            float hitTime = line0D / (line0D - line1D);
            Vector3 lineDir = linep1 - linep0;
            Vector3 hitP = linep0 + lineDir * hitTime;

            // Check if the point point is inside the triangle
            s_trianglePoints[0] = trip0;
            s_trianglePoints[1] = trip1;
            s_trianglePoints[2] = trip2;
            for (int sideIndex = 0; sideIndex < 3; ++sideIndex)
            {
                Vector3 edge = s_trianglePoints[(sideIndex + 1) % 3] - s_trianglePoints[sideIndex];
                Vector3 sideDir = Vector3.Cross(triNormal, edge);
                Vector3 hitDir = hitP - s_trianglePoints[sideIndex];
                float side = Vector3.Dot(hitDir, sideDir);
                if (side < 0)
                {
                    // Hit point is outside the triangle.
                    hitTime = float.MaxValue;
                    break;
                }
            }

            if(hitTime < check.HitResult.hitTime)
            {
                check.HitResult.hitTime = hitTime;
                check.HitResult.isHit = true;
                check.HitResult.hitTriangle = triIndex;
            }
        }
    }

    private bool LineCheckBounds(KDopCollisionCheck check, KDopTree kDopTree)
    {
        Vector3 min = boundingVolumes.min;
        Vector3 max = boundingVolumes.max;

        Vector3 startP = check.LocalStart;
        Vector3 endP = check.LocalEnd;

        Vector3 dir = check.LocalEnd - check.LocalStart;
        Vector3 oneOverDir = check.LocalOneOverDir;

        // Slabs
        float _minSlabX = (min.x - startP.x) * oneOverDir.x;
        float _minSlabY = (min.y - startP.y) * oneOverDir.y;
        float _minSlabZ = (min.z - startP.z) * oneOverDir.z;

        float _maxSlabX = (max.x - startP.x) * oneOverDir.x;
        float _maxSlabY = (max.y - startP.y) * oneOverDir.y;
        float _maxSlabZ = (max.z - startP.z) * oneOverDir.z;

        // Min/Max Slabs
        float minSlabX = Mathf.Min(_minSlabX, _maxSlabX);
        float minSlabY = Mathf.Min(_minSlabY, _maxSlabY);
        float minSlabZ = Mathf.Min(_minSlabZ, _maxSlabZ);

        float maxSlabX = Mathf.Max(_minSlabX, _maxSlabX);
        float maxSlabY = Mathf.Max(_minSlabY, _maxSlabY);
        float maxSlabZ = Mathf.Max(_minSlabZ, _maxSlabZ);

        float minSlab = Mathf.Max(Mathf.Max(minSlabX, minSlabY), minSlabZ);
        float maxSlab = Mathf.Min(Mathf.Min(maxSlabX, maxSlabY), maxSlabZ);

        bool bHit = maxSlab >= 0.0f && maxSlab >= minSlab && minSlab <= 1.0f;
        if (bHit)
        {
            int hitSurface = 0;
            if (minSlab >= 0 && minSlab <= 1)
            {
                // Draw first hit point
                ++hitSurface;
            }
            if (maxSlab >= 0 && maxSlab <= 1)
            {
                // Draw second hit point
                ++hitSurface;
            }
            if (hitSurface == 0)
            {
                // line segment inside bounds
                return true;
            }
            else
            {
                // line segment hit surface
                return true;
            }
        }
        else
        {
            // line segment hit nothing
            return false;
        }
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
        Vector3 normal = GetNormal();
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

    private bool isValid;

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

public class KDopCollisionCheck
{
    public Vector3 WorldStart { private set; get; }

    public Vector3 WorldEnd { private set; get; }

    public Matrix4x4 WorldToLocal { private set; get; }

    public KDopHitResult HitResult { private set; get; }

    public Vector3 LocalStart { private set; get; }

    public Vector3 LocalEnd { private set; get; }

    public Vector3 LocalDir { private set; get; }

    public Vector3 LocalOneOverDir { private set; get; }

    public void Init(Vector3 worldStart, Vector3 worldEnd, Matrix4x4 worldToLocal)
    {
        this.WorldStart = worldStart;
        this.WorldEnd = worldEnd;
        this.WorldToLocal = worldToLocal;

        this.LocalStart = worldToLocal.MultiplyPoint(worldStart);
        this.LocalEnd = worldToLocal.MultiplyPoint(worldEnd);
        this.LocalDir = this.LocalEnd - this.LocalStart;
        this.LocalOneOverDir = this.LocalDir.Inverse();

        HitResult = HitResult == null ? new KDopHitResult() : HitResult;
        HitResult.hitTime = float.MaxValue;
        HitResult.isHit = false;
        HitResult.hitTriangle = -1;
    }
}

public class KDopHitResult
{
    public int hitTriangle = 0;

    public float hitTime = 0;

    public bool isHit = false;
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

    public static Vector3 Inverse(this Vector3 v)
    {
        v.x = 1.0f / v.x;
        v.y = 1.0f / v.y;
        v.z = 1.0f / v.z;
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