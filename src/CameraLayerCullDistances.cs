using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraLayerCullDistances : MonoBehaviour
{
	public static List<CameraLayerCullDistances> cameraLayerCullDistancesList = null;

	[SerializedField]
	public float cullDistance = 0.0f;

	[SerializedField]
	public LayerMask layer;

#if UNITY_EDITOR
	[HideInInspector]
	public bool refresh = false;

	public Color debugColor = Color.yellow;

	public bool debug = false;
#endif

	private Camera camera = null;

	private void Start()
	{
		if(cameraLayerCullDistancesList == null)
		{
			cameraLayerCullDistancesList = new List<CameraLayerCullDistances>();
		}
		cameraLayerCullDistancesList.Add(this);

		GetCameraComponent();
		ClearLayerCullDistances();
		SetLayerCullDistances();
	}

#if UNITY_EDITOR
	private void Update()
	{
		if(refresh)
		{
			refresh = false;
			GetCameraComponent();
			ClearLayerCullDistances();
			SetLayerCullDistances();
		}
	}
#endif

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if(debug && camera != null)
		{
			float distance = Mathf.Approximately(cullDistance, 0.0f) ? camera.farClipPlane : cullDistance;
			Gizmos.matrix = camera.transform.localToWorldMatrix;
			Gizmos.color = debugColor;
			Gizmos.DrawFrustum(Vector3.zero, camera.fieldOfView, distance, camera.nearClipPlane, camera.aspect);
		}
	}
#endif

	private void SetLayerCullDistances()
	{
		if(camera != null)
		{
			float[] distances = camera.layerCullDistances == null ? new float[32] : camera.layerCullDistances;
			for(int i = 0; i < 32; ++i)
			{
				if(((layer.value >> i) & 1) == 1)
				{
					distances[i] = cullDistance;
				}
			}
			camera.layerCullDistances = distances;
		}
	}

	private void ClearLayerCullDistances()
	{
		if(camera != null && 
		   cameraLayerCullDistancesList != null && 
		   cameraLayerCullDistancesList.Count > 0 && 
		   this == cameraLayerCullDistancesList[0])
		{
			camera.layerCullDistances = new float[32];
		}
	}

	private void GetCameraComponent()
	{
		if(camera == null)
		{
			camera = GetComponent<Camera>();
		}
	}
}

