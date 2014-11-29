using UnityEngine;
using System.Collections;

public class SequenceFrameMeshAnimationPlayer
{
	public SequenceFrameMeshAnimationSampler sampler
	{
		private set;
		get;
	}

	public Material material
	{
		private set;
		get;
	}

	public float time
	{
		set;
		get;
	}

	private bool m_isPlaying = false;
	public bool isPlaying
	{
		private set
		{
			m_isPlaying = value;
		}
		get
		{
			return m_isPlaying;
		}
	}
	
	public float speed
	{
		set;
		get;
	}
	
	public string currentClipName
	{
		set;
		get;
	}

	public GameObject gameObject
	{
		set;
		get;
	}

	private SequenceFrameMeshAnimationSampler.Clip clip = null;

	private Mesh mesh = null;

	private MeshFilter meshFilter = null;

	private MeshRenderer meshRenderer = null;

	private int currentIndex = -1;

	public SequenceFrameMeshAnimationPlayer(SequenceFrameMeshAnimationSampler sampler, Material material, GameObject gameObject)
	{
		this.speed = 1;
		this.sampler = sampler;
		this.material = material;
		if(gameObject != null)
		{
			this.gameObject = gameObject;
			meshFilter = gameObject.AddComponent<MeshFilter>();
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
		}
	}

	public bool Play(string clipName)
	{
		if(gameObject == null)
		{
			return false;
		}

		if(sampler == null)
		{
			return false;
		}

		if(sampler.animation == null)
		{
			return false;
		}

		if(string.IsNullOrEmpty(clipName))
		{
			return false;
		}

		if(sampler.animation.clips != null && sampler.animation.clips.TryGetValue(clipName, out clip))
		{
			currentClipName = clipName;
			time = 0;
			isPlaying = true;
			return true;
		}
		else
		{
			return false;
		}
	}

	public void Stop()
	{
		isPlaying = false;
		time = 0;
	}

	public void Pause()
	{
		isPlaying = false;
	}

	public void Resume()
	{
		isPlaying = true;
	}

	public void Update(float deltaTime)
	{
		if(!isPlaying)
		{
			return;
		}

		if(gameObject == null)
		{
			return;
		}

		if(sampler == null)
		{
			return;
		}

		if(sampler.animation == null)
		{
			return;
		}

		if(clip == null)
		{
			return;
		}

		if(mesh == null)
		{
			if(clip.keyframes == null || clip.keyframes.Count == 0)
			{
				return;
			}

			mesh = new Mesh();
			mesh.vertices = clip.keyframes[0].vertices;
			mesh.triangles = sampler.animation.triangles;
			mesh.normals = sampler.animation.normals;
			mesh.uv = sampler.animation.uv;
			meshRenderer.sharedMaterial = material;
			meshFilter.mesh = mesh;
		}

		int index = 0;
		bool stopAtOnce = false;
		if(clip.wrapMode == WrapMode.Default || clip.wrapMode == WrapMode.Loop)
		{
			index = ((int)(time * sampler.fps)) % clip.keyframes.Count;
		}
		else if(clip.wrapMode == WrapMode.Once)
		{
			index = (int)(time * sampler.fps);
			if(index >= clip.keyframes.Count)
			{
				stopAtOnce = true;
			}
		}
		else
		{
			Debug.LogError("Unsupported wrapMode:" + clip.wrapMode);
		}

		if(index != currentIndex)
		{
			currentIndex = index;
			mesh.vertices = clip.keyframes[currentIndex].vertices;
		}

		if(stopAtOnce)
		{
			Stop();
		}

		time += deltaTime * speed;
	}
}
