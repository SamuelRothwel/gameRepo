using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public partial class staticCreator : BackgroundLogicNode
{
	public override void setup()
	{
		Gun.guns = returnTemplates<gunTemplate>();
		projectile.physicsTemplates = returnTemplates<projectilePhysicsTemplate>();
		projectile.effectTemplates = returnTemplates<projectileEffectsTemplate>();
	}

	public Dictionary<string, T> returnTemplates<T>()
	{
		return typeof(T).GetNestedTypes().ToDictionary(x => x.Name, x => (T)Activator.CreateInstance(x));
	}
}
