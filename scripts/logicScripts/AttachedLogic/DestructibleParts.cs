using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

// The generic layer for units assembled from independent sprite parts.  It is
// intentionally independent of a particular combat game type: projectiles,
// melee, explosions, and scripted removal all submit the same DamageRequest.
public enum DamageCause { Projectile, Melee, Explosion, WeakSpot, ScriptedRemoval, Tear }
public enum PartState { Intact, Damaged, Destroyed, Removed }
public enum PartEffectType { DisableAbility, ArmourPenalty, MovementMultiplier, Bleed, Explosion }

public sealed class DamageRequest
{
	public DamageCause Cause { get; init; } = DamageCause.Projectile;
	public float Damage { get; init; } = 1f;
	public int MaterialDamage { get; init; } = 1;
	public Vector2 WorldImpact { get; init; }
	public Vector2 ImpactDirection { get; init; }
	public float StructuralImpulse { get; init; }
}

public readonly struct SpriteDamageResult
{
	public SpriteDamageResult(bool hitMaterial, bool cellDestroyed, Vector2I cell, int cellHealthBefore, int cellHealthAfter)
	{
		HitMaterial = hitMaterial;
		CellDestroyed = cellDestroyed;
		Cell = cell;
		CellHealthBefore = cellHealthBefore;
		CellHealthAfter = cellHealthAfter;
	}
	public bool HitMaterial { get; }
	public bool CellDestroyed { get; }
	public Vector2I Cell { get; }
	public int CellHealthBefore { get; }
	public int CellHealthAfter { get; }
	public static SpriteDamageResult None => new(false, false, new Vector2I(-1, -1), 0, 0);
}

public readonly struct DamageApplicationResult
{
	public DamageApplicationResult(SpriteDamageResult sprite, DamageableUnitPart part, float partDamage)
	{
		Sprite = sprite;
		Part = part;
		PartDamage = partDamage;
	}
	public SpriteDamageResult Sprite { get; }
	public DamageableUnitPart Part { get; }
	public float PartDamage { get; }
	public bool HitMaterial => Sprite.HitMaterial;
	public static DamageApplicationResult None => new(SpriteDamageResult.None, null, 0);
}

public sealed class PartEffectDefinition
{
	public PartEffectType Type { get; init; }
	public float Value { get; init; } = 1f;
}

// Serializable authoring data. It is stored as a unit trait in the database,
// while these node components hold only the live scene state.
public sealed class DestructibleAssemblyDefinition
{
	public float MaxBodyHealth { get; set; } = 100f;
	public List<DestructiblePartDefinition> Parts { get; set; } = new();
	public List<StructuralJointDefinition> Joints { get; set; } = new();
}
public sealed class DestructiblePartDefinition
{
	public string Id { get; set; } = "";
	public float PositionX { get; set; }
	public float PositionY { get; set; }
	public float MaxHitPoints { get; set; } = 10;
	public float BodyDamageMultiplier { get; set; }
	public int GridWidth { get; set; } = 1;
	public int GridHeight { get; set; } = 1;
	public int PixelSize { get; set; } = 8;
	public int CellHitPoints { get; set; } = 1;
	public string MaterialColor { get; set; } = "ffffff";
	public float WeakSpotX { get; set; }
	public float WeakSpotY { get; set; }
	public float WeakSpotWidth { get; set; }
	public float WeakSpotHeight { get; set; }
	public float WeakSpotMultiplier { get; set; } = 1;
	public List<PartEffectDefinition> Effects { get; set; } = new();
}
public sealed class StructuralJointDefinition
{
	public string ParentPartId { get; set; } = "";
	public string ChildPartId { get; set; } = "";
	public float ChildLoad { get; set; } = 0.5f;
	public float BreakImpulse { get; set; } = 4f;
	public List<StructuralAnchorDefinition> ParentAnchors { get; set; } = new();
}
public sealed class StructuralAnchorDefinition { public int X { get; set; } public int Y { get; set; } }

