using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
#if UNITY_3_5
[CustomEditor(typeof(CameraLayerCullDistances))]
#else
[CustomEditor(typeof(CameraLayerCullDistances), true)]
#endif
public class CameraLayerCullDistancesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Refresh"))
        {
            int length = CameraLayerCullDistances.cameraLayerCullDistancesList == null ? 0 : CameraLayerCullDistances.cameraLayerCullDistancesList.Count;
            for (int i = 0; i < length; ++i)
            {
                CameraLayerCullDistances.cameraLayerCullDistancesList[i].refresh = true;
            }
        }
    }
}
