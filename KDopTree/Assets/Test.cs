using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public MeshFilter mf = null;

    [Range(0, 20)]
    public int depth = 0;

    public Transform linep0 = null;

    public Transform linep1 = null;

    private KDopTree kDopTree = null;

    private Color[] colors = null;

    private KDopCollisionCheck check = new KDopCollisionCheck();

    private void OnDrawGizmos()
    {
        if(kDopTree != null)
        {
            Gizmos.matrix = mf.transform.localToWorldMatrix;

            List<KDopNode> nodes = kDopTree.nodes;
            int leafCount = 0;
            for(int nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex)
            {
                if(leafCount == depth)
                {
                    break;
                }
                KDopNode node = kDopTree.nodes[nodeIndex];
                if (node.isLeaf)
                {
                    ++leafCount;
                    KDopBounds bounds = node.boundingVolumes;
                    Gizmos.color = colors[nodeIndex % colors.Length];
                    Gizmos.DrawWireCube(bounds.Center, bounds.Size);
                }
            }
        }

        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.white;
        Gizmos.DrawLine(linep0.position, linep1.position);

        if (kDopTree != null)
        {
            check.Init(linep0.position, linep1.position, mf.transform.worldToLocalMatrix);
            kDopTree.LineCheck(check);
            if (check.HitResult.isHit)
            {
                KDopTriangle tri = kDopTree.triangles[check.HitResult.hitTriangle];
                Vector3 hitPoint = check.LocalStart + check.HitResult.hitTime * check.LocalDir;

                Gizmos.matrix = mf.transform.localToWorldMatrix;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(tri.v0, tri.v1);
                Gizmos.DrawLine(tri.v1, tri.v2);
                Gizmos.DrawLine(tri.v2, tri.v0);
                Gizmos.DrawSphere(hitPoint, 0.01f);
                Gizmos.DrawLine(hitPoint, hitPoint + tri.GetNormal() * 0.3f);
            }
        }
    }

    private void Start()
    {
        colors = new Color[100];
        for(int i = 0; i < colors.Length; ++i)
        {
            colors[i] = new Color(Random.value, Random.value, Random.value);
        }

        kDopTree = new KDopTree();
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();
        kDopTree.Build(mf.mesh);
        stopWatch.Stop();
        UnityEngine.Debug.LogError("Elapsed Milliseconds for Building KDopTree:" + stopWatch.ElapsedMilliseconds);
    }
}
