using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;

// Core test logic. Suites contain tagged scenarios; this manager owns execution, restoration, and results UI.
public partial class TestManagement : managerNode
{
	readonly Dictionary<string, IGameTestSuite> suites = new(StringComparer.OrdinalIgnoreCase);
	readonly TestImpactMap impactMap = new();
	readonly RuntimeDiagnosticMonitor diagnosticMonitor = new();
	bool headlessRunPending;
	string requestedHeadlessSuiteId = "";
	string headlessStartupFailure = "";
	public bool IsRunning { get; private set; }

	public override void setup()
	{
		diagnosticMonitor.Start();
		RegisterSuite(new CoolBeatsTacticalGameplayTests());
		RegisterSuite(new MobaProjectileArenaGameplayTests());
		impactMap.Register("scripts/managerscripts/inputmanagement.cs", "input", "selection");
		impactMap.Register("scripts/managerscripts/teammanagement.cs", "selection", "units");
		impactMap.Register("scripts/managerscripts/unitmanagement.cs", "units");
		impactMap.Register("scripts/gameframework/gamesessionmanagement.cs", "lifecycle", "scene");
		impactMap.Register("scripts/managerscripts/scenemanagment.cs", "lifecycle", "scene");
		impactMap.Register("scripts/managerscripts/uimanagement.cs", "lifecycle", "scene", "selection");
		impactMap.Register("scenes/uiscenes/unitabilities.cs", "lifecycle", "selection");
		impactMap.Register("scripts/logicscripts/backgroundlogic", "lifecycle");
		impactMap.Register("scripts/managerscripts/animationmanagement.cs", "animation", "lifecycle");
		impactMap.Register("scripts/staticscriptsanddatastructures/dynamicanimationpropertyaccessors.cs", "animation", "lifecycle");
		impactMap.Register("scripts/logicscripts/attachedlogic/mobaprojectilearena.cs", "moba", "projectile", "damageable-sprite");
		impactMap.Register("scripts/managerscripts/damagemanagement.cs", "moba", "projectile", "damageable-sprite");
		impactMap.Register("scenes/gamescenes/moba_scene.tscn", "moba", "projectile", "damageable-sprite");
		impactMap.Register("scripts/managerscripts/entityframeworkmanagement.cs", "moba", "scene", "lifecycle");
		queueHeadlessRunFromCommandLine();
	}

	public override void _Process(double delta)
	{
		if (!headlessRunPending) return;
		headlessRunPending = false;
		if (!string.IsNullOrEmpty(headlessStartupFailure))
		{
			GD.Print("GAMEPLAY_TEST_REPORT " + JsonSerializer.Serialize(HeadlessTestReport.Failure("Command line", headlessStartupFailure)));
			GetTree().Quit(2);
			return;
		}
		if (!suites.TryGetValue(requestedHeadlessSuiteId, out IGameTestSuite suite))
		{
			GD.Print("GAMEPLAY_TEST_REPORT " + JsonSerializer.Serialize(HeadlessTestReport.Failure("Command line", "Gameplay test suite is not registered: " + requestedHeadlessSuiteId)));
			GetTree().Quit(2);
			return;
		}
		RunScenariosAsync(suite.Name, suite.Scenarios, false, true);
	}

	public void RegisterSuite(IGameTestSuite suite) => suites[suite.Id] = suite;
	public void RunSuite(string suiteId)
	{
		if (!suites.TryGetValue(suiteId, out IGameTestSuite suite))
		{
			GD.PushError("Gameplay test suite is not registered: " + suiteId);
			return;
		}
		RunScenarios(suite.Name, suite.Scenarios);
	}

