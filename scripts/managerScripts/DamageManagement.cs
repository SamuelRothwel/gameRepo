using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DamageManagement : managerNode
{
	readonly HashSet<DamageableSprite> damageableSprites = new();
	public int RegisteredDamageableSpriteCount => damageableSprites.Count(sprite => GodotObject.IsInstanceValid(sprite));

	public void Register(DamageableSprite sprite)
	{
		if (sprite != null) damageableSprites.Add(sprite);
	}

	public void Unregister(DamageableSprite sprite)
	{
		if (sprite != null) damageableSprites.Remove(sprite);
	}

	// This follows a confirmed hitbox collision; it does not replace normal
	// hitbox, projectile, or health handling.
	public bool TryApplyProjectileImpact(GameSession session, DamageableSprite sprite, Vector2 worldImpact, int damage = 1)
	{
		return TryApplyDamage(session, sprite, new DamageRequest
		{
			Cause = DamageCause.Projectile,
			Damage = damage,
			MaterialDamage = damage,
			WorldImpact = worldImpact
		}).HitMaterial;
	}

	public DamageApplicationResult TryApplyDamage(GameSession session, DamageableSprite sprite, DamageRequest request)
	{
		if (session == null || session != mAccess.gameSessionManager?.Current || !session.HasRule("damageableSprites")) return DamageApplicationResult.None;
		if (sprite == null || !GodotObject.IsInstanceValid(sprite) || !damageableSprites.Contains(sprite) || request == null) return DamageApplicationResult.None;
		SpriteDamageResult materialResult = sprite.ApplyImpactDetailed(request.WorldImpact, request.MaterialDamage);
		if (!materialResult.HitMaterial) return DamageApplicationResult.None;
		DamageableUnitPart part = FindOwningPart(sprite);
		float partDamage = 0;
		if (part != null && session.HasRule("destructibleParts")) partDamage = part.ApplyMaterialDamage(request);
		return new DamageApplicationResult(materialResult, part, partDamage);
	}

	// Used by scripted removal, explosions, and future non-sprite combat. It
	// intentionally does not alter the part's material image.
	public bool TryApplyDirectPartDamage(GameSession session, DamageableUnitPart part, DamageRequest request)
	{
		if (session == null || session != mAccess.gameSessionManager?.Current || !session.HasRule("destructibleParts") || part == null || !GodotObject.IsInstanceValid(part) || request == null) return false;
		part.ApplyDirectDamage(request);
		return true;
	}

	public bool TryRemovePart(GameSession session, DamageableUnitPart part)
	{
		if (session == null || session != mAccess.gameSessionManager?.Current || !session.HasRule("destructibleParts") || part == null || !GodotObject.IsInstanceValid(part)) return false;
		part.Remove(new DamageRequest { Cause = DamageCause.ScriptedRemoval });
		return true;
	}

	static DamageableUnitPart FindOwningPart(Node node)
	{
		for (Node current = node?.GetParent(); current != null; current = current.GetParent())
		{
			if (current is DamageableUnitPart part) return part;
		}
		return null;
	}

	public void sessionStopped(GameSession session)
	{
		damageableSprites.RemoveWhere(sprite => sprite == null || !GodotObject.IsInstanceValid(sprite) || session.WorldRoot.IsAncestorOf(sprite));
	}
	// colour health resistance type
	/*
	Dictionary<int, (int, int)>  colorValues;
    Dictionary<string, Dictionary<string, (int, int)[][,]>> healthMaps;
    public override void setup()
	{
		colorValues = new Dictionary<int, (int, int)>();
		for (int i = 0; i < 10; i++)
		{
			colorValues[i] = (1, 1);
		}
		healthMaps = mAccess.spriteManager.spriteMaps
		.ToDictionary(x => x.Key, x => x.Value
		.ToDictionary(y => y.Key, y => y.Value
		.Select(z => z.Select(u => colorValues[u])).ToArray()));
	}
	public (int, int)[,] GetHealthMap(string name)
	{
		return healthMaps[name].Values.First().First();
	}
	*/
}
