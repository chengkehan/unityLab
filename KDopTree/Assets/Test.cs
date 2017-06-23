using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public MeshFilter mf = null;

    [Range(0, 20)]
    public int depth = 0;

    private KDopTree kDopTree = null;

    private Color[] colors = null;

    private void OnDrawGizmos()
    {
        Gizmos.matrix = mf.transform.localToWorldMatrix;

        if(kDopTree != null)
        {
            List<KDopNode> nodes = kDopTree.nodes;
            for(int nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex)
            {
                if(nodeIndex == depth)
                {
                    break;
                }
                Gizmos.color = colors[nodeIndex];
                KDopNode node = kDopTree.nodes[nodeIndex];
                KDopBounds bounds = node.boundingVolumes;
                Gizmos.DrawWireCube(bounds.Center, bounds.Size);
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
        kDopTree.Build(mf.mesh);
    }
}
