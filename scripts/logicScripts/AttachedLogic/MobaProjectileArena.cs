using System;
using System.Collections.Generic;
using Godot;

// A compact, self-contained game-type implementation. It demonstrates player
// movement/shooting without coupling MOBA controls to RTS unit selection.
public partial class MobaProjectileArena : Node2D
{
	public DestructibleAssemblyDefinition TestEnemyDefinition { get; set; }
	public MobaPlayer Player { get; private set; }
	public MobaTestEnemy TestEnemy { get; private set; }
	public int ResolvedHitboxCollisions { get; private set; }
	public int AppliedSpriteDamage { get; private set; }

	public override void _Ready()
	{
		Name = "MobaArena";
		Player = new MobaPlayer { Name = "Player", Position = new Vector2(300, 330) };
		TestEnemy = new MobaTestEnemy { Name = "TestEnemy", Position = new Vector2(760, 330), Definition = TestEnemyDefinition };
		AddChild(Player);
		AddChild(TestEnemy);
	}

	public override void _UnhandledInput(InputEvent input)
	{
		if (mAccess.gameSessionManager?.Current?.HasCapability("mobaControl") != true) return;
		if (input is InputEventMouseButton mouse && mouse.Pressed)
		{
			Vector2 target = mAccess.uiManager == null ? mouse.GlobalPosition : mAccess.uiManager.toGameWindowPosition(mouse.GlobalPosition);
			if (mouse.ButtonIndex == MouseButton.Left) Player?.FireAt(target);
			if (mouse.ButtonIndex == MouseButton.Right) Player?.MoveTo(target);
		}
	}

	public MobaProjectile FireTestProjectile(Vector2 target) => Player?.FireAt(target);

	internal bool ResolveProjectileHit(MobaProjectile projectile, Vector2 from, Vector2 to)
	{
		if (TestEnemy == null || !GodotObject.IsInstanceValid(TestEnemy) || !SegmentHitsCircle(from, to, TestEnemy.GlobalPosition, TestEnemy.HitRadius, out _)) return false;
		if (!TestEnemy.TryFindFirstSolidImpact(from, to, out DamageableUnitPart part, out Vector2 impact)) return false;
		ResolvedHitboxCollisions++;
		DamageApplicationResult result = mAccess.damageManager.TryApplyDamage(mAccess.gameSessionManager.Current, part.MaterialSprite, new DamageRequest
		{
			Cause = DamageCause.Projectile,
			Damage = 1,
			MaterialDamage = 1,
			WorldImpact = impact,
			ImpactDirection = projectile.Direction,
			StructuralImpulse = 0
		});
		if (result.HitMaterial) AppliedSpriteDamage++;
		projectile.QueueFree();
		return true;
	}

	static bool SegmentHitsCircle(Vector2 from, Vector2 to, Vector2 center, float radius, out Vector2 impact)
	{
		Vector2 segment = to - from;
		float lengthSquared = segment.LengthSquared();
		float progress = lengthSquared <= 0 ? 0 : Mathf.Clamp((center - from).Dot(segment) / lengthSquared, 0, 1);
		impact = from + segment * progress;
		return impact.DistanceSquaredTo(center) <= radius * radius;
	}
}

public partial class MobaPlayer : Node2D
{
	public float MoveSpeed { get; set; } = 240f;
	public Vector2? MoveTarget { get; private set; }
	public void MoveTo(Vector2 target) => MoveTarget = target;
	public override void _Process(double delta)
	{
		if (MoveTarget is not Vector2 target) return;
		Position = Position.MoveToward(target, MoveSpeed * (float)delta);
		if (Position.DistanceTo(target) <= 1f) MoveTarget = null;
	}
	public MobaProjectile FireAt(Vector2 target)
	{
		if (GetParent() is not MobaProjectileArena arena) return null;
		Vector2 direction = (target - GlobalPosition).Normalized();
		if (direction == Vector2.Zero) return null;
		MobaProjectile projectile = new MobaProjectile { Position = GlobalPosition, Direction = direction };
		arena.AddChild(projectile);
		return projectile;
	}
	public override void _Draw()
	{
		DrawCircle(Vector2.Zero, 26, new Color("3d8cff"));
		DrawArc(Vector2.Zero, 26, 0, Mathf.Tau, 32, Colors.White, 2);
	}
}