// A unit-level coordinator. It holds mutable combat state that part effects
// can change without a part reaching into a game-specific controller.
public partial class DamageableUnitAssembly : Node2D
{
	readonly List<DamageableUnitPart> parts = new();
	readonly List<StructuralJoint> joints = new();
	public IReadOnlyList<DamageableUnitPart> Parts => parts;
	public IReadOnlyList<StructuralJoint> Joints => joints;
	public float MaxBodyHealth { get; set; } = 100f;
	public float BodyHealth { get; private set; } = 100f;
	public float Armour { get; private set; } = 10f;
	public float MovementMultiplier { get; private set; } = 1f;
	public bool AbilityEnabled { get; private set; } = true;
	public int BleedEffectCount { get; private set; }
	public int ExplosionEffectCount { get; private set; }

	public override void _Ready()
	{
		BodyHealth = MaxBodyHealth;
	}

	public void RegisterPart(DamageableUnitPart part)
	{
		if (part != null && !parts.Contains(part)) parts.Add(part);
	}
	public void UnregisterPart(DamageableUnitPart part) => parts.Remove(part);
	public void RegisterJoint(StructuralJoint joint)
	{
		if (joint != null && !joints.Contains(joint)) joints.Add(joint);
	}
	public void UnregisterJoint(StructuralJoint joint) => joints.Remove(joint);

	public void ApplyBodyDamage(float amount)
	{
		if (amount > 0) BodyHealth = Mathf.Max(0, BodyHealth - amount);
	}

	public void ApplyEffect(PartEffectDefinition effect)
	{
		if (effect == null) return;
		switch (effect.Type)
		{
			case PartEffectType.DisableAbility: AbilityEnabled = false; break;
			case PartEffectType.ArmourPenalty: Armour = Mathf.Max(0, Armour - effect.Value); break;
			case PartEffectType.MovementMultiplier: MovementMultiplier *= Mathf.Max(0, effect.Value); break;
			case PartEffectType.Bleed: BleedEffectCount++; break;
			case PartEffectType.Explosion: ExplosionEffectCount++; break;
		}
	}

	internal void EvaluateJoints(DamageableUnitPart changedPart, DamageRequest request)
	{
		foreach (StructuralJoint joint in joints.ToArray()) joint.Evaluate(changedPart, request);
	}
}

// A component with a single associated damageable sprite. More elaborate
// enemies simply contain several of these under one assembly.
public partial class DamageableUnitPart : Node2D
{
	bool effectsTriggered;
	public string PartId { get; set; } = "";
	public DamageableSprite MaterialSprite { get; private set; }
	public DamageableUnitAssembly Assembly { get; private set; }
	public float MaxHitPoints { get; set; } = 10f;
	public float HitPoints { get; private set; } = 10f;
	public float BodyDamageMultiplier { get; set; } = 0f;
	public float DamageMultiplier { get; set; } = 1f;
	public Rect2 WeakSpotLocalRect { get; set; } = new Rect2();
	public float WeakSpotMultiplier { get; set; } = 1f;
	public bool DetachWhenDestroyed { get; set; }
	public bool RemoveWhenDestroyed { get; set; }
	public PartState State { get; private set; } = PartState.Intact;
	public bool IsDetached { get; private set; }
	public Vector2 DetachedVelocity { get; private set; }
	public List<PartEffectDefinition> Effects { get; } = new();

	public void ConfigureSprite(DamageableSprite sprite)
	{
		MaterialSprite = sprite;
		if (sprite != null && sprite.GetParent() != this) AddChild(sprite);
	}

	public override void _Ready()
	{
		Assembly = GetParent() as DamageableUnitAssembly;
		Assembly?.RegisterPart(this);
		HitPoints = MaxHitPoints;
		if (MaterialSprite == null) MaterialSprite = GetChildren().OfType<DamageableSprite>().FirstOrDefault();
	}
	public override void _ExitTree() => Assembly?.UnregisterPart(this);
	public override void _Process(double delta)
	{
		if (!IsDetached || DetachedVelocity.IsZeroApprox()) return;
		GlobalPosition += DetachedVelocity * (float)delta;
		DetachedVelocity = DetachedVelocity.MoveToward(Vector2.Zero, 100f * (float)delta);
	}

	public float GetDamageMultiplier(Vector2 worldImpact)
	{
		if (WeakSpotMultiplier > 1f && WeakSpotLocalRect.HasPoint(ToLocal(worldImpact))) return WeakSpotMultiplier;
		return 1f;
	}

