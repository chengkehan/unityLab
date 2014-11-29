using UnityEngine;
using System.Collections;

public class SequenceFrameMeshAnimation
{
	public SequenceFrameMeshAnimationSampler sampler
	{
		get
		{
			return player == null ? null : player.sampler;
		}
	}

	public SequenceFrameMeshAnimationPlayer player
	{
		private set;
		get;
	}

	public SequenceFrameMeshAnimation(SequenceFrameMeshAnimationSampler sampler, Material material, GameObject gameObject)
	{
		player = new SequenceFrameMeshAnimationPlayer(sampler, material, gameObject);
	}

	public SequenceFrameMeshAnimation(Animation animation, Material material, GameObject gameObject, int fps)
	{
		SequenceFrameMeshAnimationSampler sampler = new SequenceFrameMeshAnimationSampler();
		sampler.fps = fps;
		sampler.SampleAnimation(animation);
		player = new SequenceFrameMeshAnimationPlayer(sampler, material, gameObject);
	}
}
