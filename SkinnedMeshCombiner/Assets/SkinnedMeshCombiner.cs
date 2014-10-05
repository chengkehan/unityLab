using UnityEngine;
using System.Collections.Generic;

public class SkinnedMeshCombiner : MonoBehaviour
{
    public Material mtrl = null;

    void Start()
    {
        Animation[] animations = GetComponentsInChildren<Animation>();
        foreach (Animation animation in animations)
        {
            animation.cullingType = AnimationCullingType.AlwaysAnimate;
        }

        SkinnedMeshRenderer[] smRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        List<Transform> bones = new List<Transform>();
        List<BoneWeight> boneWeights = new List<BoneWeight>();
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        Dictionary<Transform, Matrix4x4> ts = new Dictionary<Transform, Matrix4x4>();

        int boneOffset = 0;
        for (int s = 0; s < smRenderers.Length; s++)
        {
            SkinnedMeshRenderer smr = smRenderers[s];
            BoneWeight[] meshBoneweight = smr.sharedMesh.boneWeights;
            foreach (BoneWeight bw in meshBoneweight)
            {
                BoneWeight bWeight = bw;

                bWeight.boneIndex0 += boneOffset;
                bWeight.boneIndex1 += boneOffset;
                bWeight.boneIndex2 += boneOffset;
                bWeight.boneIndex3 += boneOffset;

                boneWeights.Add(bWeight);
            }
            boneOffset += smr.bones.Length;

            Transform[] meshBones = smr.bones;
            int meshBoneIndex = 0;
            foreach (Transform bone in meshBones)
            {
                bones.Add(bone);
                ts.Add(bone, smr.sharedMesh.bindposes[meshBoneIndex] * smr.worldToLocalMatrix);
                ++meshBoneIndex;
            }

            CombineInstance ci = new CombineInstance();
            ci.mesh = smr.sharedMesh;
            ci.transform = smr.transform.localToWorldMatrix;
            combineInstances.Add(ci);

            Object.Destroy(smr.gameObject);
        }

        List<Matrix4x4> bindposes = new List<Matrix4x4>();

        for (int b = 0; b < bones.Count; b++)
        {
            //bindposes.Add(bones[b].worldToLocalMatrix * transform.worldToLocalMatrix);
            //bindposes.Add(bones[b].worldToLocalMatrix);
            bindposes.Add(ts[bones[b]]);
        }

        SkinnedMeshRenderer r = gameObject.AddComponent<SkinnedMeshRenderer>();
        r.sharedMesh = new Mesh();
        r.sharedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
        r.sharedMaterial = mtrl;
        r.bones = bones.ToArray();
        r.sharedMesh.boneWeights = boneWeights.ToArray();
        r.sharedMesh.bindposes = bindposes.ToArray();
        r.sharedMesh.RecalculateBounds();
    }
}