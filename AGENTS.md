# Agent Instructions

## Gameplay testing

- Treat `scripts/gameFramework/TestManagement.cs` as core logic. It owns suite registration, execution, scene loading, frame waits, assertions, and the results UI.
- Keep game-specific scenarios out of the core manager. Add them as `IGameTestSuite` implementations under `scripts/gameplayTests/`, then register them in `TestManagement.setup()` with a stable suite ID.
- Build scenarios from `TestExecutionContext`: use `LoadStoredScene`, `WaitForFrames`, `Check`, and `Require`. A scenario should load its own stored game scene and validate observable game-world results after player-equivalent actions.
- When a test needs a capability that could be useful again, add a small generic helper to `TestExecutionContext` instead of duplicating it in a game suite. For example, use `RequireVisible(Control, description)` for menu/HUD visibility checks; add similar helpers there for other reusable UI, input, or world assertions.
- The main-menu **Run Tests** button runs the `cool-beats-tactical` suite. Keep it separate from the **Test** button, which opens the sandbox.
- Tests must not persist gameplay changes. The test manager restores the main menu and presents pass/fail results when a suite finishes.