	internal float ApplyMaterialDamage(DamageRequest request)
	{
		if (State is PartState.Destroyed or PartState.Removed) return 0;
		float applied = Mathf.Max(0, request.Damage) * DamageMultiplier * GetDamageMultiplier(request.WorldImpact);
		if (applied <= 0) return 0;
		HitPoints = Mathf.Max(0, HitPoints - applied);
		if (State == PartState.Intact) State = PartState.Damaged;
		Assembly?.ApplyBodyDamage(applied * BodyDamageMultiplier);
		if (HitPoints <= 0) Destroy(request);
		Assembly?.EvaluateJoints(this, request);
		return applied;
	}

	public void ApplyDirectDamage(DamageRequest request)
	{
		ApplyMaterialDamage(request);
	}
	public void Remove(DamageRequest request = null)
	{
		if (State == PartState.Removed) return;
		State = PartState.Removed;
		TriggerEffects();
		QueueFree();
	}

	void Destroy(DamageRequest request)
	{
		if (State == PartState.Destroyed || State == PartState.Removed) return;
		State = PartState.Destroyed;
		TriggerEffects();
		if (RemoveWhenDestroyed) { Remove(request); return; }
		if (DetachWhenDestroyed) Detach(request.ImpactDirection * request.StructuralImpulse);
	}
	internal void Detach(Vector2 impulse)
	{
		if (IsDetached || State == PartState.Removed) return;
		IsDetached = true;
		TriggerEffects();
		DetachedVelocity = impulse;
		Node detachedRoot = mAccess.gameSessionManager?.Current?.WorldRoot;
		Node oldParent = GetParent();
		if (detachedRoot != null && GodotObject.IsInstanceValid(detachedRoot) && oldParent != null && oldParent != detachedRoot)
		{
			Vector2 position = GlobalPosition;
			oldParent.RemoveChild(this);
			detachedRoot.AddChild(this);
			GlobalPosition = position;
		}
	}
	void TriggerEffects()
	{
		if (effectsTriggered) return;
		effectsTriggered = true;
		foreach (PartEffectDefinition effect in Effects) Assembly?.ApplyEffect(effect);
	}
}

// A joint is attached to a small, explicitly authored set of material cells.
// It fails if the remaining cross-section cannot support its child load, which
// produces a tree-like tear before every attachment pixel is cut away.
public partial class StructuralJoint : Node
{
	readonly List<Vector2I> parentAnchorCells = new();
	public DamageableUnitPart ParentPart { get; private set; }
	public DamageableUnitPart ChildPart { get; private set; }
	public float ChildLoad { get; set; } = 0.5f;
	public float BreakImpulse { get; set; } = 4f;
	public bool IsBroken { get; private set; }
	public float RemainingCapacity { get; private set; } = 1f;
	public IReadOnlyList<Vector2I> ParentAnchorCells => parentAnchorCells;

	public void Configure(DamageableUnitPart parentPart, DamageableUnitPart childPart, IEnumerable<Vector2I> anchors)
	{
		ParentPart = parentPart;
		ChildPart = childPart;
		parentAnchorCells.Clear();
		if (anchors != null) parentAnchorCells.AddRange(anchors);
	}
	public override void _Ready() => (GetParent() as DamageableUnitAssembly)?.RegisterJoint(this);
	public override void _ExitTree() => (GetParent() as DamageableUnitAssembly)?.UnregisterJoint(this);

	internal void Evaluate(DamageableUnitPart changedPart, DamageRequest request)
	{
		if (IsBroken || changedPart == null || (changedPart != ParentPart && changedPart != ChildPart)) return;
		if (ParentPart?.MaterialSprite == null || ChildPart == null || parentAnchorCells.Count == 0) return;
		int intact = parentAnchorCells.Count(cell => ParentPart.MaterialSprite.IsCellSolid(cell));
		RemainingCapacity = (float)intact / parentAnchorCells.Count;
		float impulseLimit = BreakImpulse * RemainingCapacity;
		if (RemainingCapacity <= 0 || RemainingCapacity < ChildLoad || request.StructuralImpulse > impulseLimit)
		{
			IsBroken = true;
			ChildPart.Detach(request.ImpactDirection * Mathf.Max(request.StructuralImpulse, ChildLoad * 120f));
		}
	}
}
