using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

// End-to-end MOBA scenario. Collision and sprite damage are asserted separately
// so a bad hitbox cannot be mistaken for failed visual-damage logic.
public sealed class MobaProjectileArenaGameplayTests : IGameTestSuite
{
	public string Id => "moba-projectile-arena";
	public string Name => "MOBA Projectile Arena Gameplay";
	public IReadOnlyCollection<string> Tags => new[] { "moba", "projectile", "damageable-sprite", "destructible-parts", "structural-joint", "scene", "lifecycle" };
	public IReadOnlyList<IGameTestScenario> Scenarios { get; }

	public MobaProjectileArenaGameplayTests()
	{
		Scenarios = new IGameTestScenario[]
		{
			createProjectileDamageScenario(),
			createPartDamageScenario(),
			createStructuralTearScenario(),
			createPartRemovalScenario()
		};
	}

	IGameTestScenario createProjectileDamageScenario()
	{
		return new StoredSceneTestScenario(
			"moba-projectile-damages-sprite", "MOBA projectile damages test enemy sprite", new[] { "moba", "projectile", "damageable-sprite" },
			EntityFrameworkManagement.MobaGameId, "moba", null,
			new SceneTestAction("scene setup", (context, session) =>
			{
				context.Check("MOBA scene enables destructible sprites", () =>
				{
					IReadOnlyList<StoredGame> gameTypes = mAccess.entityFrameworkManager.GetStartGameTypes();
					MobaProjectileArena arena = context.FindDescendant<MobaProjectileArena>(session.WorldRoot);
					context.Require(gameTypes.Count == 2 && gameTypes[0].Id == EntityFrameworkManagement.DefaultGameId && gameTypes[1].Id == EntityFrameworkManagement.MobaGameId, "The Start menu game-type list did not contain exactly RTS and MOBA.");
					context.Require(session.HasCapability("mobaControl"), "The MOBA scene did not enable MOBA controls.");
					context.Require(session.HasRule("damageableSprites"), "The MOBA scene did not opt in to destructible sprite damage.");
					context.Require(session.HasRule("destructibleParts"), "The MOBA scene did not opt in to destructible parts.");
					context.Require(arena?.TestEnemyDefinition?.Parts?.Count == 2 && arena.TestEnemyDefinition.Joints?.Count == 1, "The assembled enemy part and joint definition was not loaded from the stored game data.");
					context.Require(arena?.Player != null && arena.TestEnemy?.DamageableSprite != null && arena.TestEnemy.ArmPart?.MaterialSprite != null, "The MOBA player or assembled test enemy was not created.");
					context.Require(mAccess.damageManager.RegisteredDamageableSpriteCount == 2, "The body and arm sprites were not registered with DamageManagement.");
					context.Require(!mAccess.damageManager.TryApplyProjectileImpact(session, null, Vector2.Zero), "Damage was accepted without a damageable sprite.");
				});
				return Task.CompletedTask;
			}),
			new SceneTestAction("moba right-click movement", async (context, session) =>
			{
				MobaProjectileArena arena = context.FindDescendant<MobaProjectileArena>(session.WorldRoot);
				Vector2 before = arena.Player.Position;
				Vector2 destination = before + new Vector2(100, 0);
				Vector2 windowDestination = destination + new Vector2(0, mAccess.uiManager.getReservedTopbarHeight());
				arena._UnhandledInput(new InputEventMouseButton { ButtonIndex = MouseButton.Right, Pressed = true, Position = windowDestination, GlobalPosition = windowDestination });
				await context.WaitForFrames(8);
				context.Check("MOBA right-click moves the player", () => context.Require(arena.Player.Position.X > before.X, "The MOBA player did not move after a right-click order."));
			}),
			new SceneTestAction("destroy every sprite pixel", async (context, session) =>
			{
				MobaProjectileArena arena = context.FindDescendant<MobaProjectileArena>(session.WorldRoot);
				DamageableSprite sprite = arena.TestEnemy.DamageableSprite;
				for (int row = 0; row < sprite.GridSize.Y; row++)
				{
					for (int shot = 0; shot < sprite.GridSize.X; shot++)
					{
						int collisionsBefore = arena.ResolvedHitboxCollisions;
						int pixelsBefore = sprite.DamagedPixelCount;
						MobaProjectile projectile = arena.FireTestProjectile(sprite.GetCellCenterWorld(sprite.GridSize.X - 1, row));
						context.Require(projectile != null, "The MOBA player could not fire a projectile at sprite row " + row + ".");
						projectile.Speed = 1800f;
						context.Require(await context.WaitUntil(() => !GodotObject.IsInstanceValid(projectile) || projectile.IsQueuedForDeletion()), "Projectile did not finish within 120 frames for row " + row + ".");
						context.Require(arena.ResolvedHitboxCollisions == collisionsBefore + 1, "Projectile did not resolve exactly one opaque-pixel collision for row " + row + ". Collisions before/after: " + collisionsBefore + "/" + arena.ResolvedHitboxCollisions + ".");
						context.RequireDamageableSpriteChanged(sprite, pixelsBefore, "The enemy sprite after shot " + shot + " on row " + row);
						Image visual = ((ImageTexture)sprite.Texture).GetImage();
						Vector2I cell = sprite.LastDamagedCell;
						context.Require(visual.GetPixel(cell.X * sprite.PixelSize, cell.Y * sprite.PixelSize).A == 0, "Destroyed sprite cell " + cell + " was still visible on screen.");
					}
				}
				context.Check("All nine enemy pixels become transparent", () => context.Require(sprite.RemainingPixelCount == 0, "The 3x3 test enemy still has opaque sprite pixels after nine hits."));
			}),
			new SceneTestAction("transparent pixels do not collide", async (context, session) =>
			{
				MobaProjectileArena arena = context.FindDescendant<MobaProjectileArena>(session.WorldRoot);
				int collisionsBefore = arena.ResolvedHitboxCollisions;
				int pixelsBefore = arena.TestEnemy.DamageableSprite.DamagedPixelCount;
				MobaProjectile projectile = arena.FireTestProjectile(arena.TestEnemy.DamageableSprite.GetCellCenterWorld(1, 1));
				projectile.Speed = 1800f;
				context.Require(await context.WaitUntil(() => !GodotObject.IsInstanceValid(projectile) || projectile.IsQueuedForDeletion()), "Projectile through the transparent enemy did not finish within 120 frames.");
				context.Check("Projectile passes through transparent sprite", () =>
				{
					context.Require(arena.ResolvedHitboxCollisions == collisionsBefore, "Projectile collided with the enemy after every sprite pixel was transparent.");
					context.Require(arena.TestEnemy.DamageableSprite.DamagedPixelCount == pixelsBefore, "A transparent sprite pixel received additional damage.");
				});
			}));
	}

