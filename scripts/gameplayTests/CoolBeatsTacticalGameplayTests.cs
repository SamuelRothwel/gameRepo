using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

// Cool Beats-specific scene setup and player-equivalent action scenarios.
public sealed class CoolBeatsTacticalGameplayTests : IGameTestSuite
{
	public string Id => "cool-beats-tactical";
	public string Name => "Cool Beats Tactical Gameplay";
	public IReadOnlyCollection<string> Tags => new[] { "tactical", "input", "selection", "units", "lifecycle", "scene", "animation" };
	public IReadOnlyList<IGameTestScenario> Scenarios { get; }

	public CoolBeatsTacticalGameplayTests()
	{
		Scenarios = new IGameTestScenario[] { createInSceneControlsScenario(), createReopenSelectionScenario() };
	}

	IGameTestScenario createInSceneControlsScenario()
	{
		return new StoredSceneTestScenario(
			"tactical-in-scene-controls", "Tactical in-scene controls", new[] { "tactical", "input", "selection", "units" },
			EntityFrameworkManagement.DefaultGameId, "tactical", null,
			new SceneTestAction("scene setup", (context, session) =>
			{
				context.Check("Tactical scene loads", () =>
				{
					context.Require(session.Scene.SceneKey == "tactical", "Loaded scene key was not tactical.");
					context.Require(session.HasCapability("unitControl"), "The tactical scene did not enable unit control.");
					context.RequireSessionOwned(mAccess.inputManager.camera, session, "Player camera");
				});
				return Task.CompletedTask;
			}),
			new SceneTestAction("box selection", (context, session) =>
			{
				context.Check("Box selection selects a player unit", () =>
				{
					unitControler player = findPlayerUnit(session);
					context.Require(player != null, "Expected a controllable team-0 unit.");
					float selectionRadius = MathF.Max(player.radius, 20);
					Vector2 padding = new Vector2(selectionRadius, selectionRadius);
					context.Require(mAccess.inputManager.TryBoxSelectPlayerUnits(player.Position - padding, player.Position + padding), "Box selection did not select the player unit.");
				});
				return Task.CompletedTask;
			}),
			new SceneTestAction("selection overlay cleanup", (context, session) =>
			{
				context.Check("Selection overlay clears after box selection ends", () =>
				{
					unitControler player = findPlayerUnit(session);
					context.Require(player != null, "Expected a controllable team-0 unit.");
					float selectionRadius = MathF.Max(player.radius, 20);
					Vector2 padding = new Vector2(selectionRadius, selectionRadius);
					Vector2 start = mAccess.inputManager.worldToWindowCoords(player.Position - padding);
					Vector2 end = mAccess.inputManager.worldToWindowCoords(player.Position + padding);
					context.SendUnhandledInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true, Position = start, GlobalPosition = start });
					context.SendUnhandledInput(new InputEventMouseMotion { Position = end, GlobalPosition = end, Relative = end - start });
					context.Require(mAccess.inputManager.SelectionOverlayVisible, "The selection overlay was not drawn.");
					context.Require(mAccess.inputManager.SelectionOverlayColor.A > 0, "The selection overlay did not have a visible color.");
					context.SendUnhandledInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false, Position = end, GlobalPosition = end });
					context.Require(!mAccess.inputManager.SelectionOverlayVisible, "The selection overlay remained visible after selection ended.");
					context.Require(mAccess.inputManager.SelectionOverlayColor.A == 0, "The selection overlay color remained visible after mouse release.");
				});
				return Task.CompletedTask;
			}),
			new SceneTestAction("move command", async (context, session) =>
			{
				unitControler player = findPlayerUnit(session);
				context.Check("Selected player receives move command", () =>
				{
					context.Require(player != null, "Expected a controllable team-0 unit.");
					context.Require(mAccess.inputManager.TryIssuePlayerTargetCommand(Key.None, player.Position + new Vector2(80, 0)), "The move command could not be issued.");
					context.Require(player.activeCommand.state == "move", "The player unit did not receive the move command.");
				});
				await context.WaitForFrames(3);
			}));
	}

	IGameTestScenario createReopenSelectionScenario()
	{
		Camera2D firstCamera = null;
		pen firstPen = null;
		unitAbilities firstAbilities = null;
		return new StoredSceneTestScenario(
			"tactical-reopen-selection", "Tactical close and reopen selection", new[] { "tactical", "lifecycle", "scene", "input", "selection", "animation" },
			EntityFrameworkManagement.DefaultGameId, "tactical", (context, session) =>
			{
				firstCamera = mAccess.inputManager.camera;
				firstPen = firstCamera?.GetChildren().OfType<pen>().FirstOrDefault();
				firstAbilities = context.FindDescendant<unitAbilities>(mAccess.uiManager.CurrentUIRoot);
				context.RequireSessionOwned(firstCamera, session, "Initial player camera");
				context.RequireSessionOwned(firstPen, session, "Initial selection pen");
				context.Require(firstAbilities != null, "The initial unit abilities panel was not created.");
				context.Require(mAccess.inputManager.UnitSelectedSubscriberCount == 1, "The initial UI did not register exactly one selection listener.");
				return Task.CompletedTask;
			},
			new SceneTestAction("dynamic animation teardown", (context, session) =>
			{
				context.Check("Dynamic animations retain only live session targets", () =>
				{
					Sprite2D target = new Sprite2D { Name = "SessionAnimationTarget" };
					session.WorldRoot.AddChild(target);
					DynamicAnimator animator = mAccess.animationManager.PlayEnumerableAnimation("CircularSpriteRotate", new[] { target }, speed: 0.1f);
					context.Require(animator != null && !animator.IsComplete, "The session animation did not start.");
				});
				return Task.CompletedTask;
			}),
			new SceneTestAction("return to menu", async (context, session) =>
			{
				await context.ReturnToMainMenu();
				context.Check("Closing tactical scene releases its input objects", () =>
				{
					context.Require(mAccess.gameSessionManager.Current == null, "A game session remained active after returning to the menu.");
					context.Require(!GodotObject.IsInstanceValid(firstCamera), "The old player camera was not released.");
					context.Require(!GodotObject.IsInstanceValid(firstPen), "The old selection pen was not released.");
					context.Require(!GodotObject.IsInstanceValid(firstAbilities), "The old unit abilities panel was not released.");
					context.Require(mAccess.inputManager.UnitSelectedSubscriberCount == 0, "The disposed unit abilities panel remained subscribed to selection events.");
					context.Require(mAccess.animationManager.ActiveDynamicAnimatorCount == 0, "A dynamic animation retained a disposed session target.");
				});
			}),
			new SceneTestAction("reopen and select", async (context, closedSession) =>
			{
				await context.LoadStoredScene(EntityFrameworkManagement.DefaultGameId, "tactical");
				GameSession reopened = mAccess.gameSessionManager.Current;
				context.Check("Box selection works after reopening tactical scene", () =>
				{
					context.Require(reopened != null && reopened != closedSession, "The tactical scene did not create a fresh session.");
					context.RequireSessionOwned(mAccess.inputManager.camera, reopened, "Reopened player camera");
					pen reopenedPen = mAccess.inputManager.camera?.GetChildren().OfType<pen>().FirstOrDefault();
					context.RequireSessionOwned(reopenedPen, reopened, "Reopened selection pen");
					unitAbilities reopenedAbilities = context.FindDescendant<unitAbilities>(mAccess.uiManager.CurrentUIRoot);
					context.Require(reopenedAbilities != null, "The reopened unit abilities panel was not created.");
					context.Require(mAccess.inputManager.UnitSelectedSubscriberCount == 1, "The reopened UI did not register exactly one selection listener.");
					unitControler player = findPlayerUnit(reopened);
					context.Require(player != null, "The reopened scene has no controllable player unit.");
					float selectionRadius = MathF.Max(player.radius, 20);
					Vector2 padding = new Vector2(selectionRadius, selectionRadius);
					Vector2 start = mAccess.inputManager.worldToWindowCoords(player.Position - padding);
					Vector2 end = mAccess.inputManager.worldToWindowCoords(player.Position + padding);
					context.SendBoxSelectionThroughInput(start, end);
					context.Require(reopenedAbilities.GetChildCount() > 0, "The reopened unit abilities panel did not receive the mouse-selection event.");
					context.Require(mAccess.inputManager.TryIssuePlayerTargetCommand(Key.None, player.Position + new Vector2(80, 0)), "A move command could not be issued after reopening.");
					context.Require(player.activeCommand.state == "move", "The reopened player unit did not receive the move command.");
				});
			}));
	}

	static unitControler findPlayerUnit(GameSession session)
	{
		return session?.UnitIds
			.Select(id => mAccess.unitManager.units.TryGetValue(id, out unitControler unit) ? unit : null)
			.FirstOrDefault(unit => unit != null && GodotObject.IsInstanceValid(unit) && mAccess.teamManager.GetTeamIndex(unit.ID) == 0);
	}
}
