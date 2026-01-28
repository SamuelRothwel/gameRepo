using Godot;
using System;
using System.Collections.Generic;

public partial class BackgroundLogicNode : Node
{
	public virtual void setup() { }
	public string[] creationFlags = new string[0];
	public virtual void creationFlag(Node node) { }
	public virtual void preProcess(Node entity) {}
}
