using Godot;
using System;
using System.Collections.Generic;

public partial class BackgroundLogicNode : Node
{
	public virtual void setup() { }
	public string[] creationFlags = new string[0];
	public virtual void creationFlag(Node node) { }
	public virtual void preProcess(Node entity) {}
	// Background logic is manager-owned, while many of the nodes it observes are
	// game-session-owned.  Clear those references before the session world frees.
	public virtual void sessionStopped(GameSession session) { }
}
