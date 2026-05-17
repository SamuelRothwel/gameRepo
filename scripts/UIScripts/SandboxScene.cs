using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using coolbeats.scripts.staticScriptsAndDataStructures;

public partial class SandboxScene : Node2D
{
	OptionButton entityChoice;
	OptionButton animationChoice;
	Label statusLabel;
	Node2D animationRig;
	CircularEnumerator<Sprite2D> rigSprites;
	readonly List<Node> spawnedNodes = new();
	readonly List<Guid> spawnedUnitIds = new();
	readonly List<DynamicAnimator> activeDynamicAnimators = new();
	int spawnCount;

	public override void _Ready()
	{
		BuildMenu();
		BuildAnimationRig();
		RefreshEntityChoices();
		RefreshAnimationChoices();
	}

	public override void _ExitTree()
	{
		foreach (Guid id in spawnedUnitIds)
		{
			mAccess.unitManager?.remove(id);
		}
		foreach (Node node in spawnedNodes)
		{
			if (GodotObject.IsInstanceValid(node))
			{
				node.QueueFree();
			}
		}
	}

	public override void _Process(double delta)
	{
		for (int i = activeDynamicAnimators.Count - 1; i >= 0; i--)
		{
			DynamicAnimator animator = activeDynamicAnimators[i];
			animator.Process(delta);
			if (animator.IsComplete)
			{
				activeDynamicAnimators.RemoveAt(i);
			}
		}
	}

	void BuildMenu()
	{
		CanvasLayer layer = new CanvasLayer();
		AddChild(layer);

		PanelContainer panel = new PanelContainer();
		panel.OffsetLeft = 16;
		panel.OffsetTop = 16;
		panel.OffsetRight = 390;
		panel.OffsetBottom = 330;
		layer.AddChild(panel);

		VBoxContainer content = new VBoxContainer();
		content.AddThemeConstantOverride("separation", 8);
		panel.AddChild(content);

		Label title = new Label();
		title.Text = "Sandbox";
		content.AddChild(title);

		entityChoice = new OptionButton();
		content.AddChild(entityChoice);

		Button spawnButton = new Button();
		spawnButton.Text = "Create Entity";
		spawnButton.Pressed += SpawnSelectedEntity;
		content.AddChild(spawnButton);

		animationChoice = new OptionButton();
		content.AddChild(animationChoice);

		Button runAnimationButton = new Button();
		runAnimationButton.Text = "Run Animation";
		runAnimationButton.Pressed += RunSelectedAnimation;
		content.AddChild(runAnimationButton);

		Button benchmarkButton = new Button();
		benchmarkButton.Text = "3 Second Animation Test";
		benchmarkButton.Pressed += StartBenchmark;
		content.AddChild(benchmarkButton);

		Button clearButton = new Button();
		clearButton.Text = "Clear Spawned";
		clearButton.Pressed += ClearSpawned;
		content.AddChild(clearButton);

		Button menuButton = new Button();
		menuButton.Text = "Back To Menu";
		menuButton.Pressed += () => mAccess.sceneManager.startMenu();
		content.AddChild(menuButton);

		statusLabel = new Label();
		statusLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		statusLabel.Text = "Choose an entity or animation.";
		content.AddChild(statusLabel);
	}

	void BuildAnimationRig()
	{
		animationRig = new Node2D();
		animationRig.Position = new Vector2(720, 320);
		AddChild(animationRig);

		Texture2D texture = GD.Load<Texture2D>("res://icon.svg");
		Sprite2D[] sprites = new Sprite2D[6];
		for (int i = 0; i < sprites.Length; i++)
		{
			float angle = Mathf.Tau * i / sprites.Length;
			Sprite2D sprite = new Sprite2D();
			sprite.Texture = texture;
			sprite.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 60f;
			sprite.Scale = new Vector2(0.18f, 0.18f);
			animationRig.AddChild(sprite);
			sprites[i] = sprite;
		}
		rigSprites = new CircularEnumerator<Sprite2D>(ref sprites);
	}

	void RefreshEntityChoices()
	{
		entityChoice.Clear();
		foreach (StoredUnit unit in mAccess.entityFrameworkManager?.GetUnits() ?? new List<StoredUnit>())
		{
			int index = entityChoice.ItemCount;
			entityChoice.AddItem(unit.Name);
			entityChoice.SetItemMetadata(index, unit.Name);
		}
		if (entityChoice.ItemCount == 0)
		{
			entityChoice.AddItem("No stored units found");
			entityChoice.Disabled = true;
		}
	}

	void RefreshAnimationChoices()
	{
		animationChoice.Clear();
		foreach (string name in mAccess.animationManager.dynamicAnimationDefinitions.Keys.OrderBy(name => name))
		{
			int index = animationChoice.ItemCount;
			animationChoice.AddItem("Dynamic: " + name);
			animationChoice.SetItemMetadata(index, "dynamic|" + name);
		}
		foreach (KeyValuePair<string, AnimationLibrary> library in mAccess.animationManager.animationSets.OrderBy(pair => pair.Key))
		{
			foreach (StringName animationName in library.Value.GetAnimationList().OrderBy(name => name.ToString()))
			{
				int index = animationChoice.ItemCount;
				animationChoice.AddItem(library.Key + "/" + animationName);
				animationChoice.SetItemMetadata(index, "static|" + library.Key + "/" + animationName);
			}
		}
		if (animationChoice.ItemCount == 0)
		{
			animationChoice.AddItem("No animations found");
			animationChoice.Disabled = true;
		}
	}

