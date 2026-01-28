using Godot;
using System;
using System.Collections.Generic;

public interface AnimationSet
{
	public StringName Name { get; }
	public List<(string, (float, (string, Tuple<double, Variant>[])[]))> library { get; set; }
	public void setup(Dictionary<string, AudioStream> audioFiles);
}
