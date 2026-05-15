using Godot;
using coolbeats.scripts.staticScriptsAndDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.VisualBasic;

public partial class AnimationManagement : managerNode
{
	[Export] public PackedScene packedAnimations;
	public Dictionary<string, AnimationLibrary> animationSets = new Dictionary<string, AnimationLibrary>();
	public Dictionary<string, DynamicAnimationDefinition> dynamicAnimationDefinitions = new Dictionary<string, DynamicAnimationDefinition>();
	List<DynamicAnimator> activeDynamicAnimators = new List<DynamicAnimator>();
	public Dictionary<string, int> trackTypeIndex = new Dictionary<string, int>
	{ { "Value", 0 }, { "Position3D", 1 }, {"Rotation3D", 2 }, {"Scale3D", 3 }, { "BlendShape", 4 }, { "Method", 5 }, { "Bezier", 6 }, { "Audio", 7 }, { "Animation", 8 } };
	public override void setup()
	{
		Node animationScene = packedAnimations.Instantiate();
		Dictionary<string, AudioStream> audioFiles = fileSearch.getAudio("");
		foreach (AnimationSet node in animationScene.GetChildren())
		{
			node.setup(audioFiles);
			CreateAnimationLibrary(node.Name, node.library.ToArray());
		}
		RegisterDynamicAnimation(new DynamicAnimationDefinition
		{
			Name = "CircularSpriteRotate",
			Duration = 1f,
			RotationDegrees = 120f
		});
	}

	public override void _Process(double delta)
	{
		for (int i = activeDynamicAnimators.Count - 1; i >= 0; i--)
		{
			DynamicAnimator animator = activeDynamicAnimators[i];
			animator.Process(delta);
			if (animator.IsComplete)
			{
				activeDynamicAnimators.RemoveAt(i);
			}
		}
	}

	public void RegisterDynamicAnimation(DynamicAnimationDefinition definition)
	{
		dynamicAnimationDefinitions[definition.Name] = definition;
	}

	public DynamicAnimationDefinition SaveCircularSpriteRotateAnimation(string name, float duration, float rotationDegrees)
	{
		DynamicAnimationDefinition definition = new DynamicAnimationDefinition
		{
			Name = name,
			Duration = duration,
			RotationDegrees = rotationDegrees
		};
		RegisterDynamicAnimation(definition);
		mAccess.entityFrameworkManager?.SaveAnimation(new StoredAnimation
		{
			Name = name,
			AnimationType = "CircularSpriteRotate",
			Duration = duration,
			Parameters = new List<StoredAnimationParameter>
			{
				new StoredAnimationParameter
				{
					Key = "rotationDegrees",
					ValueType = "number",
					ValueJson = System.Text.Json.JsonSerializer.Serialize(rotationDegrees)
				}
			}
		});
		return definition;
	}

	public void RegisterStoredDynamicAnimations(IEnumerable<StoredAnimation> animations)
	{
		foreach (StoredAnimation animation in animations)
		{
			if (animation.AnimationType != "CircularSpriteRotate")
			{
				continue;
			}

			RegisterDynamicAnimation(new DynamicAnimationDefinition
			{
				Name = animation.Name,
				Duration = animation.Duration,
				RotationDegrees = ReadFloatParameter(animation.Parameters, "rotationDegrees", 120f)
			});
		}
	}

	float ReadFloatParameter(IEnumerable<StoredAnimationParameter> parameters, string key, float fallback)
	{
		StoredAnimationParameter parameter = parameters.FirstOrDefault(parameter => parameter.Key == key && parameter.ValueType == "number");
		if (parameter == null)
		{
			return fallback;
		}

		try
		{
			return System.Text.Json.JsonSerializer.Deserialize<float>(parameter.ValueJson);
		}
		catch
		{
			return fallback;
		}
	}

