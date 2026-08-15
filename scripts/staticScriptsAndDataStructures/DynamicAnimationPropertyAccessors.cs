using Godot;
using System;
using System.Collections.Generic;

public interface IDynamicAnimationPropertyAccessor
{
	string TargetName { get; }
	Type TargetType { get; }
	bool Supports(string targetName, string propertyName, Type targetType);
	void SetValue(object target, string propertyName, float value);
}

public static class DynamicAnimationPropertyAccessorRegistry
{
	static readonly Dictionary<string, IDynamicAnimationPropertyAccessor> accessors = new Dictionary<string, IDynamicAnimationPropertyAccessor>();

	public static void Register(IDynamicAnimationPropertyAccessor accessor)
	{
		string key = CreateKey(accessor.TargetName, accessor.TargetType);
		accessors[key] = accessor;
	}

	public static IDynamicAnimationPropertyAccessor Get(string targetName, string propertyName, Type targetType)
	{
		string key = CreateKey(targetName, targetType);
		if (!accessors.TryGetValue(key, out IDynamicAnimationPropertyAccessor accessor))
		{
			foreach (IDynamicAnimationPropertyAccessor candidate in accessors.Values)
			{
				if (candidate.Supports(targetName, propertyName, targetType))
				{
					return candidate;
				}
			}

			return null;
		}

		return accessor.Supports(targetName, propertyName, targetType) ? accessor : null;
	}

	static string CreateKey(string targetName, Type targetType)
	{
		return targetName + "|" + targetType.FullName;
	}
}

public interface ICircularSpriteRotateItemAnimationProperties
{
	void SetPositionX(Sprite2D target, float value);
	void SetZIndex(Sprite2D target, float value);
	void SetScaleX(Sprite2D target, float value);
	void SetScaleY(Sprite2D target, float value);
}

public interface IHorizontalMoveItemAnimationProperties
{
	void SetPositionX(Node2D target, float value);
}

public static class DynamicAnimationGeneratedAccessors
{
	static bool registered;

	public static void RegisterAll()
	{
		if (registered)
		{
			return;
		}

		DynamicAnimationPropertyAccessorRegistry.Register(new CircularSpriteRotateItemAnimationAccessor());
		DynamicAnimationPropertyAccessorRegistry.Register(new HorizontalMoveItemAnimationAccessor());
		registered = true;
	}
}

public class HorizontalMoveItemAnimationAccessor : IDynamicAnimationPropertyAccessor, IHorizontalMoveItemAnimationProperties
{
	public string TargetName => "item";
	public Type TargetType => typeof(Node2D);

	public bool Supports(string targetName, string propertyName, Type targetType)
	{
		return targetName == TargetName &&
			TargetType.IsAssignableFrom(targetType) &&
			propertyName == "Position.X";
	}

	public void SetValue(object target, string propertyName, float value)
	{
		if (propertyName == "Position.X" && target is Node2D node)
		{
			SetPositionX(node, value);
		}
	}

	public void SetPositionX(Node2D target, float value)
	{
		target.Position = target.Position with { X = value };
	}
}

public class CircularSpriteRotateItemAnimationAccessor : IDynamicAnimationPropertyAccessor, ICircularSpriteRotateItemAnimationProperties
{
	public string TargetName => "item";
	public Type TargetType => typeof(Sprite2D);

	public bool Supports(string targetName, string propertyName, Type targetType)
	{
		if (targetName != TargetName || !TargetType.IsAssignableFrom(targetType))
		{
			return false;
		}

		return propertyName == "Position.X"
			|| propertyName == "ZIndex"
			|| propertyName == "Scale.X"
			|| propertyName == "Scale.Y";
	}

	public void SetValue(object target, string propertyName, float value)
	{
		Sprite2D sprite = target as Sprite2D;
		if (sprite == null)
		{
			return;
		}

		switch (propertyName)
		{
			case "Position.X":
				SetPositionX(sprite, value);
				break;
			case "ZIndex":
				SetZIndex(sprite, value);
				break;
			case "Scale.X":
				SetScaleX(sprite, value);
				break;
			case "Scale.Y":
				SetScaleY(sprite, value);
				break;
		}
	}

	public void SetPositionX(Sprite2D target, float value)
	{
		target.Position = target.Position with { X = value };
	}

	public void SetZIndex(Sprite2D target, float value)
	{
		target.ZIndex = (int)value;
	}

	public void SetScaleX(Sprite2D target, float value)
	{
		target.Scale = target.Scale with { X = value };
	}

	public void SetScaleY(Sprite2D target, float value)
	{
		target.Scale = target.Scale with { Y = value };
	}
}
