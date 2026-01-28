using Godot;
using System.Collections.Generic;
using System;
using System.Linq;

public partial class GunBarrelAnimation : Node, AnimationSet
{
	public List<(string, (float, (string, Tuple<double, Variant>[])[]))> library { get; set; }
	public void setup(Dictionary<string, AudioStream> audioFiles)
	{
		string animationName = "Spin";
		library = new List<(string, (float, (string, Tuple<double, Variant>[])[]))>
		{
			(
				animationName, (1f,
				new (string, Tuple<double, Variant>[])[]
				{
					(
						"Value|Gun:offset",
						new Tuple<double, Variant>[]
						{
							new Tuple<double, Variant>(1, new Vector2(0, 1)),
							new Tuple<double, Variant>(0, new Vector2(0, 0))
						}
					),
					(
						"Method|Gun",
						new Tuple<double, Variant>[]
						{
							new Tuple<double, Variant>(0, mAccess.animationManagement.methodCaller("setTexture", new Variant[] { animationName, 0 })),
							new Tuple<double, Variant>(0.5, mAccess.animationManagement.methodCaller("setTexture", new Variant[] { animationName, 1 })),
						}
					),
					(
						"Audio|Gun/audio",
						new Tuple<double, Variant>[]
						{
							new Tuple<double, Variant>(0, mAccess.animationManagement.audioCaller(audioFiles["metalPipe.mp3"], 0, 0))
						}
					),
					(
						"Method|Gun",
						new Tuple<double, Variant>[]
						{
							new Tuple<double, Variant>(0.5, mAccess.animationManagement.methodCaller("spawnBullet"))
						}
					),
				})
			)
		};

		animationName = "coolMinigun";
		library.Add(new
			(
				animationName, (1f,
				new (string, Tuple<double, Variant>[])[]
				{
					(
						"Value|Gun:texture",
						new Tuple<double, Variant>[]
						{
							new Tuple<double, Variant>(0, mAccess.animationManagement.methodCaller("setTexture", new Variant[] { animationName, 0 })),
							new Tuple<double, Variant>(0.5, mAccess.animationManagement.methodCaller("setTexture", new Variant[] { animationName, 1 })),
						}
					),
				})
			)
		);
		animationName = "shootSniper";
		library.Add(new
			(
				animationName, (1f,
				new (string, Tuple<double, Variant>[])[]
				{
					(
						"Value|Gun:offset",
						new Tuple<double, Variant>[]
						{
							new Tuple<double, Variant>(0, new Vector2(0, 2)),
							new Tuple<double, Variant>(1, new Vector2(0, 0))
						}
					),
					(
						"Method|Gun",
						new Tuple<double, Variant>[]
						{
							new Tuple<double, Variant>(0, mAccess.animationManagement.methodCaller("setTexture", new Variant[] { animationName, 0 })),
							new Tuple<double, Variant>(0.5, mAccess.animationManagement.methodCaller("setTexture", new Variant[] { animationName, 1 })),
						}
					),
					(
						"Audio|Gun/audio",
						new Tuple<double, Variant>[]
						{
							new Tuple<double, Variant>(0, mAccess.animationManagement.audioCaller(audioFiles["metalPipe.mp3"], 0, 0))
						}
					),
					(
						"Method|Gun",
						new Tuple<double, Variant>[]
						{
							new Tuple<double, Variant>(0, mAccess.animationManagement.methodCaller("spawnBullet"))
						}
					),
				})
			)
		);
	}
}
