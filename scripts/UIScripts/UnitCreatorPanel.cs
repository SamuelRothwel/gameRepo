using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UnitCreatorPanel : Control
{
	const float PreviewSpriteScale = 4f;
	const float LibraryDragThreshold = 8f;
	VBoxContainer tools;
	VBoxContainer componentsList;
	TabContainer bottomTabs;
	VBoxContainer animationList;
	VBoxContainer animationEditor;
	Control previewArea;
	Control previewLayer;
	UnitSpriteAttachmentData draggedSprite;
	UnitSubUnitAttachmentData draggedSubUnit;
	StoredSprite draggedLibrarySprite;
	StoredUnit draggedLibraryUnit;
	StoredAnimation selectedAnimation;
	bool libraryDragActive;
	TextureRect dragGhost;
	string selectedComponentPath;
	Vector2 dragOffset;
	Vector2 draggedParentOrigin;
	Vector2 libraryDragStart;
	readonly Dictionary<string, bool> expandedComponents = new();

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;
		BuildLayout();
		if (mAccess.unitCreatorManager != null)
		{
			mAccess.unitCreatorManager.unitChanged += OnUnitChanged;
			RefreshComponentsList();
			RefreshPreview();
		}
	}

	public override void _ExitTree()
	{
		if (mAccess.unitCreatorManager != null)
		{
			mAccess.unitCreatorManager.unitChanged -= OnUnitChanged;
		}
	}

	void BuildLayout()
	{
		Control previewArea = CreatePreviewArea();
		AddChild(previewArea);

		PanelContainer leftPanel = CreatePanel(new Vector2(190, 0));
		leftPanel.Name = "ToolsPanel";
		leftPanel.AnchorTop = 0;
		leftPanel.AnchorBottom = 1;
		leftPanel.OffsetLeft = 12;
		leftPanel.OffsetTop = 12;
		leftPanel.OffsetRight = 202;
		leftPanel.OffsetBottom = -12;
		AddChild(leftPanel);

		tools = new VBoxContainer();
		tools.AddThemeConstantOverride("separation", 8);
		leftPanel.AddChild(tools);

		ShowMainTools();

		PanelContainer rightPanel = CreatePanel(new Vector2(230, 0));
		rightPanel.Name = "ComponentsPanel";
		rightPanel.AnchorLeft = 1;
		rightPanel.AnchorRight = 1;
		rightPanel.AnchorTop = 0;
		rightPanel.AnchorBottom = 1;
		rightPanel.OffsetLeft = -242;
		rightPanel.OffsetTop = 12;
		rightPanel.OffsetRight = -12;
		rightPanel.OffsetBottom = -12;
		AddChild(rightPanel);

		VBoxContainer components = new VBoxContainer();
		components.AddThemeConstantOverride("separation", 8);
		rightPanel.AddChild(components);

		components.AddChild(CreatePanelTitle("Components"));

		ScrollContainer componentScroll = new ScrollContainer();
		componentScroll.SizeFlagsVertical = SizeFlags.ExpandFill;
		components.AddChild(componentScroll);

		componentsList = new VBoxContainer();
		componentsList.AddThemeConstantOverride("separation", 6);
		componentScroll.AddChild(componentsList);

		PanelContainer bottomPanel = CreatePanel(new Vector2(0, 160));
		bottomPanel.Name = "InspectorPanel";
		bottomPanel.AnchorLeft = 0;
		bottomPanel.AnchorRight = 1;
		bottomPanel.AnchorTop = 1;
		bottomPanel.AnchorBottom = 1;
		bottomPanel.OffsetLeft = 214;
		bottomPanel.OffsetTop = -172;
		bottomPanel.OffsetRight = -254;
		bottomPanel.OffsetBottom = -12;
		AddChild(bottomPanel);

		bottomTabs = new TabContainer();
		bottomTabs.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		bottomTabs.SizeFlagsVertical = SizeFlags.ExpandFill;
		bottomPanel.AddChild(bottomTabs);

		VBoxContainer inspector = new VBoxContainer();
		inspector.Name = "Inspector";
		inspector.AddThemeConstantOverride("separation", 8);
		bottomTabs.AddChild(inspector);

		Label info = new Label();
		info.Text = "Select a component";
		mAccess.styleManager.applyTextStyle(info, "muted");
		inspector.AddChild(info);

		bottomTabs.AddChild(CreateAnimationTab());
	}

	void ShowMainTools()
	{
		ClearChildren(tools);

		Label toolsTitle = CreatePanelTitle("Tools");
		tools.AddChild(toolsTitle);

		Button spritesButton = new Button();
		spritesButton.Text = "Sprites";
		spritesButton.CustomMinimumSize = new Vector2(150, 34);
		mAccess.styleManager.applyButtonStyle(spritesButton, "secondary");
		spritesButton.Pressed += ShowSpritesSubMenu;
		tools.AddChild(spritesButton);

		Button unitsButton = new Button();
		unitsButton.Text = "Open Units";
		unitsButton.CustomMinimumSize = new Vector2(150, 34);
		mAccess.styleManager.applyButtonStyle(unitsButton, "secondary");
		unitsButton.Pressed += ShowUnitsSubMenu;
		tools.AddChild(unitsButton);

		Button saveUnitButton = new Button();
		saveUnitButton.Text = "Save Unit";
		saveUnitButton.CustomMinimumSize = new Vector2(150, 34);
		mAccess.styleManager.applyButtonStyle(saveUnitButton, "secondary");
		saveUnitButton.Pressed += () => mAccess.unitCreatorManager.SaveActiveUnit();
		tools.AddChild(saveUnitButton);

		tools.AddChild(CreateExpandingSpacer());
	}

	void ShowSpritesSubMenu()
	{
		ClearChildren(tools);

		Button backButton = new Button();
		backButton.Text = "Back";
		backButton.CustomMinimumSize = new Vector2(150, 34);
		mAccess.styleManager.applyButtonStyle(backButton, "secondary");
		backButton.Pressed += ShowMainTools;
		tools.AddChild(backButton);

		tools.AddChild(CreatePanelTitle("Saved Sprites"));

		ScrollContainer scroll = new ScrollContainer();
		scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
		tools.AddChild(scroll);

		VBoxContainer spriteList = new VBoxContainer();
		spriteList.AddThemeConstantOverride("separation", 6);
		scroll.AddChild(spriteList);

		List<StoredSprite> sprites = mAccess.entityFrameworkManager.GetSprites();
		if (sprites.Count == 0)
		{
			Label emptyLabel = new Label();
			emptyLabel.Text = "No saved sprites";
			mAccess.styleManager.applyTextStyle(emptyLabel, "muted");
			spriteList.AddChild(emptyLabel);
		}
		else
		{
			foreach (StoredSprite storedSprite in sprites)
			{
				spriteList.AddChild(CreateSavedSpriteRow(storedSprite));
			}
		}

		Button createSpriteButton = new Button();
		createSpriteButton.Text = "Create Sprite";
		createSpriteButton.CustomMinimumSize = new Vector2(150, 34);
		mAccess.styleManager.applyButtonStyle(createSpriteButton, "secondary");
		createSpriteButton.Pressed += () => mAccess.sceneManager.spriteCreator();
		tools.AddChild(createSpriteButton);
	}

	Control CreateSavedSpriteRow(StoredSprite storedSprite)
	{
		Button rowButton = new Button();
		rowButton.Text = "";
		mAccess.styleManager.applyButtonStyle(rowButton, "secondary");
		rowButton.CustomMinimumSize = new Vector2(150, 54);
		rowButton.GuiInput += inputEvent => HandleLibrarySpriteInput(inputEvent, storedSprite);

		HBoxContainer row = new HBoxContainer();
		row.CustomMinimumSize = new Vector2(150, 54);
		row.SetAnchorsPreset(LayoutPreset.FullRect);
		row.OffsetLeft = 4;
		row.OffsetRight = -4;
		row.MouseFilter = MouseFilterEnum.Ignore;
		rowButton.AddChild(row);

		TextureRect preview = new TextureRect();
		preview.CustomMinimumSize = new Vector2(44, 44);
		preview.Texture = mAccess.spriteCreatorManager.CreateStoredSpritePreview(storedSprite);
		preview.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		preview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		preview.MouseFilter = MouseFilterEnum.Ignore;
		row.AddChild(preview);

		Label nameLabel = new Label();
		nameLabel.Text = storedSprite.Name;
		nameLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		nameLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		nameLabel.MouseFilter = MouseFilterEnum.Ignore;
		mAccess.styleManager.applyTextStyle(nameLabel, "default");
		row.AddChild(nameLabel);

		return rowButton;
	}

	void ShowUnitsSubMenu()
	{
		ClearChildren(tools);

		Button backButton = new Button();
		backButton.Text = "Back";
		backButton.CustomMinimumSize = new Vector2(150, 34);
		mAccess.styleManager.applyButtonStyle(backButton, "secondary");
		backButton.Pressed += ShowMainTools;
		tools.AddChild(backButton);

		tools.AddChild(CreatePanelTitle("Saved Units"));

		ScrollContainer scroll = new ScrollContainer();
		scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
		tools.AddChild(scroll);

		VBoxContainer unitList = new VBoxContainer();
		unitList.AddThemeConstantOverride("separation", 6);
		scroll.AddChild(unitList);

		List<StoredUnit> units = mAccess.entityFrameworkManager.GetUnits();
		if (units.Count == 0)
		{
			Label emptyLabel = new Label();
			emptyLabel.Text = "No saved units";
			mAccess.styleManager.applyTextStyle(emptyLabel, "muted");
			unitList.AddChild(emptyLabel);
		}
		else
		{
			foreach (StoredUnit storedUnit in units)
			{
				unitList.AddChild(CreateSavedUnitRow(storedUnit));
			}
		}

		Button createUnitButton = new Button();
		createUnitButton.Text = "Create Unit";
		createUnitButton.CustomMinimumSize = new Vector2(150, 34);
		mAccess.styleManager.applyButtonStyle(createUnitButton, "secondary");
		createUnitButton.Pressed += () =>
		{
			mAccess.unitCreatorManager.CreateNewUnit();
			ShowMainTools();
		};
		tools.AddChild(createUnitButton);
	}

	Control CreateExpandingSpacer()
	{
		Control spacer = new Control();
		spacer.SizeFlagsVertical = SizeFlags.ExpandFill;
		return spacer;
	}

	Control CreateAnimationTab()
	{
		HSplitContainer split = new HSplitContainer();
		split.Name = "Animation";
		split.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		split.SizeFlagsVertical = SizeFlags.ExpandFill;
		split.SplitOffset = 210;

		VBoxContainer left = new VBoxContainer();
		left.CustomMinimumSize = new Vector2(200, 0);
		left.AddThemeConstantOverride("separation", 6);
		split.AddChild(left);

		HBoxContainer header = new HBoxContainer();
		left.AddChild(header);

		Label title = CreatePanelTitle("Animations");
		title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		header.AddChild(title);

		Button addButton = new Button();
		addButton.Text = "+";
		addButton.CustomMinimumSize = new Vector2(30, 28);
		mAccess.styleManager.applyButtonStyle(addButton, "secondary");
		addButton.Pressed += CreateNewDynamicAnimation;
		header.AddChild(addButton);

		ScrollContainer listScroll = new ScrollContainer();
		listScroll.SizeFlagsVertical = SizeFlags.ExpandFill;
		left.AddChild(listScroll);

		animationList = new VBoxContainer();
		animationList.AddThemeConstantOverride("separation", 4);
		listScroll.AddChild(animationList);

		ScrollContainer editorScroll = new ScrollContainer();
		editorScroll.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		editorScroll.SizeFlagsVertical = SizeFlags.ExpandFill;
		split.AddChild(editorScroll);

		animationEditor = new VBoxContainer();
		animationEditor.AddThemeConstantOverride("separation", 8);
		editorScroll.AddChild(animationEditor);

		RefreshAnimationsTab();
		return split;
	}

	Control CreateSavedUnitRow(StoredUnit storedUnit)
	{
		Button rowButton = new Button();
		rowButton.Text = "";
		mAccess.styleManager.applyButtonStyle(rowButton, "secondary");
		rowButton.CustomMinimumSize = new Vector2(150, 54);
		rowButton.GuiInput += inputEvent => HandleLibraryUnitInput(inputEvent, storedUnit);

		HBoxContainer row = new HBoxContainer();
		row.CustomMinimumSize = new Vector2(150, 54);
		row.SetAnchorsPreset(LayoutPreset.FullRect);
		row.OffsetLeft = 4;
		row.OffsetRight = -4;
		row.MouseFilter = MouseFilterEnum.Ignore;
		rowButton.AddChild(row);

		TextureRect preview = new TextureRect();
		preview.CustomMinimumSize = new Vector2(44, 44);
		preview.Texture = mAccess.unitCreatorManager.CreateUnitPreview(storedUnit);
		preview.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		preview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		preview.MouseFilter = MouseFilterEnum.Ignore;
		row.AddChild(preview);

		Label nameLabel = new Label();
		nameLabel.Text = storedUnit.Name;
		nameLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		nameLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		nameLabel.MouseFilter = MouseFilterEnum.Ignore;
		mAccess.styleManager.applyTextStyle(nameLabel, "default");
		row.AddChild(nameLabel);

		return rowButton;
	}

	void ClearChildren(Node parent)
	{
		foreach (Node child in parent.GetChildren())
		{
			parent.RemoveChild(child);
			child.QueueFree();
		}
	}

	Control CreatePreviewArea()
	{
		Panel preview = new Panel();
		preview.Name = "PreviewArea";
		preview.SetAnchorsPreset(LayoutPreset.FullRect);
		preview.OffsetLeft = 214;
		preview.OffsetTop = 12;
		preview.OffsetRight = -254;
		preview.OffsetBottom = -184;
		MakeInputTransparent(preview);
		mAccess.styleManager.applyPanelStyle(preview, "subtle");
		previewArea = preview;

		previewLayer = new Control();
		previewLayer.Name = "PreviewLayer";
		previewLayer.SetAnchorsPreset(LayoutPreset.FullRect);
		MakeInputTransparent(previewLayer);
		preview.AddChild(previewLayer);

		return preview;
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (previewArea == null || previewLayer == null || mAccess.unitCreatorManager?.activeUnit == null)
		{
			return;
		}

		if (draggedLibrarySprite != null || draggedLibraryUnit != null)
		{
			HandleActiveLibraryDrag(inputEvent);
			if (libraryDragActive)
			{
				return;
			}
		}

		if (inputEvent is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
		{
			if (mouseButton.Pressed)
			{
				StartSpriteDrag(mouseButton.GlobalPosition);
			}
			else
			{
				draggedSprite = null;
				draggedSubUnit = null;
			}
			return;
		}

		if (inputEvent is InputEventMouseMotion mouseMotion && (draggedSprite != null || draggedSubUnit != null))
		{
			Vector2 localPosition = previewLayer.GetGlobalTransformWithCanvas().AffineInverse() * mouseMotion.GlobalPosition;
			Vector2 centeredPosition = localPosition - previewLayer.Size / 2f - dragOffset;
			if (draggedSprite != null)
			{
				mAccess.unitCreatorManager.MoveSpriteAttachment(draggedSprite, centeredPosition - draggedParentOrigin);
			}
			else
			{
				mAccess.unitCreatorManager.MoveSubUnitAttachment(draggedSubUnit, centeredPosition - draggedParentOrigin);
			}
			AcceptEvent();
		}
	}

	void HandleLibrarySpriteInput(InputEvent inputEvent, StoredSprite storedSprite)
	{
		if (inputEvent is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
		{
			if (mouseButton.Pressed)
			{
				BeginLibrarySpriteDrag(storedSprite, mouseButton.GlobalPosition);
			}
			else
			{
				EndLibrarySpriteDrag(mouseButton.GlobalPosition);
			}
			AcceptEvent();
		}
		else if (inputEvent is InputEventMouseMotion mouseMotion)
		{
			UpdateLibraryDrag(mouseMotion.GlobalPosition);
			if (libraryDragActive)
			{
				AcceptEvent();
			}
		}
	}

	void HandleLibraryUnitInput(InputEvent inputEvent, StoredUnit storedUnit)
	{
		if (inputEvent is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
		{
			if (mouseButton.Pressed)
			{
				BeginLibraryUnitDrag(storedUnit, mouseButton.GlobalPosition);
			}
			else
			{
				EndLibraryUnitDrag(mouseButton.GlobalPosition);
			}
			AcceptEvent();
		}
		else if (inputEvent is InputEventMouseMotion mouseMotion)
		{
			UpdateLibraryDrag(mouseMotion.GlobalPosition);
			if (libraryDragActive)
			{
				AcceptEvent();
			}
		}
	}

	void HandleActiveLibraryDrag(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseMotion mouseMotion)
		{
			UpdateLibraryDrag(mouseMotion.GlobalPosition);
			if (libraryDragActive)
			{
				AcceptEvent();
			}
		}
		else if (inputEvent is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && !mouseButton.Pressed)
		{
			if (draggedLibrarySprite != null)
			{
				EndLibrarySpriteDrag(mouseButton.GlobalPosition);
			}
			else
			{
				EndLibraryUnitDrag(mouseButton.GlobalPosition);
			}
			AcceptEvent();
		}
	}

	void BeginLibrarySpriteDrag(StoredSprite storedSprite, Vector2 globalPosition)
	{
		draggedLibrarySprite = storedSprite;
		draggedLibraryUnit = null;
		libraryDragActive = false;
		libraryDragStart = globalPosition;
	}

	void BeginLibraryUnitDrag(StoredUnit storedUnit, Vector2 globalPosition)
	{
		draggedLibraryUnit = storedUnit;
		draggedLibrarySprite = null;
		libraryDragActive = false;
		libraryDragStart = globalPosition;
	}

	void UpdateLibraryDrag(Vector2 globalPosition)
	{
		if (draggedLibrarySprite == null && draggedLibraryUnit == null)
		{
			return;
		}

		if (!libraryDragActive && globalPosition.DistanceTo(libraryDragStart) >= LibraryDragThreshold)
		{
			libraryDragActive = true;
		}
		UpdateLibraryDragGhost(globalPosition);
	}

	void EndLibrarySpriteDrag(Vector2 globalPosition)
	{
		if (draggedLibrarySprite == null)
		{
			return;
		}

		if (libraryDragActive && TryGetPreviewCenteredPosition(globalPosition, out Vector2 dropPosition))
		{
			mAccess.unitCreatorManager.AddSpriteAttachment(draggedLibrarySprite, dropPosition);
			ShowMainTools();
		}
		else if (!libraryDragActive)
		{
			mAccess.unitCreatorManager.AddSpriteAttachment(draggedLibrarySprite);
			ShowMainTools();
		}

		ClearLibraryDrag();
	}

	void EndLibraryUnitDrag(Vector2 globalPosition)
	{
		if (draggedLibraryUnit == null)
		{
			return;
		}

		if (libraryDragActive && TryGetPreviewCenteredPosition(globalPosition, out Vector2 dropPosition))
		{
			mAccess.unitCreatorManager.AddSubUnitAttachment(draggedLibraryUnit, dropPosition);
			ShowMainTools();
		}
		else if (!libraryDragActive)
		{
			mAccess.unitCreatorManager.LoadStoredUnit(draggedLibraryUnit);
			ShowMainTools();
		}

		ClearLibraryDrag();
	}

	void ClearLibraryDrag()
	{
		draggedLibrarySprite = null;
		draggedLibraryUnit = null;
		libraryDragActive = false;
		ClearDragGhost();
	}

	void UpdateLibraryDragGhost(Vector2 globalPosition)
	{
		if (!libraryDragActive || !TryGetPreviewCenteredPosition(globalPosition, out Vector2 dropPosition))
		{
			ClearDragGhost();
			return;
		}

		Texture2D texture = GetDraggedLibraryTexture();
		if (texture == null)
		{
			ClearDragGhost();
			return;
		}

		if (dragGhost == null)
		{
			dragGhost = new TextureRect();
			dragGhost.Texture = texture;
			dragGhost.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			dragGhost.StretchMode = TextureRect.StretchModeEnum.Scale;
			dragGhost.Modulate = new Color(1, 1, 1, 0.55f);
			MakeInputTransparent(dragGhost);
			dragGhost.SetAnchorsPreset(LayoutPreset.Center);
			previewLayer.AddChild(dragGhost);
		}
		else
		{
			dragGhost.Texture = texture;
		}

		Vector2 size = texture.GetSize() * PreviewSpriteScale;
		dragGhost.OffsetLeft = dropPosition.X - size.X / 2f;
		dragGhost.OffsetTop = dropPosition.Y - size.Y / 2f;
		dragGhost.OffsetRight = dropPosition.X + size.X / 2f;
		dragGhost.OffsetBottom = dropPosition.Y + size.Y / 2f;
	}

	Texture2D GetDraggedLibraryTexture()
	{
		if (draggedLibrarySprite != null)
		{
			return mAccess.spriteCreatorManager.CreateStoredSpritePreview(draggedLibrarySprite);
		}
		if (draggedLibraryUnit != null)
		{
			return mAccess.unitCreatorManager.CreateUnitPreview(draggedLibraryUnit);
		}

		return null;
	}

	void ClearDragGhost()
	{
		if (dragGhost == null)
		{
			return;
		}

		previewLayer.RemoveChild(dragGhost);
		dragGhost.QueueFree();
		dragGhost = null;
	}

	bool TryGetPreviewCenteredPosition(Vector2 globalPosition, out Vector2 centeredPosition)
	{
		centeredPosition = Vector2.Zero;
		Vector2 localPosition = previewLayer.GetGlobalTransformWithCanvas().AffineInverse() * globalPosition;
		if (!new Rect2(Vector2.Zero, previewLayer.Size).HasPoint(localPosition))
		{
			return false;
		}

		centeredPosition = localPosition - previewLayer.Size / 2f;
		return true;
	}

	void StartSpriteDrag(Vector2 globalPosition)
	{
		Vector2 localPosition = previewLayer.GetGlobalTransformWithCanvas().AffineInverse() * globalPosition;
		if (!new Rect2(Vector2.Zero, previewLayer.Size).HasPoint(localPosition))
		{
			return;
		}

		Vector2 centeredPosition = localPosition - previewLayer.Size / 2f;
		PreviewHit hit = FindDeepestPreviewHit(mAccess.unitCreatorManager.activeUnit, "unit:" + mAccess.unitCreatorManager.activeUnit.Id, Vector2.Zero, localPosition);
		if (hit?.Sprite != null)
		{
			draggedSprite = hit.Sprite;
			dragOffset = centeredPosition - hit.GlobalPosition;
			draggedParentOrigin = hit.ParentOrigin;
			SelectComponent(hit.Path);
			AcceptEvent();
			return;
		}
		if (hit?.SubUnit != null)
		{
			draggedSubUnit = hit.SubUnit;
			dragOffset = centeredPosition - hit.GlobalPosition;
			draggedParentOrigin = hit.ParentOrigin;
			SelectComponent(hit.Path);
			AcceptEvent();
			return;
		}
	}

	PreviewHit FindDeepestPreviewHit(UnitDefinition definition, string path, Vector2 origin, Vector2 localPosition)
	{
		foreach (UnitSubUnitAttachmentData subUnit in definition.SubUnitAttachments.OrderByDescending(subUnit => subUnit.Order))
		{
			UnitDefinition childDefinition = mAccess.unitCreatorManager.GetUnitDefinition(subUnit.ChildUnitId);
			string subUnitPath = path + "/subUnit:" + subUnit.Id;
			if (childDefinition != null)
			{
				PreviewHit childHit = FindDeepestPreviewHit(childDefinition, subUnitPath, origin + subUnit.Position, localPosition);
				if (childHit != null)
				{
					return childHit;
				}
			}

			Rect2 bounds = GetSubUnitPreviewBounds(subUnit, origin);
			if (bounds.HasPoint(localPosition))
			{
				return new PreviewHit
				{
					Path = subUnitPath,
					SubUnit = subUnit,
					GlobalPosition = origin + subUnit.Position,
					ParentOrigin = origin
				};
			}
		}

		foreach (UnitSpriteAttachmentData sprite in definition.SpriteAttachments.OrderByDescending(sprite => sprite.Order))
		{
			Texture2D texture = mAccess.unitCreatorManager.CreateAttachmentPreview(sprite);
			if (texture == null)
			{
				continue;
			}

			Rect2 centeredBounds = GetCenteredSpritePreviewBounds(sprite, texture, origin);
			Rect2 bounds = new Rect2(centeredBounds.Position + previewLayer.Size / 2f, centeredBounds.Size);
			if (bounds.HasPoint(localPosition))
			{
				return new PreviewHit
				{
					Path = path + "/sprite:" + sprite.Id,
					Sprite = sprite,
					GlobalPosition = origin + sprite.Position,
					ParentOrigin = origin
				};
			}
		}

		return null;
	}

	PanelContainer CreatePanel(Vector2 minimumSize)
	{
		PanelContainer panel = new PanelContainer();
		panel.CustomMinimumSize = minimumSize;
		panel.MouseFilter = MouseFilterEnum.Stop;
		mAccess.styleManager.applyPanelStyle(panel, "default");
		return panel;
	}

	Label CreatePanelTitle(string text)
	{
		Label label = new Label();
		label.Text = text;
		mAccess.styleManager.applyTextStyle(label, "title");
		return label;
	}

	void RefreshAnimationsTab()
	{
		if (animationList == null || animationEditor == null)
		{
			return;
		}

		ClearChildren(animationList);
		List<StoredAnimation> animations = GetUsableAnimations();
		if (selectedAnimation != null)
		{
			selectedAnimation = animations.FirstOrDefault(animation => animation.Id == selectedAnimation.Id);
		}

		if (animations.Count == 0)
		{
			Label empty = new Label();
			empty.Text = "No usable animations";
			mAccess.styleManager.applyTextStyle(empty, "muted");
			animationList.AddChild(empty);
		}
		else
		{
			foreach (StoredAnimation animation in animations)
			{
				animationList.AddChild(CreateAnimationRow(animation));
			}
		}

		RefreshAnimationEditor();
	}

	Control CreateAnimationRow(StoredAnimation animation)
	{
		Button button = new Button();
		button.Text = animation.Name;
		button.Alignment = HorizontalAlignment.Left;
		button.CustomMinimumSize = new Vector2(180, 28);
		mAccess.styleManager.applyButtonStyle(button, selectedAnimation?.Id == animation.Id ? "selected" : "secondary");
		button.Pressed += () =>
		{
			selectedAnimation = animation;
			RefreshAnimationsTab();
		};
		return button;
	}

	List<StoredAnimation> GetUsableAnimations()
	{
		if (mAccess.entityFrameworkManager == null)
		{
			return new List<StoredAnimation>();
		}

		HashSet<string> usableTypeNames = GetUsableMetadataTypeNames();
		return mAccess.entityFrameworkManager.GetAnimations()
			.Where(animation => AnimationMatchesUnitMetadata(animation, usableTypeNames))
			.OrderBy(animation => animation.Name)
			.ToList();
	}

	bool AnimationMatchesUnitMetadata(StoredAnimation animation, HashSet<string> usableTypeNames)
	{
		if (animation.PropertyRequirements == null || animation.PropertyRequirements.Count == 0)
		{
			return true;
		}

		return animation.PropertyRequirements.All(requirement =>
			string.IsNullOrEmpty(requirement.TargetTypeName) ||
			usableTypeNames.Contains(requirement.TargetTypeName));
	}

	HashSet<string> GetUsableMetadataTypeNames()
	{
		HashSet<string> typeNames = new HashSet<string>();
		foreach (StoredUnitComponentType metadata in GetUsableMetadataTypes())
		{
			typeNames.Add(metadata.TypeName);
			if (!string.IsNullOrEmpty(metadata.DirectBaseTypeName))
			{
				typeNames.Add(metadata.DirectBaseTypeName);
			}
			foreach (StoredUnitComponentObjectMember member in metadata.ObjectMembers)
			{
				if (!string.IsNullOrEmpty(member.MemberTypeName))
				{
					typeNames.Add(member.MemberTypeName);
				}
				if (!string.IsNullOrEmpty(member.ElementTypeName))
				{
					typeNames.Add(member.ElementTypeName);
				}
			}
		}
		return typeNames;
	}

	List<StoredUnitComponentType> GetUsableMetadataTypes()
	{
		if (mAccess.unitManager?.unitVariableMetadata == null)
		{
			return new List<StoredUnitComponentType>();
		}

		List<StoredUnitComponentType> selectedTypes = GetSelectedMetadataTypes();
		if (selectedTypes.Count > 0)
		{
			return selectedTypes
				.OrderBy(type => type.DisplayName)
				.ToList();
		}

		return mAccess.unitManager.unitVariableMetadata.Values
			.Where(type => type.Kind == "unit")
			.OrderBy(type => type.DisplayName)
			.ToList();
	}

	List<StoredUnitComponentType> GetSelectedMetadataTypes()
	{
		if (mAccess.unitManager?.unitVariableMetadata == null || mAccess.unitCreatorManager?.activeUnit == null)
		{
			return new List<StoredUnitComponentType>();
		}

		if (string.IsNullOrEmpty(selectedComponentPath))
		{
			return GetUnitMetadataTypes(mAccess.unitCreatorManager.activeUnit);
		}

		string[] pathParts = selectedComponentPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
		string selectedPart = pathParts.LastOrDefault() ?? "";
		if (selectedPart.StartsWith("sprite:"))
		{
			return GetSpriteMetadataTypes();
		}

		UnitDefinition selectedDefinition = GetDefinitionForPath(pathParts);
		return selectedDefinition == null
			? new List<StoredUnitComponentType>()
			: GetUnitMetadataTypes(selectedDefinition);
	}

	List<StoredUnitComponentType> GetUnitMetadataTypes(UnitDefinition definition)
	{
		if (definition == null)
		{
			return new List<StoredUnitComponentType>();
		}

		return mAccess.unitManager.unitVariableMetadata.Values
			.Where(type => type.Kind == "unit" && TypeMatchesUnitName(type, definition.Name))
			.ToList();
	}

	List<StoredUnitComponentType> GetSpriteMetadataTypes()
	{
		const string spriteTypeName = "Godot.Sprite2D";
		return mAccess.unitManager.unitVariableMetadata.Values
			.Where(type =>
				type.TypeName == spriteTypeName ||
				type.DirectBaseTypeName == spriteTypeName ||
				type.ObjectMembers.Any(member =>
					member.MemberTypeName == spriteTypeName ||
					member.ElementTypeName == spriteTypeName))
			.ToList();
	}

	bool TypeMatchesUnitName(StoredUnitComponentType type, string unitName)
	{
		return string.Equals(type.DisplayName, unitName, StringComparison.OrdinalIgnoreCase) ||
			string.Equals(type.TypeName.Split('.').LastOrDefault(), unitName, StringComparison.OrdinalIgnoreCase);
	}

	UnitDefinition GetDefinitionForPath(string[] pathParts)
	{
		UnitDefinition currentDefinition = mAccess.unitCreatorManager.activeUnit;
		foreach (string pathPart in pathParts.Skip(1))
		{
			if (!pathPart.StartsWith("subUnit:"))
			{
				continue;
			}

			if (!Guid.TryParse(pathPart["subUnit:".Length..], out Guid subUnitId))
			{
				return currentDefinition;
			}

			UnitSubUnitAttachmentData subUnit = currentDefinition.SubUnitAttachments.FirstOrDefault(subUnit => subUnit.Id == subUnitId);
			if (subUnit == null)
			{
				return currentDefinition;
			}

			currentDefinition = mAccess.unitCreatorManager.GetUnitDefinition(subUnit.ChildUnitId);
			if (currentDefinition == null)
			{
				return null;
			}
		}

		return currentDefinition;
	}

	void RefreshAnimationEditor()
	{
		ClearChildren(animationEditor);
		if (selectedAnimation == null)
		{
			Label hint = new Label();
			hint.Text = "Open or create an animation";
			mAccess.styleManager.applyTextStyle(hint, "muted");
			animationEditor.AddChild(hint);
			return;
		}

		HBoxContainer header = new HBoxContainer();
		header.AddThemeConstantOverride("separation", 8);
		animationEditor.AddChild(header);

		LineEdit nameEdit = new LineEdit();
		nameEdit.Text = selectedAnimation.Name;
		nameEdit.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		nameEdit.TextSubmitted += value =>
		{
			selectedAnimation.Name = value;
			SaveSelectedAnimation();
		};
		nameEdit.FocusExited += () =>
		{
			selectedAnimation.Name = nameEdit.Text;
			SaveSelectedAnimation();
		};
		header.AddChild(nameEdit);

		Button addTrackButton = new Button();
		addTrackButton.Text = "Add Variable";
		addTrackButton.CustomMinimumSize = new Vector2(120, 30);
		mAccess.styleManager.applyButtonStyle(addTrackButton, "secondary");
		addTrackButton.Pressed += ShowVariablePicker;
		header.AddChild(addTrackButton);

		foreach (StoredAnimationTransformation track in selectedAnimation.Transformations.OrderBy(track => track.PropertyName))
		{
			animationEditor.AddChild(CreateAnimationTrackRow(track));
		}

		if (selectedAnimation.Transformations.Count == 0)
		{
			Label empty = new Label();
			empty.Text = "No variable tracks";
			mAccess.styleManager.applyTextStyle(empty, "muted");
			animationEditor.AddChild(empty);
		}
	}

	Control CreateAnimationTrackRow(StoredAnimationTransformation track)
	{
		PanelContainer panel = CreatePanel(new Vector2(0, 34));
		HBoxContainer row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 6);
		panel.AddChild(row);

		Label name = new Label();
		name.Text = track.PropertyName;
		name.CustomMinimumSize = new Vector2(180, 24);
		name.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		mAccess.styleManager.applyTextStyle(name, "default");
		row.AddChild(name);

		AddTrackNumberField(row, "Start", track.StartTime, value => track.StartTime = value);
		AddTrackNumberField(row, "End", track.EndTime, value => track.EndTime = value);
		AddTrackTextField(row, track.StartValue, value => track.StartValue = value);
		AddTrackTextField(row, track.EndValue, value => track.EndValue = value);
		AddTrackTextField(row, track.FunctionType, value => track.FunctionType = value);

		Button remove = new Button();
		remove.Text = "x";
		remove.CustomMinimumSize = new Vector2(28, 26);
		mAccess.styleManager.applyButtonStyle(remove, "secondary");
		remove.Pressed += () =>
		{
			selectedAnimation.Transformations.Remove(track);
			SaveSelectedAnimation();
		};
		row.AddChild(remove);

		return panel;
	}

	void AddTrackNumberField(HBoxContainer row, string placeholder, float value, Action<float> setValue)
	{
		LineEdit edit = new LineEdit();
		edit.PlaceholderText = placeholder;
		edit.Text = value.ToString();
		edit.CustomMinimumSize = new Vector2(52, 26);
		edit.FocusExited += () =>
		{
			if (float.TryParse(edit.Text, out float parsed))
			{
				setValue(parsed);
				SaveSelectedAnimation();
			}
		};
		row.AddChild(edit);
	}

	void AddTrackTextField(HBoxContainer row, string value, Action<string> setValue)
	{
		LineEdit edit = new LineEdit();
		edit.Text = value;
		edit.CustomMinimumSize = new Vector2(72, 26);
		edit.FocusExited += () =>
		{
			setValue(edit.Text);
			SaveSelectedAnimation();
		};
		row.AddChild(edit);
	}

	void CreateNewDynamicAnimation()
	{
		if (mAccess.unitCreatorManager?.activeUnit == null || mAccess.entityFrameworkManager == null)
		{
			return;
		}

		selectedAnimation = new StoredAnimation
		{
			Id = Guid.NewGuid(),
			Name = mAccess.unitCreatorManager.activeUnit.Name + " animation",
			AnimationType = "dynamic",
			Duration = 1,
			Variables = new List<StoredAnimationVariable>(),
			PropertyRequirements = new List<StoredAnimationPropertyRequirement>(),
			Transformations = new List<StoredAnimationTransformation>()
		};
		SaveSelectedAnimation();
	}

	void ShowVariablePicker()
	{
		if (selectedAnimation == null)
		{
			return;
		}

		VBoxContainer content = new VBoxContainer();
		content.CustomMinimumSize = new Vector2(420, 460);
		content.AddThemeConstantOverride("separation", 6);

		ScrollContainer scroll = new ScrollContainer();
		scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
		content.AddChild(scroll);

		VBoxContainer variables = new VBoxContainer();
		variables.AddThemeConstantOverride("separation", 4);
		scroll.AddChild(variables);

		foreach (AnimationVariableChoice choice in GetAnimationVariableChoices())
		{
			Button button = new Button();
			button.Text = choice.Label;
			button.Alignment = HorizontalAlignment.Left;
			button.CustomMinimumSize = new Vector2(380, 28);
			mAccess.styleManager.applyButtonStyle(button, "secondary");
			button.Pressed += () =>
			{
				AddAnimationTrack(choice);
				mAccess.windowManager.closeWindow("Animation Variables", false);
			};
			variables.AddChild(button);
		}

		mAccess.windowManager.openWindow("Animation Variables", content);
	}

	List<AnimationVariableChoice> GetAnimationVariableChoices()
	{
		List<AnimationVariableChoice> choices = new List<AnimationVariableChoice>();
		foreach (StoredUnitComponentType metadata in GetUsableMetadataTypes())
		{
			foreach (StoredUnitComponentVariable variable in metadata.Variables.Where(variable => variable.CanWrite))
			{
				choices.Add(new AnimationVariableChoice
				{
					TargetName = metadata.DisplayName,
					TargetTypeName = metadata.TypeName,
					PropertyName = variable.Name,
					ValueTypeName = variable.ValueTypeName,
					Label = metadata.DisplayName + "." + variable.Name + " : " + variable.ValueTypeName
				});
			}
		}

		return choices
			.OrderBy(choice => choice.Label)
			.ToList();
	}

	void AddAnimationTrack(AnimationVariableChoice choice)
	{
		if (selectedAnimation == null)
		{
			return;
		}

		string propertyName = choice.TargetName + "." + choice.PropertyName;
		if (!selectedAnimation.PropertyRequirements.Any(requirement =>
			requirement.TargetTypeName == choice.TargetTypeName &&
			requirement.PropertyName == choice.PropertyName))
		{
			selectedAnimation.PropertyRequirements.Add(new StoredAnimationPropertyRequirement
			{
				TargetName = choice.TargetName,
				TargetTypeName = choice.TargetTypeName,
				PropertyName = choice.PropertyName,
				ValueTypeName = choice.ValueTypeName,
				InterfaceName = CreateAnimationInterfaceName(choice.TargetName)
			});
		}

		selectedAnimation.Transformations.Add(new StoredAnimationTransformation
		{
			PropertyName = propertyName,
			StartTime = 0,
			EndTime = selectedAnimation.Duration <= 0 ? 1 : selectedAnimation.Duration,
			StartValue = "0",
			EndValue = "0",
			FunctionType = "linear"
		});
		SaveSelectedAnimation();
	}

	string CreateAnimationInterfaceName(string targetName)
	{
		string cleaned = new string(targetName.Where(char.IsLetterOrDigit).ToArray());
		return "I" + (string.IsNullOrEmpty(cleaned) ? "AnimationTarget" : cleaned) + "AnimationProperties";
	}

	void SaveSelectedAnimation()
	{
		if (selectedAnimation == null || mAccess.entityFrameworkManager == null)
		{
			return;
		}

		mAccess.entityFrameworkManager.SaveAnimation(selectedAnimation);
		RefreshAnimationsTab();
	}

	void OnUnitChanged(object sender, EventArgs args)
	{
		RefreshComponentsList();
		RefreshPreview();
		RefreshAnimationsTab();
	}

	void RefreshPreview()
	{
		ClearDragGhost();
		foreach (Node child in previewLayer.GetChildren())
		{
			previewLayer.RemoveChild(child);
			child.QueueFree();
		}

		if (mAccess.unitCreatorManager?.activeUnit == null)
		{
			return;
		}

		AddUnitDefinitionPreview(mAccess.unitCreatorManager.activeUnit, Vector2.Zero);
	}

	void AddUnitDefinitionPreview(UnitDefinition definition, Vector2 origin)
	{
		foreach (UnitSpriteAttachmentData sprite in definition.SpriteAttachments.OrderBy(sprite => sprite.Order))
		{
			AddPreviewSprite(sprite, origin);
		}

		foreach (UnitSubUnitAttachmentData subUnit in definition.SubUnitAttachments.OrderBy(subUnit => subUnit.Order))
		{
			AddSubUnitPreview(subUnit, origin);
		}
	}

	void AddSubUnitPreview(UnitSubUnitAttachmentData subUnit, Vector2 origin)
	{
		UnitDefinition childDefinition = mAccess.unitCreatorManager.GetUnitDefinition(subUnit.ChildUnitId);
		if (childDefinition == null)
		{
			return;
		}

		AddUnitDefinitionPreview(childDefinition, origin + subUnit.Position);
	}

	void AddPreviewSprite(UnitSpriteAttachmentData sprite, Vector2 origin)
	{
		Texture2D texture = mAccess.unitCreatorManager.CreateAttachmentPreview(sprite);
		if (texture == null)
		{
			return;
		}

		TextureRect spritePreview = new TextureRect();
		spritePreview.Texture = texture;
		spritePreview.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		spritePreview.StretchMode = TextureRect.StretchModeEnum.Scale;
		MakeInputTransparent(spritePreview);
		spritePreview.SetAnchorsPreset(LayoutPreset.Center);
		Rect2 bounds = GetCenteredSpritePreviewBounds(sprite, texture, origin);
		spritePreview.OffsetLeft = bounds.Position.X;
		spritePreview.OffsetTop = bounds.Position.Y;
		spritePreview.OffsetRight = bounds.End.X;
		spritePreview.OffsetBottom = bounds.End.Y;
		spritePreview.Rotation = sprite.Rotation;
		previewLayer.AddChild(spritePreview);
	}

	Rect2 GetSpritePreviewBounds(UnitSpriteAttachmentData sprite)
	{
		Texture2D texture = mAccess.unitCreatorManager.CreateAttachmentPreview(sprite);
		if (texture == null)
		{
			return new Rect2();
		}

		Rect2 centeredBounds = GetCenteredSpritePreviewBounds(sprite, texture, Vector2.Zero);
		return new Rect2(centeredBounds.Position + previewLayer.Size / 2f, centeredBounds.Size);
	}

	Rect2 GetSubUnitPreviewBounds(UnitSubUnitAttachmentData subUnit, Vector2 origin)
	{
		UnitDefinition childDefinition = mAccess.unitCreatorManager.GetUnitDefinition(subUnit.ChildUnitId);
		if (childDefinition == null)
		{
			return new Rect2();
		}

		Rect2 bounds = new Rect2();
		bool hasBounds = false;
		foreach (UnitSpriteAttachmentData sprite in childDefinition.SpriteAttachments)
		{
			Texture2D texture = mAccess.unitCreatorManager.CreateAttachmentPreview(sprite);
			if (texture == null)
			{
				continue;
			}

			Rect2 spriteBounds = GetCenteredSpritePreviewBounds(sprite, texture, origin + subUnit.Position);
			Rect2 localBounds = new Rect2(spriteBounds.Position + previewLayer.Size / 2f, spriteBounds.Size);
			bounds = hasBounds ? bounds.Merge(localBounds) : localBounds;
			hasBounds = true;
		}

		foreach (UnitSubUnitAttachmentData childSubUnit in childDefinition.SubUnitAttachments)
		{
			Rect2 childBounds = GetSubUnitPreviewBounds(childSubUnit, origin + subUnit.Position);
			if (childBounds.Size == Vector2.Zero)
			{
				continue;
			}

			bounds = hasBounds ? bounds.Merge(childBounds) : childBounds;
			hasBounds = true;
		}

		return hasBounds ? bounds : new Rect2(subUnit.Position + previewLayer.Size / 2f, Vector2.One);
	}

	Rect2 GetCenteredSpritePreviewBounds(UnitSpriteAttachmentData sprite, Texture2D texture, Vector2 origin)
	{
		Vector2 textureSize = texture.GetSize() * PreviewSpriteScale * sprite.Scale;
		return new Rect2(origin + sprite.Position - textureSize / 2f, textureSize);
	}

	void MakeInputTransparent(Control control)
	{
		control.MouseFilter = MouseFilterEnum.Ignore;
		control.FocusMode = FocusModeEnum.None;
	}

	void RefreshComponentsList()
	{
		foreach (Node child in componentsList.GetChildren())
		{
			componentsList.RemoveChild(child);
			child.QueueFree();
		}

		if (mAccess.unitCreatorManager?.activeUnit == null)
		{
			return;
		}

		AddUnitComponentRows(mAccess.unitCreatorManager.activeUnit, "unit:" + mAccess.unitCreatorManager.activeUnit.Id, 0, new HashSet<Guid>());
	}

	void AddUnitComponentRows(UnitDefinition definition, string path, int depth, HashSet<Guid> visitedUnits)
	{
		bool expanded = IsExpanded(path);
		AddComponentRow(definition.Name, path, depth, true, expanded, () => ToggleExpanded(path));

		if (!expanded || visitedUnits.Contains(definition.Id))
		{
			return;
		}

		visitedUnits.Add(definition.Id);
		foreach (UnitSpriteAttachmentData sprite in definition.SpriteAttachments.OrderBy(sprite => sprite.Order))
		{
			AddComponentRow(sprite.Name, path + "/sprite:" + sprite.Id, depth + 1, false, false, null);
		}
		foreach (UnitSubUnitAttachmentData subUnit in definition.SubUnitAttachments.OrderBy(subUnit => subUnit.Order))
		{
			UnitDefinition childDefinition = mAccess.unitCreatorManager.GetUnitDefinition(subUnit.ChildUnitId);
			string childName = childDefinition == null ? "Unknown Unit" : childDefinition.Name;
			string childPath = path + "/subUnit:" + subUnit.Id;
			AddSubUnitComponentRows(subUnit, childDefinition, childName, childPath, depth + 1, new HashSet<Guid>(visitedUnits));
		}
	}

	void AddSubUnitComponentRows(UnitSubUnitAttachmentData subUnit, UnitDefinition childDefinition, string childName, string path, int depth, HashSet<Guid> visitedUnits)
	{
		bool hasChildren = childDefinition != null && (childDefinition.SpriteAttachments.Count > 0 || childDefinition.SubUnitAttachments.Count > 0);
		bool expanded = IsExpanded(path);
		AddComponentRow(subUnit.Name + " (" + childName + ")", path, depth, hasChildren, expanded, hasChildren ? () => ToggleExpanded(path) : null);

		if (!hasChildren || !expanded || childDefinition == null || visitedUnits.Contains(childDefinition.Id))
		{
			return;
		}

		visitedUnits.Add(childDefinition.Id);
		foreach (UnitSpriteAttachmentData sprite in childDefinition.SpriteAttachments.OrderBy(sprite => sprite.Order))
		{
			AddComponentRow(sprite.Name, path + "/sprite:" + sprite.Id, depth + 1, false, false, null);
		}
		foreach (UnitSubUnitAttachmentData childSubUnit in childDefinition.SubUnitAttachments.OrderBy(subUnit => subUnit.Order))
		{
			UnitDefinition grandchildDefinition = mAccess.unitCreatorManager.GetUnitDefinition(childSubUnit.ChildUnitId);
			string grandchildName = grandchildDefinition == null ? "Unknown Unit" : grandchildDefinition.Name;
			AddSubUnitComponentRows(childSubUnit, grandchildDefinition, grandchildName, path + "/subUnit:" + childSubUnit.Id, depth + 1, new HashSet<Guid>(visitedUnits));
		}
	}

	bool IsExpanded(string path)
	{
		if (!expandedComponents.ContainsKey(path))
		{
			expandedComponents[path] = true;
		}

		return expandedComponents[path];
	}

	void ToggleExpanded(string path)
	{
		expandedComponents[path] = !IsExpanded(path);
		RefreshComponentsList();
	}

	void SelectComponent(string path)
	{
		selectedComponentPath = path;
		RefreshComponentsList();
		RefreshAnimationsTab();
	}

	void AddComponentRow(string name, string path, int depth, bool hasChildren, bool expanded, Action toggle)
	{
		HBoxContainer row = new HBoxContainer();
		row.CustomMinimumSize = new Vector2(180, 30);
		row.AddThemeConstantOverride("separation", 4);

		Control indent = new Control();
		indent.CustomMinimumSize = new Vector2(depth * 14, 1);
		row.AddChild(indent);

		Button arrow = new Button();
		arrow.Text = hasChildren ? (expanded ? "v" : ">") : "";
		arrow.CustomMinimumSize = new Vector2(24, 24);
		arrow.Disabled = !hasChildren;
		mAccess.styleManager.applyButtonStyle(arrow, "secondary");
		if (toggle != null)
		{
			arrow.Pressed += toggle;
		}
		row.AddChild(arrow);

		Button label = new Button();
		label.Text = name;
		label.Alignment = HorizontalAlignment.Left;
		label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		label.CustomMinimumSize = new Vector2(120, 28);
		mAccess.styleManager.applyButtonStyle(label, selectedComponentPath == path ? "selected" : "secondary");
		label.Pressed += () => SelectComponent(path);
		row.AddChild(label);

		componentsList.AddChild(row);
	}

	class PreviewHit
	{
		public string Path;
		public UnitSpriteAttachmentData Sprite;
		public UnitSubUnitAttachmentData SubUnit;
		public Vector2 GlobalPosition;
		public Vector2 ParentOrigin;
	}

	class AnimationVariableChoice
	{
		public string TargetName;
		public string TargetTypeName;
		public string PropertyName;
		public string ValueTypeName;
		public string Label;
	}
}