	IGameTestScenario createPartDamageScenario()
	{
		return new StoredSceneTestScenario(
			"moba-part-health-and-effects", "MOBA assembled part health, weak spot, and destruction effects", new[] { "moba", "part-health", "weak-spot", "effects" },
			EntityFrameworkManagement.MobaGameId, "moba", null,
			new SceneTestAction("damage associated sprite and weak spot", (context, session) =>
			{
				MobaProjectileArena arena = context.FindDescendant<MobaProjectileArena>(session.WorldRoot);
				DamageableUnitPart body = arena.TestEnemy.BodyPart;
				DamageableSprite sprite = body.MaterialSprite;
				float partHealthBefore = body.HitPoints;
				float bodyHealthBefore = arena.TestEnemy.Assembly.BodyHealth;
				DamageApplicationResult normal = mAccess.damageManager.TryApplyDamage(session, sprite, new DamageRequest
				{
					Cause = DamageCause.Projectile, Damage = 1, MaterialDamage = 1, WorldImpact = sprite.GetCellCenterWorld(0, 1)
				});
				DamageApplicationResult weakSpot = mAccess.damageManager.TryApplyDamage(session, sprite, new DamageRequest
				{
					Cause = DamageCause.WeakSpot, Damage = 1, MaterialDamage = 1, WorldImpact = sprite.GetCellCenterWorld(1, 1)
				});
				context.Check("Part damage follows its sprite and weak spots multiply it", () =>
				{
					context.Require(normal.HitMaterial && normal.Part == body && normal.PartDamage == 1, "A normal opaque body pixel did not route damage to its owning part.");
					context.Require(weakSpot.HitMaterial && weakSpot.PartDamage == 3, "The configured weak spot did not multiply part damage.");
					context.Require(body.HitPoints == partHealthBefore - 4, "Part HP did not accumulate normal and weak-spot damage.");
					context.Require(arena.TestEnemy.Assembly.BodyHealth == bodyHealthBefore - 2, "Part damage did not transfer its configured amount to main body health.");
					Image visual = ((ImageTexture)sprite.Texture).GetImage();
					context.Require(visual.GetPixel(sprite.PixelSize, sprite.PixelSize).A == 0, "Weak-spot sprite material did not become transparent after its cell was destroyed.");
				});
				return Task.CompletedTask;
			}),
			new SceneTestAction("direct part destruction effects", (context, session) =>
			{
				MobaProjectileArena arena = context.FindDescendant<MobaProjectileArena>(session.WorldRoot);
				DamageableUnitAssembly assembly = arena.TestEnemy.Assembly;
				bool applied = mAccess.damageManager.TryApplyDirectPartDamage(session, arena.TestEnemy.ArmPart, new DamageRequest { Cause = DamageCause.Explosion, Damage = 2 });
				context.Check("Destroyed part changes unit abilities armour movement and bleed", () =>
				{
					context.Require(applied && arena.TestEnemy.ArmPart.State == PartState.Destroyed, "Direct part damage did not destroy the arm part.");
					context.Require(!assembly.AbilityEnabled, "Destroyed arm did not disable its configured ability.");
					context.Require(assembly.Armour == 6 && assembly.MovementMultiplier == 0.5f && assembly.BleedEffectCount == 1, "Destroyed arm did not apply its armour, movement, and bleed effects exactly once.");
				});
				return Task.CompletedTask;
			}));
	}

