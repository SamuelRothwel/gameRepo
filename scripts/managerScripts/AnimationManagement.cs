using Godot;
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