	void SpawnSelectedEntity()
	{
		string unitName = GetSelectedMetadata(entityChoice);
		if (string.IsNullOrEmpty(unitName))
		{
			return;
		}
		if (!mAccess.entityManager.packedEntities.ContainsKey(unitName))
		{
			statusLabel.Text = unitName + " is in the database, but no packed entity named " + unitName + " exists.";
			return;
		}
		if (!mAccess.unitManager.unitDefinitions.ContainsKey(unitName))
		{
			statusLabel.Text = unitName + " has a packed entity, but no registered unit definition.";
			return;
		}

		Guid id = mAccess.unitManager.createUnit(unitName, 0);
		unitControler unit = mAccess.unitManager.units[id];
		unit.Position = new Vector2(520 + (spawnCount % 5) * 90, 180 + (spawnCount / 5) * 80);
		spawnCount++;
		spawnedNodes.Add(unit);
		spawnedUnitIds.Add(id);
		statusLabel.Text = "Created " + unitName + " from the entity database.";
	}

	void RunSelectedAnimation()
	{
		AnimationSelection selection = GetAnimationSelection();
		if (!selection.IsValid)
		{
			return;
		}
		if (selection.IsDynamic)
		{
			StartDynamicAnimation(selection.Name, null);
			statusLabel.Text = "Running dynamic animation " + selection.Name + ".";
			return;
		}

		AnimationPlayer player = FindAnimationPlayer(selection.Name);
		if (player == null)
		{
			statusLabel.Text = "No spawned entity has animation " + selection.Name + ". Create a matching entity first.";
			return;
		}
		player.Play(selection.Name);
		statusLabel.Text = "Running animation " + selection.Name + ".";
	}

	void StartBenchmark()
	{
		AnimationSelection selection = GetAnimationSelection();
		if (!selection.IsValid)
		{
			return;
		}

		if (selection.IsDynamic)
		{
			StressTestDynamicAnimation(selection.Name);
			return;
		}

		StressTestAnimationPlayer(selection.Name);
	}

	void StressTestDynamicAnimation(string animationName)
	{
		if (!mAccess.animationManager.dynamicAnimationDefinitions.TryGetValue(animationName, out DynamicAnimationDefinition definition))
		{
			statusLabel.Text = "Unable to run dynamic animation " + animationName + ".";
			return;
		}

		int runs = 0;
		ulong deadline = Time.GetTicksUsec() + 3000000;
		while (Time.GetTicksUsec() < deadline)
		{
			DynamicAnimator animator = new DynamicAnimator(definition, new CircularSpriteAnimationTarget(rigSprites), null);
			animator.Process(Math.Max(definition.Duration, 0.001f));
			runs++;
		}
		statusLabel.Text = "Stress test completed in 3 seconds: " + runs + " dynamic animation runs.";
	}

	void StressTestAnimationPlayer(string animationName)
	{
		AnimationPlayer player = FindAnimationPlayer(animationName);
		if (player == null)
		{
			statusLabel.Text = "No spawned entity has animation " + animationName + ". Create a matching entity first.";
			return;
		}

		Animation animation = player.GetAnimation(animationName);
		if (animation == null)
		{
			statusLabel.Text = "Unable to load animation " + animationName + ".";
			return;
		}

		int runs = 0;
		double duration = Math.Max(animation.Length, 0.001);
		ulong deadline = Time.GetTicksUsec() + 3000000;
		while (Time.GetTicksUsec() < deadline)
		{
			player.Play(animationName);
			player.Advance(duration);
			runs++;
		}

		player.Stop();
		statusLabel.Text = "Stress test completed in 3 seconds: " + runs + " animation runs.";
	}
	
	DynamicAnimator StartDynamicAnimation(string animationName, Action<string> eventHandler)
	{
		if (!mAccess.animationManager.dynamicAnimationDefinitions.TryGetValue(animationName, out DynamicAnimationDefinition definition))
		{
			return null;
		}

		DynamicAnimator animator = new DynamicAnimator(definition, new CircularSpriteAnimationTarget(rigSprites), eventHandler);
		activeDynamicAnimators.Add(animator);
		return animator;
	}

	AnimationPlayer FindAnimationPlayer(string animationName)
	{
		foreach (Node root in spawnedNodes.Where(GodotObject.IsInstanceValid))
		{
			foreach (AnimationPlayer player in FindChildrenOfType<AnimationPlayer>(root))
			{
				if (player.HasAnimation(animationName))
				{
					return player;
				}
			}
		}
		return null;
	}

	IEnumerable<T> FindChildrenOfType<T>(Node node) where T : Node
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is T typedChild)
			{
				yield return typedChild;
			}
			foreach (T descendant in FindChildrenOfType<T>(child))
			{
				yield return descendant;
			}
		}
	}

	void ClearSpawned()
	{
		foreach (Guid id in spawnedUnitIds)
		{
			mAccess.unitManager?.remove(id);
		}
		foreach (Node node in spawnedNodes)
		{
			if (GodotObject.IsInstanceValid(node))
			{
				node.QueueFree();
			}
		}
		spawnedUnitIds.Clear();
		spawnedNodes.Clear();
		statusLabel.Text = "Cleared spawned sandbox entities.";
	}

	string GetSelectedMetadata(OptionButton optionButton)
	{
		if (optionButton.Selected < 0)
		{
			return "";
		}
		return optionButton.GetItemMetadata(optionButton.Selected).AsString();
	}

	AnimationSelection GetAnimationSelection()
	{
		string metadata = GetSelectedMetadata(animationChoice);
		string[] parts = metadata.Split('|', 2);
		if (parts.Length != 2)
		{
			return new AnimationSelection();
		}
		return new AnimationSelection
		{
			IsValid = true,
			IsDynamic = parts[0] == "dynamic",
			Name = parts[1]
		};
	}

	struct AnimationSelection
	{
		public bool IsValid;
		public bool IsDynamic;
		public string Name;
	}
}