	public void RunScenariosWithTags(params string[] tags)
	{
		HashSet<string> requestedTags = new(tags ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
		RunScenarios("Tagged Gameplay Tests", suites.Values.SelectMany(suite => suite.Scenarios)
			.Where(scenario => requestedTags.Count == 0 || scenario.Tags.Any(requestedTags.Contains)));
	}

	// For CI or a development command that supplies changed source paths. Unmapped paths run all tests safely.
	public void RunAffectedForPaths(IEnumerable<string> changedPaths)
	{
		IReadOnlyCollection<string> tags = impactMap.GetTags(changedPaths);
		if (tags.Count == 0) RunScenarios("Gameplay Tests (unmapped change)", suites.Values.SelectMany(suite => suite.Scenarios));
		else RunScenariosWithTags(tags.ToArray());
	}

	void RunScenarios(string runName, IEnumerable<IGameTestScenario> scenarios)
	{
		if (IsRunning) return;
		List<IGameTestScenario> selected = scenarios.ToList();
		if (selected.Count == 0) { GD.PushError("No gameplay test scenarios matched the requested selection."); return; }
		RunScenariosAsync(runName, selected, true, false);
	}

	async void RunScenariosAsync(string runName, IReadOnlyList<IGameTestScenario> scenarios, bool showResults, bool quitWhenFinished)
	{
		IsRunning = true;
		TestExecutionContext context = new(this, diagnosticMonitor);
		try
		{
			foreach (IGameTestScenario scenario in scenarios) await context.RunScenario(scenario);
		}
		finally
		{
			await context.ReturnToMainMenu();
			if (showResults) ShowResults(runName, context.Results);
			else WriteHeadlessResults(runName, context.Results);
			IsRunning = false;
			if (quitWhenFinished) GetTree().Quit(context.Results.All(result => result.Passed) ? 0 : 1);
		}
	}

	void queueHeadlessRunFromCommandLine()
	{
		// Godot exposes user arguments after `--` through GetCmdlineUserArgs.
		// Some embedded/console launches retain them only in GetCmdlineArgs, so
		// inspect both forms before deciding whether to start the normal game loop.
		string[] arguments = OS.GetCmdlineUserArgs().Concat(OS.GetCmdlineArgs()).ToArray();
		int index = Array.FindIndex(arguments, argument => string.Equals(argument, "--run-gameplay-tests", StringComparison.OrdinalIgnoreCase));
		if (index < 0) return;
		headlessRunPending = true;
		if (index + 1 >= arguments.Length)
		{
			headlessStartupFailure = "--run-gameplay-tests requires a suite ID.";
			return;
		}
		requestedHeadlessSuiteId = arguments[index + 1];
	}

	void WriteHeadlessResults(string suiteName, IReadOnlyList<TestResult> results)
	{
		HeadlessTestReport report = new()
		{
			Suite = suiteName,
			Passed = results.Count > 0 && results.All(result => result.Passed),
			Results = results.Select(result => new HeadlessTestResult { Name = result.Name, Passed = result.Passed, Message = result.Message }).ToList()
		};
		GD.Print("GAMEPLAY_TEST_REPORT " + JsonSerializer.Serialize(report));
	}

	void ShowResults(string suiteName, IEnumerable<TestResult> results)
	{
		VBoxContainer content = new VBoxContainer { CustomMinimumSize = new Vector2(460, 160) };
		content.AddChild(new Label { Text = suiteName + " — Test Results" });
		foreach (TestResult result in results)
		{
			Label row = new Label { Text = (result.Passed ? "PASS  " : "FAIL  ") + result.Name + "\n" + result.Message };
			row.Modulate = result.Passed ? new Color(0.55f, 0.9f, 0.6f) : new Color(1f, 0.55f, 0.55f);
			content.AddChild(row);
		}
		mAccess.windowManager.openWindow("Gameplay Test Results", content, "closeButtonTransparentTopbar", false);
	}
}

public interface IGameTestSuite
{
	string Id { get; }
	string Name { get; }
	IReadOnlyCollection<string> Tags { get; }
	IReadOnlyList<IGameTestScenario> Scenarios { get; }
}

public interface IGameTestScenario
{
	string Id { get; }
	string Name { get; }
	IReadOnlyCollection<string> Tags { get; }
	Task Run(TestExecutionContext context);
}

// Reusable stored-scene setup plus an ordered list of in-scene actions.
public sealed class StoredSceneTestScenario : IGameTestScenario
{
	readonly Guid gameId;
	readonly string sceneKey;
	readonly Func<TestExecutionContext, GameSession, Task> setup;
	readonly IReadOnlyList<SceneTestAction> actions;
	public StoredSceneTestScenario(string id, string name, IEnumerable<string> tags, Guid gameId, string sceneKey,
		Func<TestExecutionContext, GameSession, Task> setup, params SceneTestAction[] actions)
	{
		Id = id; Name = name; Tags = tags.ToArray(); this.gameId = gameId; this.sceneKey = sceneKey;
		this.setup = setup; this.actions = actions;
	}
	public string Id { get; }
	public string Name { get; }
	public IReadOnlyCollection<string> Tags { get; }
	public async Task Run(TestExecutionContext context)
	{
		await context.LoadStoredScene(gameId, sceneKey);
		GameSession session = mAccess.gameSessionManager.Current;
		context.Require(session != null, "Scene setup did not create a game session.");
		if (setup != null) await setup(context, session);
		foreach (SceneTestAction action in actions)
		{
			try { await action.Run(context, session); }
			catch (Exception exception) { context.AddFailure(action.Name, exception.Message); }
		}
	}
}

public sealed class SceneTestAction
{
	readonly Func<TestExecutionContext, GameSession, Task> action;
	public SceneTestAction(string name, Func<TestExecutionContext, GameSession, Task> action) { Name = name; this.action = action; }
	public string Name { get; }
	public Task Run(TestExecutionContext context, GameSession session) => action(context, session);
}

public sealed class TestExecutionContext
{
	readonly TestManagement manager;
	readonly RuntimeDiagnosticMonitor diagnosticMonitor;
	readonly List<TestResult> results = new();
	string scenarioName = "";
	public IReadOnlyList<TestResult> Results => results;
	public TestExecutionContext(TestManagement manager, RuntimeDiagnosticMonitor diagnosticMonitor)
	{
		this.manager = manager;
		this.diagnosticMonitor = diagnosticMonitor;
	}
	public async Task RunScenario(IGameTestScenario scenario)
	{
		int diagnosticsAtStart = diagnosticMonitor.Count;
		scenarioName = scenario.Name;
		try { await scenario.Run(this); }
		catch (Exception exception) { AddFailure("Scenario", exception.Message); GD.PushError("Gameplay scenario failed: " + exception); }
		finally
		{
			foreach (RuntimeDiagnostic diagnostic in diagnosticMonitor.GetSince(diagnosticsAtStart))
			{
				AddFailure("Runtime error", diagnostic.Message);
			}
			scenarioName = "";
		}
	}
	public async Task LoadStoredScene(Guid gameId, string sceneKey, int readyFrames = 2)
	{
		mAccess.sceneManager.startStoredGameScene(gameId, sceneKey);
		await WaitForFrames(readyFrames);
	}
	public async Task ReturnToMainMenu(int readyFrames = 2)
	{
		if (mAccess.gameSessionManager.Current == null) return;
		mAccess.sceneManager.startMenuForTests();
		await WaitForFrames(readyFrames);
	}
	public async Task WaitForFrames(int frameCount)
	{
		for (int i = 0; i < frameCount; i++) await manager.ToSignal(manager.GetTree(), SceneTree.SignalName.ProcessFrame);
	}
	public async Task<bool> WaitUntil(Func<bool> condition, int maximumFrames = 120)
	{
		for (int frame = 0; frame < maximumFrames; frame++)
		{
			if (condition()) return true;
			await WaitForFrames(1);
		}
		return condition();
	}
	public void SendUnhandledInput(InputEvent inputEvent)
	{
		mAccess.inputManager._UnhandledInput(inputEvent);
	}
	public void SendBoxSelectionThroughInput(Vector2 startWindowPosition, Vector2 endWindowPosition)
	{
		SendUnhandledInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true, Position = startWindowPosition, GlobalPosition = startWindowPosition });
		SendUnhandledInput(new InputEventMouseMotion { Position = endWindowPosition, GlobalPosition = endWindowPosition, Relative = endWindowPosition - startWindowPosition });
		SendUnhandledInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false, Position = endWindowPosition, GlobalPosition = endWindowPosition });
	}
	public T FindDescendant<T>(Node root) where T : Node
	{
		if (root == null || !GodotObject.IsInstanceValid(root)) return null;
		if (root is T match) return match;
		foreach (Node child in root.GetChildren())
		{
			T descendant = FindDescendant<T>(child);
			if (descendant != null) return descendant;
		}
		return null;
	}
	public void Check(string name, Action test)
	{
		try { test(); results.Add(TestResult.Pass(Decorate(name))); }
		catch (Exception exception) { results.Add(TestResult.Fail(Decorate(name), exception.Message)); }
	}
	public void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
	public void RequireVisible(Control control, string description) => Require(control != null && GodotObject.IsInstanceValid(control) && control.IsVisibleInTree(), description + " is not visible.");
	public void RequireDamageableSpriteChanged(DamageableSprite sprite, int previousDamageCount, string description)
	{
		Require(sprite != null && GodotObject.IsInstanceValid(sprite), description + " does not exist.");
		Require(sprite.DamagedPixelCount > previousDamageCount, description + " did not lose any tracked sprite pixels.");
	}
	public void RequireSessionOwned(Node node, GameSession session, string description)
	{
		Require(node != null && GodotObject.IsInstanceValid(node), description + " does not exist.");
		Require(session != null && GodotObject.IsInstanceValid(session.WorldRoot) && (node == session.WorldRoot || session.WorldRoot.IsAncestorOf(node)), description + " is not owned by the active game session.");
	}
	public void AddFailure(string name, string message) => results.Add(TestResult.Fail(Decorate(name), message));
	string Decorate(string name) => string.IsNullOrEmpty(scenarioName) ? name : scenarioName + ": " + name;
}

