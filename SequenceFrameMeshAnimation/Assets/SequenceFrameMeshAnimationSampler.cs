using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SequenceFrameMeshAnimationSampler 
{
	public int fps
	{
		set;
		get;
	}

	public MeshAnimation animation
	{
		private set;
		get;
	}

	public SequenceFrameMeshAnimationSampler()
	{
		fps = 15;
	}

	public bool SampleAnimation(UnityEngine.Animation animation)
	{
		if(animation == null)
		{
			return false;
		}

		SkinnedMeshRenderer skinnedMeshRenderer = animation.GetComponentInChildren<SkinnedMeshRenderer>();
		if(skinnedMeshRenderer == null)
		{
			return false;
		}

		Mesh mesh = skinnedMeshRenderer.sharedMesh;
		if(mesh == null)
		{
			return false;
		}

		this.animation = new MeshAnimation();
		this.animation.triangles = mesh.triangles;
		this.animation.normals = mesh.normals;
		this.animation.uv = mesh.uv;

		List<string> clipNames = new List<string>();
		foreach(AnimationState animationState in animation)
		{
			clipNames.Add(animationState.name);
		}

		Mesh bakedMesh = new Mesh();
		int numClips = clipNames.Count;
		for(int i = 0; i < numClips; ++i)
		{
			string clipName = clipNames[i];
			if(this.animation.clips.ContainsKey(clipName))
			{
				Debug.LogError("Repeated name of AnimationClip:" + clipName);
				continue;
			}

			animation.Play(clipName);
			{
				AnimationState animationState = animation[clipName];

				Clip clip = new Clip();
				clip.wrapMode = animationState.wrapMode;
				this.animation.clips.Add(clipName, clip);

				float length = animationState.length;
				float step = 1.0f / fps;
				for(float time = 0; time < length; time += step)
				{
					Keyframe keyframe = new Keyframe();
					clip.keyframes.Add(keyframe);

					animationState.time = time;
					animation.Sample();

					skinnedMeshRenderer.BakeMesh(bakedMesh);
					keyframe.vertices = bakedMesh.vertices;
				}
			}
			animation.Stop(clipName);
		}

		return true;
	}

	public class MeshAnimation
	{
		public int[] triangles = null;

		public Vector2[] uv = null;

		public Vector3[] normals = null;

		public Dictionary<string, Clip> clips = new Dictionary<string, Clip>();
	}

	public class Clip
	{
		public WrapMode wrapMode = WrapMode.Loop;

		public List<Keyframe> keyframes = new List<Keyframe>();
	}

	public class Keyframe
	{
		public Vector3[] vertices = null;
	}
}
