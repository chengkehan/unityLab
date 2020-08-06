using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CopyAnimationClips : EditorWindow
{
    [MenuItem("Tools/Copy Clips")]
    private static void CopyClips()
    {
        EditorWindow.CreateWindow<CopyAnimationClips>().Show();
    }

    private Animation animSrc = null;

    private Animation animDest = null;

    private void OnGUI()
    {
        animSrc = EditorGUILayout.ObjectField(animSrc, typeof(Animation), true) as Animation;
        animDest = EditorGUILayout.ObjectField(animDest, typeof(Animation), true) as Animation;
        if(GUILayout.Button("Copy Clips"))
        {
            ModelImporter modelImporterSrc = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(animSrc.gameObject)) as ModelImporter;
            ModelImporter modelImporterDest = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(animDest.gameObject)) as ModelImporter;
            ModelImporterClipAnimation[] clips = modelImporterSrc.clipAnimations;
            ModelImporterClipAnimation[] newClips = new ModelImporterClipAnimation[clips.Length];
            int clipI = 0;
            foreach(var clip in clips)
            {
                ModelImporterClipAnimation newClip = new ModelImporterClipAnimation();
                newClip.name = clip.name;
                newClip.firstFrame = clip.firstFrame;
                newClip.lastFrame = clip.lastFrame;
                newClip.loop = clip.loop;
                newClip.loopPose = clip.loopPose;
                newClip.loopTime = clip.loopTime;
                newClip.wrapMode = clip.wrapMode;
                newClips[clipI] = newClip;
                ++clipI;
            }
            modelImporterDest.clipAnimations = newClips;
        }
    }
}
