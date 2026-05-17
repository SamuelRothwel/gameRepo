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
	Control previewArea;
	Control previewLayer;
	UnitSpriteAttachmentData draggedSprite;
	UnitSubUnitAttachmentData draggedSubUnit;
	StoredSprite draggedLibrarySprite;
	StoredUnit draggedLibraryUnit;
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

		VBoxContainer inspector = new VBoxContainer();
		inspector.AddThemeConstantOverride("separation", 8);
		bottomPanel.AddChild(inspector);

		inspector.AddChild(CreatePanelTitle("Inspector"));

		Label info = new Label();
		info.Text = "Select a component";
		mAccess.styleManager.applyTextStyle(info, "muted");
		inspector.AddChild(info);
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

	void OnUnitChanged(object sender, EventArgs args)
	{
		RefreshComponentsList();
		RefreshPreview();
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
}