	IGameTestScenario createStructuralTearScenario()
	{
		return new StoredSceneTestScenario(
			"moba-partial-cut-tears-joint", "MOBA partial cut tears a supported sprite part", new[] { "moba", "structural-joint", "tear" },
			EntityFrameworkManagement.MobaGameId, "moba", null,
			new SceneTestAction("partially cut arm attachment", (context, session) =>
			{
				MobaProjectileArena arena = context.FindDescendant<MobaProjectileArena>(session.WorldRoot);
				DamageableSprite bodySprite = arena.TestEnemy.BodyPart.MaterialSprite;
				StructuralJoint joint = arena.TestEnemy.ArmJoint;
				DamageRequest cut(Vector2 impact) => new DamageRequest { Cause = DamageCause.Projectile, Damage = 1, MaterialDamage = 1, WorldImpact = impact, ImpactDirection = Vector2.Right };
				mAccess.damageManager.TryApplyDamage(session, bodySprite, cut(bodySprite.GetCellCenterWorld(2, 0)));
				context.Require(!joint.IsBroken && !arena.TestEnemy.ArmPart.IsDetached, "The arm detached while two of its three attachment cells were still intact.");
				mAccess.damageManager.TryApplyDamage(session, bodySprite, cut(bodySprite.GetCellCenterWorld(2, 1)));
				context.Check("Partial cut tears off an overloaded part", () =>
				{
					context.Require(joint.IsBroken && joint.RemainingCapacity > 0, "The joint did not break while it still had a partial material connection.");
					context.Require(arena.TestEnemy.ArmPart.IsDetached, "The broken joint did not detach its child sprite part.");
					context.Require(arena.TestEnemy.ArmPart.GetParent() == session.WorldRoot, "Detached part was not moved outside the parent assembly.");
				});
				return Task.CompletedTask;
			}));
	}

	IGameTestScenario createPartRemovalScenario()
	{
		return new StoredSceneTestScenario(
			"moba-scripted-part-removal", "MOBA scripted part removal follows destruction cleanup", new[] { "moba", "part-removal", "lifecycle" },
			EntityFrameworkManagement.MobaGameId, "moba", null,
			new SceneTestAction("remove arm part", async (context, session) =>
			{
				MobaProjectileArena arena = context.FindDescendant<MobaProjectileArena>(session.WorldRoot);
				DamageableUnitPart arm = arena.TestEnemy.ArmPart;
				bool removed = mAccess.damageManager.TryRemovePart(session, arm);
				PartState removalState = arm.State;
				await context.WaitForFrames(2);
				context.Check("Scripted part removal unregisters material and applies effects", () =>
				{
					context.Require(removed && removalState == PartState.Removed, "Scripted removal did not mark the arm part for deletion.");
					context.Require(mAccess.damageManager.RegisteredDamageableSpriteCount == 1, "Removed arm material remained registered for damage.");
					context.Require(!arena.TestEnemy.Assembly.AbilityEnabled && arena.TestEnemy.Assembly.BleedEffectCount == 1, "Scripted removal bypassed the part's configured destruction effects.");
				});
			}));
	}
}