public partial class MobaTestEnemy : Area2D
{
	public float HitRadius { get; } = 118f;
	public DamageableUnitAssembly Assembly { get; private set; }
	public DamageableUnitPart BodyPart { get; private set; }
	public DamageableUnitPart ArmPart { get; private set; }
	public StructuralJoint ArmJoint { get; private set; }
	public DamageableSprite DamageableSprite => BodyPart?.MaterialSprite;
	public DestructibleAssemblyDefinition Definition { get; set; }
	public override void _Ready()
	{
		CollisionShape2D hitbox = new CollisionShape2D { Name = "Hitbox", Shape = new CircleShape2D { Radius = HitRadius } };
		AddChild(hitbox);
		DestructibleAssemblyDefinition definition = Definition ?? throw new InvalidOperationException("MOBA test enemy definition was not loaded from game data.");
		Assembly = new DamageableUnitAssembly { Name = "EnemyAssembly", MaxBodyHealth = definition.MaxBodyHealth };
		AddChild(Assembly);
		Dictionary<string, DamageableUnitPart> parts = new();
		foreach (DestructiblePartDefinition partDefinition in definition.Parts)
		{
			DamageableUnitPart part = new DamageableUnitPart
			{
				Name = partDefinition.Id + "Part", PartId = partDefinition.Id, Position = new Vector2(partDefinition.PositionX, partDefinition.PositionY),
				MaxHitPoints = partDefinition.MaxHitPoints, BodyDamageMultiplier = partDefinition.BodyDamageMultiplier,
				WeakSpotLocalRect = new Rect2(partDefinition.WeakSpotX, partDefinition.WeakSpotY, partDefinition.WeakSpotWidth, partDefinition.WeakSpotHeight), WeakSpotMultiplier = partDefinition.WeakSpotMultiplier
			};
			part.Effects.AddRange(partDefinition.Effects ?? new List<PartEffectDefinition>());
			part.ConfigureSprite(new DamageableSprite { Name = partDefinition.Id + "Sprite", GridSize = new Vector2I(partDefinition.GridWidth, partDefinition.GridHeight), PixelSize = partDefinition.PixelSize, CellHitPoints = partDefinition.CellHitPoints, MaterialColor = new Color(partDefinition.MaterialColor) });
			Assembly.AddChild(part);
			parts[partDefinition.Id] = part;
		}
		BodyPart = parts["body"];
		ArmPart = parts["arm"];
		StructuralJointDefinition armDefinition = definition.Joints.Find(joint => joint.ChildPartId == "arm") ?? throw new InvalidOperationException("MOBA arm joint definition is missing.");
		ArmJoint = new StructuralJoint { Name = "ArmJoint", ChildLoad = armDefinition.ChildLoad, BreakImpulse = armDefinition.BreakImpulse };
		ArmJoint.Configure(parts[armDefinition.ParentPartId], parts[armDefinition.ChildPartId], armDefinition.ParentAnchors.ConvertAll(anchor => new Vector2I(anchor.X, anchor.Y)));
		Assembly.AddChild(ArmJoint);
	}

	public bool TryFindFirstSolidImpact(Vector2 from, Vector2 to, out DamageableUnitPart hitPart, out Vector2 impact)
	{
		hitPart = null;
		impact = Vector2.Zero;
		float nearestDistance = float.MaxValue;
		foreach (DamageableUnitPart part in Assembly?.Parts ?? Array.Empty<DamageableUnitPart>())
		{
			if (part?.MaterialSprite == null || !GodotObject.IsInstanceValid(part.MaterialSprite)) continue;
			if (!part.MaterialSprite.TryFindFirstSolidImpact(from, to, out Vector2 candidate)) continue;
			float distance = candidate.DistanceSquaredTo(from);
			if (distance >= nearestDistance) continue;
			nearestDistance = distance;
			hitPart = part;
			impact = candidate;
		}
		return hitPart != null;
	}
}

// This is a real Sprite2D texture. A false cell is rendered transparent, while
// the grid remains the authoritative per-pixel health/status representation.
public partial class DamageableSprite : Sprite2D
{
	int[,] health;
	public Vector2I GridSize { get; set; } = new Vector2I(12, 12);
	public int PixelSize { get; set; } = 8;
	public int CellHitPoints { get; set; } = 1;
	public Color MaterialColor { get; set; } = new Color("df4a4a");
	public int DamagedPixelCount { get; private set; }
	public Vector2I LastDamagedCell { get; private set; } = new Vector2I(-1, -1);
	public int RemainingPixelCount => GridSize.X * GridSize.Y - DamagedPixelCount;
	public bool IsRegistered => mAccess.damageManager != null;

	public override void _Ready()
	{
		health = new int[GridSize.X, GridSize.Y];
		for (int x = 0; x < GridSize.X; x++) for (int y = 0; y < GridSize.Y; y++) health[x, y] = Mathf.Max(1, CellHitPoints);
		mAccess.damageManager.Register(this);
		RefreshTexture();
	}
	public override void _ExitTree() => mAccess.damageManager?.Unregister(this);