// Godot's C# bridge catches many runtime exceptions and writes them to the
// debugger instead of letting them fail the process.  Record the relevant
// first-chance exception so gameplay scenarios fail on those log errors.
public sealed class RuntimeDiagnosticMonitor
{
	readonly List<RuntimeDiagnostic> diagnostics = new();
	bool started;
	public int Count => diagnostics.Count;
	public void Start()
	{
		if (started) return;
		started = true;
		AppDomain.CurrentDomain.FirstChanceException += onFirstChanceException;
	}
	void onFirstChanceException(object sender, FirstChanceExceptionEventArgs args)
	{
		if (args.Exception is ObjectDisposedException exception)
		{
			diagnostics.Add(new RuntimeDiagnostic("ObjectDisposedException: " + exception.ObjectName));
		}
	}
	public IReadOnlyList<RuntimeDiagnostic> GetSince(int index) => diagnostics.Skip(index).ToList();
}

public sealed class RuntimeDiagnostic
{
	public RuntimeDiagnostic(string message) => Message = message;
	public string Message { get; }
}

public sealed class TestImpactMap
{
	readonly List<(string prefix, string[] tags)> rules = new();
	public void Register(string pathPrefix, params string[] tags) => rules.Add((Normalise(pathPrefix), tags));
	public IReadOnlyCollection<string> GetTags(IEnumerable<string> changedPaths)
	{
		HashSet<string> result = new(StringComparer.OrdinalIgnoreCase);
		foreach (string path in changedPaths ?? Array.Empty<string>())
		{
			string normalised = Normalise(path);
			foreach ((string prefix, string[] tags) in rules.Where(rule => normalised.StartsWith(rule.prefix, StringComparison.OrdinalIgnoreCase))) result.UnionWith(tags);
		}
		return result;
	}
	static string Normalise(string path) => (path ?? "").Replace('\\', '/').TrimStart('/').ToLowerInvariant();
}

public sealed class TestResult
{
	public string Name { get; private set; }
	public string Message { get; private set; }
	public bool Passed { get; private set; }
	TestResult(string name, bool passed, string message) { Name = name; Passed = passed; Message = message; }
	public static TestResult Pass(string name) => new(name, true, "Completed.");
	public static TestResult Fail(string name, string message) => new(name, false, message);
}

// Stable machine-readable output for the headless runner. Keep this separate
// from the presentation UI so a CI/tool invocation has no window dependency.
public sealed class HeadlessTestReport
{
	public string Suite { get; set; } = "";
	public bool Passed { get; set; }
	public List<HeadlessTestResult> Results { get; set; } = new();
	public static HeadlessTestReport Failure(string name, string message) => new()
	{
		Suite = name,
		Passed = false,
		Results = new List<HeadlessTestResult> { new HeadlessTestResult { Name = name, Passed = false, Message = message } }
	};
}

public sealed class HeadlessTestResult
{
	public string Name { get; set; } = "";
	public bool Passed { get; set; }
	public string Message { get; set; } = "";
}