	public CircularSpriteRotationAnimator RotateCircularSprites(
		CircularEnumerator<Sprite2D> sprites,
		Action<string> eventHandler = null,
		float speed = 1f)
	{
		DynamicAnimationDefinition definition = dynamicAnimationDefinitions["CircularSpriteRotate"];
		CircularSpriteRotationAnimator animator = new CircularSpriteRotationAnimator(sprites, definition, eventHandler)
		{
			Speed = speed
		};
		activeDynamicAnimators.Add(animator);
		return animator;
	}
	public Godot.Collections.Dictionary methodCaller(string name, Variant[] args = null)
	{
		return new Godot.Collections.Dictionary
		{
			{ "method", name },
			{ "args", new Godot.Collections.Array ( args==null? new Variant[0] : args)}
		};
	}
	public Godot.Collections.Dictionary audioCaller(AudioStream stream, float startOffset = 0f, float endOffset = 0f)
	{
		return new Godot.Collections.Dictionary
		{
			{ "stream", stream },
			{ "start_offset", startOffset },
			{ "end_offset", endOffset }
		};
	}
	public Godot.Collections.Dictionary audioCaller(string streamPath, float startOffset = 0f, float endOffset = 0f)
	{
		AudioStream stream = GD.Load<AudioStream>(streamPath);
		return audioCaller(stream, startOffset, endOffset);
	}
	public Texture2D spriteCaller(string path)
	{
		return GD.Load<Texture2D>(path);
	}
	#region geters

	
	#endregion
	#region animationLibrary
	public void CreateAnimationLibrary(string libraryName, (string, (float, (string, Tuple<double, Variant>[])[]))[] storedAnimations)
	{
		AnimationLibrary library = new AnimationLibrary();
		for (int i = 0; i < storedAnimations.Count(); i++)
		{
			library.AddAnimation(storedAnimations[i].Item1, CreateAnimation(ref storedAnimations[i].Item2));
		}
		animationSets.Add(libraryName, library);
	}

	public Animation CreateAnimation(ref (float, (string, Tuple<double, Variant>[])[]) tracks)
	{
		Animation animation = new Animation();
		animation.Length = tracks.Item1;
		for (int i = 0; i < tracks.Item2.Count(); i++)
		{
			AddTrack(animation, ref tracks.Item2[i]);
		}
		return animation;
	}

	public void AddTrack(Animation animation, ref (string, Tuple<double, Variant>[]) track)
	{
		string[] trackType = track.Item1.Split("|");
		int trackIndex = animation.AddTrack((Animation.TrackType)trackTypeIndex[trackType[0]]);
		if (trackType.Count() > 1)
		{
			animation.TrackSetPath(trackIndex, trackType[1]);
		}
		for (int i = 0; i < track.Item2.Count(); i++)
		{
			animation.TrackInsertKey(trackIndex, track.Item2[i].Item1, track.Item2[i].Item2);
		}
	}
	#endregion
}

public class DynamicAnimationDefinition
{
	public string Name { get; set; } = "";
	public float Duration { get; set; } = 1f;
	public float RotationDegrees { get; set; } = 120f;
}

public abstract class DynamicAnimator
{
	Action<string> eventHandler;
	public string Name { get; }
	public float Speed { get; set; } = 1f;
	public bool IsComplete { get; private set; }

	protected DynamicAnimator(string name, Action<string> eventHandler)
	{
		Name = name;
		this.eventHandler = eventHandler;
	}

	public void Process(double delta)
	{
		if (IsComplete)
		{
			return;
		}

		if (ProcessAnimation(delta * Speed))
		{
			IsComplete = true;
			eventHandler?.Invoke("finished");
		}
	}

	protected abstract bool ProcessAnimation(double scaledDelta);
}

public class CircularSpriteRotationAnimator : DynamicAnimator
{
	CircularEnumerator<Sprite2D> sprites;
	float duration;
	float rotationDegrees;
	double elapsed;
	public float CurrentRotation { get; private set; }

	public CircularSpriteRotationAnimator(
		CircularEnumerator<Sprite2D> sprites,
		DynamicAnimationDefinition definition,
		Action<string> eventHandler)
		: base(definition.Name, eventHandler)
	{
		this.sprites = sprites;
		duration = Math.Max(definition.Duration, 0.001f);
		rotationDegrees = definition.RotationDegrees;
		ApplyRotation(0);
	}

	protected override bool ProcessAnimation(double scaledDelta)
	{
		elapsed += scaledDelta;
		float progress = Math.Clamp((float)(elapsed / duration), 0f, 1f);
		ApplyRotation(progress * rotationDegrees);

		if (progress < 1f)
		{
			return false;
		}

		sprites.MoveNext();
		ApplyRotation(0);
		return true;
	}

	void ApplyRotation(float rotation)
	{
		CurrentRotation = rotation;
		IEnumerable<Sprite2D> components = sprites.loop();
		int i = 0;
		foreach (Sprite2D component in components)
		{
			float sinValue = (float)LUT.sin(i + (int)rotation);
			float cosValue = (float)LUT.cos(i + (int)rotation);
			component.ZIndex = (int)((cosValue + 1) * 1000);
			component.Position = component.Position with { X = sinValue * 5 };
			component.Scale = component.Scale with { X = cosValue * 0.1f + 0.9f, Y = cosValue * 0.1f + 0.9f };
			i += 120;
		}
	}
}