	public bool ApplyImpact(Vector2 worldImpact, int damage)
	{
		return ApplyImpactDetailed(worldImpact, damage).HitMaterial;
	}
	public SpriteDamageResult ApplyImpactDetailed(Vector2 worldImpact, int damage)
	{
		if (health == null || damage <= 0) return SpriteDamageResult.None;
		Vector2 local = ToLocal(worldImpact) + new Vector2(GridSize.X * PixelSize, GridSize.Y * PixelSize) * 0.5f;
		int x = Mathf.Clamp(Mathf.FloorToInt(local.X / PixelSize), 0, GridSize.X - 1);
		int y = Mathf.Clamp(Mathf.FloorToInt(local.Y / PixelSize), 0, GridSize.Y - 1);
		int before = health[x, y];
		if (before <= 0) return SpriteDamageResult.None;
		health[x, y] = Mathf.Max(0, before - damage);
		bool destroyed = before > 0 && health[x, y] == 0;
		if (destroyed)
		{
			DamagedPixelCount++;
			LastDamagedCell = new Vector2I(x, y);
			RefreshTexture();
		}
		return new SpriteDamageResult(true, destroyed, new Vector2I(x, y), before, health[x, y]);
	}
	public int GetCellHealth(Vector2I cell) => IsCellWithinBounds(cell) ? health?[cell.X, cell.Y] ?? 0 : 0;
	public bool IsCellSolid(Vector2I cell) => GetCellHealth(cell) > 0;
	bool IsCellWithinBounds(Vector2I cell) => cell.X >= 0 && cell.X < GridSize.X && cell.Y >= 0 && cell.Y < GridSize.Y;
	public Vector2 GetCellCenterWorld(int x, int y)
	{
		return ToGlobal(new Vector2((x + 0.5f) * PixelSize - GridSize.X * PixelSize * 0.5f, (y + 0.5f) * PixelSize - GridSize.Y * PixelSize * 0.5f));
	}
	public bool TryFindFirstSolidImpact(Vector2 worldFrom, Vector2 worldTo, out Vector2 worldImpact)
	{
		worldImpact = Vector2.Zero;
		if (health == null) return false;
		Vector2 from = ToLocal(worldFrom);
		Vector2 to = ToLocal(worldTo);
		float smallestProgress = float.MaxValue;
		for (int x = 0; x < GridSize.X; x++) for (int y = 0; y < GridSize.Y; y++)
		{
			if (health[x, y] <= 0) continue;
			Vector2 start = new Vector2(x * PixelSize - GridSize.X * PixelSize * 0.5f, y * PixelSize - GridSize.Y * PixelSize * 0.5f);
			if (SegmentIntersectsRect(from, to, new Rect2(start, Vector2.One * PixelSize), out float progress) && progress < smallestProgress)
			{
				smallestProgress = progress;
			}
		}
		if (smallestProgress == float.MaxValue) return false;
		worldImpact = ToGlobal(from.Lerp(to, smallestProgress));
		return true;
	}
	static bool SegmentIntersectsRect(Vector2 from, Vector2 to, Rect2 rectangle, out float progress)
	{
		Vector2 travel = to - from;
		float minimum = 0f;
		float maximum = 1f;
		for (int axis = 0; axis < 2; axis++)
		{
			float point = axis == 0 ? from.X : from.Y;
			float delta = axis == 0 ? travel.X : travel.Y;
			float low = axis == 0 ? rectangle.Position.X : rectangle.Position.Y;
			float high = low + (axis == 0 ? rectangle.Size.X : rectangle.Size.Y);
			if (Mathf.IsZeroApprox(delta))
			{
				if (point < low || point > high) { progress = 0; return false; }
				continue;
			}
			float first = (low - point) / delta;
			float last = (high - point) / delta;
			if (first > last) (first, last) = (last, first);
			minimum = Mathf.Max(minimum, first);
			maximum = Mathf.Min(maximum, last);
			if (minimum > maximum) { progress = 0; return false; }
		}
		progress = minimum;
		return true;
	}
	void RefreshTexture()
	{
		Image image = Image.Create(GridSize.X * PixelSize, GridSize.Y * PixelSize, false, Image.Format.Rgba8);
		image.Fill(Colors.Transparent);
		for (int x = 0; x < GridSize.X; x++) for (int y = 0; y < GridSize.Y; y++)
		{
			if (health[x, y] <= 0) continue;
			for (int pixelX = 0; pixelX < PixelSize; pixelX++) for (int pixelY = 0; pixelY < PixelSize; pixelY++)
			{
				image.SetPixel(x * PixelSize + pixelX, y * PixelSize + pixelY, MaterialColor);
			}
		}
		Texture = ImageTexture.CreateFromImage(image);
	}
}

public partial class MobaProjectile : Node2D
{
	public Vector2 Direction { get; set; }
	public float Speed { get; set; } = 720f;
	public float RemainingRange { get; set; } = 900f;
	public override void _Process(double delta)
	{
		if (GetParent() is not MobaProjectileArena arena) { QueueFree(); return; }
		Vector2 from = GlobalPosition;
		Vector2 travel = Direction * Speed * (float)delta;
		Vector2 to = from + travel;
		RemainingRange -= travel.Length();
		if (arena.ResolveProjectileHit(this, from, to)) return;
		GlobalPosition = to;
		if (RemainingRange <= 0) QueueFree();
	}
	public override void _Draw() => DrawCircle(Vector2.Zero, 5, new Color("f6f1a4"));
}
