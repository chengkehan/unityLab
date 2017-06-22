using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform v0 = null;
    public Transform v1 = null;
    public Transform v2 = null;

    private void OnDrawGizmos()
    {
        KDopTriangle tri;
        tri.v0 = v0.position;
        tri.v1 = v1.position;
        tri.v2 = v2.position;

        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.red;

        Gizmos.DrawLine(tri.v0, tri.v1);
        Gizmos.DrawLine(tri.v1, tri.v2);
        Gizmos.DrawLine(tri.v2, tri.v0);

        Gizmos.DrawLine(tri.GetCentroid(), tri.GetCentroid() + new Vector3(tri.GetNormalPlane().x, tri.GetNormalPlane().y, tri.GetNormalPlane().z));
    }

    private void Start()
    {
        KDopNode node = new KDopNode();
        node.data.leftNode = 101;
        node.data.rightNode = 102;
        Debug.LogError(node.data.leftNode);
        Debug.LogError(node.data.rightNode);
        Debug.LogError(node.data.numTriangles);
        Debug.LogError(node.data.startIndex);

    }
}
