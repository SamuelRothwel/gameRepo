using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public partial class UnitCreatorManagement : managerNode
{
	public UnitDefinition activeUnit;
	public Guid? selectedSpriteId;
	public int currentUnitVersion;
	public int savedUnitVersion;
	public event EventHandler unitChanged;

	public override void setup()
	{
		CreateNewUnit();
	}

	public void CreateNewUnit()
	{
		activeUnit = new UnitDefinition
		{
			Name = "New Unit",
			CommandType = "commandable",
			Radius = 30,
			DetectionRadius = 150,
			MaxHP = 50
		};
		currentUnitVersion = 0;
		savedUnitVersion = 0;
		RegisterActiveUnit();
		unitChanged?.Invoke(this, EventArgs.Empty);
	}

	public void OpenSpritesPanel()
	{
		mAccess.windowManager.openWindow("Saved Sprites", CreateSpritesPanel());
	}

	public void OpenUnitsPanel()
	{
		mAccess.windowManager.openWindow("Saved Units", CreateUnitsPanel());
	}

	public void LoadStoredUnit(StoredUnit storedUnit)
	{
		Func<IEnumerable<IUnitBehavior>> behaviorFactory = mAccess.unitManager == null
			? () => Array.Empty<IUnitBehavior>()
			: mAccess.unitManager.CreateKnownBehaviors;
		activeUnit = mAccess.entityFrameworkManager.ToUnitDefinition(storedUnit, behaviorFactory);
		mAccess.unitManager?.RegisterUnitDefinition(activeUnit);
		currentUnitVersion = storedUnit.Version;
		savedUnitVersion = storedUnit.Version;
		RegisterActiveUnit();
		unitChanged?.Invoke(this, EventArgs.Empty);
	}

	public int SaveActiveUnit()
	{
		if (activeUnit == null || mAccess.entityFrameworkManager == null)
		{
			return currentUnitVersion;
		}

		activeUnit.Version = currentUnitVersion;
		savedUnitVersion = mAccess.entityFrameworkManager.SaveUnit(activeUnit);
		currentUnitVersion = savedUnitVersion;
		mAccess.unitManager?.RegisterUnitDefinition(activeUnit);
		RegisterActiveUnit();
		return savedUnitVersion;
	}

	public Texture2D CreateAttachmentPreview(UnitSpriteAttachmentData attachment)
	{
		if (attachment.StoredSpriteId != null)
		{
			StoredSprite storedSprite = mAccess.entityFrameworkManager
				.GetSprites()
				.FirstOrDefault(sprite => sprite.Id == attachment.StoredSpriteId);
			if (storedSprite != null)
			{
				return mAccess.spriteCreatorManager.CreateStoredSpritePreview(storedSprite);
			}
		}

		return string.IsNullOrEmpty(attachment.SpriteSetKey)
			? null
			: CreateSpriteSetPreview(attachment.SpriteSetKey);
	}

	Control CreateSpritesPanel()
	{
		ScrollContainer scroll = new ScrollContainer();
		scroll.CustomMinimumSize = new Vector2(320, 420);

		VBoxContainer savedSpritesList = new VBoxContainer();
		savedSpritesList.CustomMinimumSize = new Vector2(300, 0);
		scroll.AddChild(savedSpritesList);

		List<StoredSprite> sprites = mAccess.entityFrameworkManager.GetSprites();
		if (sprites.Count == 0)
		{
			Label emptyLabel = new Label();
			emptyLabel.Text = "No saved sprites";
			mAccess.styleManager.applyTextStyle(emptyLabel, "muted");
			savedSpritesList.AddChild(emptyLabel);
			return scroll;
		}

		foreach (StoredSprite storedSprite in sprites)
		{
			savedSpritesList.AddChild(CreateSavedSpriteRow(storedSprite));
		}

		return scroll;
	}

	Control CreateUnitsPanel()
	{
		ScrollContainer scroll = new ScrollContainer();
		scroll.CustomMinimumSize = new Vector2(320, 420);

		VBoxContainer savedUnitsList = new VBoxContainer();
		savedUnitsList.CustomMinimumSize = new Vector2(300, 0);
		scroll.AddChild(savedUnitsList);

		List<StoredUnit> units = mAccess.entityFrameworkManager.GetUnits();
		if (units.Count == 0)
		{
			Label emptyLabel = new Label();
			emptyLabel.Text = "No saved units";
			mAccess.styleManager.applyTextStyle(emptyLabel, "muted");
			savedUnitsList.AddChild(emptyLabel);
			return scroll;
		}

		foreach (StoredUnit storedUnit in units)
		{
			savedUnitsList.AddChild(CreateSavedUnitRow(storedUnit));
		}

		return scroll;
	}

	Control CreateSavedUnitRow(StoredUnit storedUnit)
	{
		Button rowButton = new Button();
		rowButton.Text = "";
		mAccess.styleManager.applyButtonStyle(rowButton, "secondary");
		rowButton.CustomMinimumSize = new Vector2(130, 58);
		rowButton.Pressed += () =>
		{
			LoadStoredUnit(storedUnit);
			mAccess.windowManager.closeWindow("Saved Units", false);
		};

		HBoxContainer row = new HBoxContainer();
		row.CustomMinimumSize = new Vector2(130, 58);
		row.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		row.OffsetLeft = 6f;
		row.OffsetRight = -6f;
		row.MouseFilter = Control.MouseFilterEnum.Ignore;
		rowButton.AddChild(row);

		TextureRect preview = new TextureRect();
		preview.CustomMinimumSize = new Vector2(54, 54);
		preview.Texture = CreateUnitPreview(storedUnit);
		preview.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		preview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		preview.MouseFilter = Control.MouseFilterEnum.Ignore;
		row.AddChild(preview);

		Label nameLabel = new Label();
		nameLabel.Text = storedUnit.Name;
		nameLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		nameLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
		mAccess.styleManager.applyTextStyle(nameLabel, "default");
		row.AddChild(nameLabel);

		return rowButton;
	}

	public Texture2D CreateUnitPreview(StoredUnit storedUnit)
	{
		StoredUnitSpriteAttachment firstSprite = storedUnit.SpriteAttachments
			.OrderBy(attachment => attachment.Order)
			.FirstOrDefault(attachment => attachment.StoredSpriteId != null || !string.IsNullOrEmpty(attachment.SpriteSetKey));
		if (firstSprite == null)
		{
			return null;
		}

		if (firstSprite.StoredSpriteId != null)
		{
			StoredSprite storedSprite = mAccess.entityFrameworkManager
				.GetSprites()
				.FirstOrDefault(sprite => sprite.Id == firstSprite.StoredSpriteId);
			if (storedSprite != null)
			{
				return mAccess.spriteCreatorManager.CreateStoredSpritePreview(storedSprite);
			}
		}

		return string.IsNullOrEmpty(firstSprite.SpriteSetKey)
			? null
			: CreateSpriteSetPreview(firstSprite.SpriteSetKey);
	}

	public Texture2D CreateSpriteSetPreview(string spriteSetKey)
	{
		if (string.IsNullOrEmpty(spriteSetKey) || !mAccess.spriteManager.spriteMaps.ContainsKey(spriteSetKey))
		{
			return null;
		}

		Dictionary<string, Image[]> entityImages = mAccess.spriteManager.getEntityImages(spriteSetKey);
		Image[] frames = entityImages.ContainsKey("idle")
			? entityImages["idle"]
			: entityImages.Values.FirstOrDefault();
		if (frames == null || frames.Length == 0)
		{
			return null;
		}

		return ImageTexture.CreateFromImage(frames[0]);
	}

	public UnitDefinition GetUnitDefinition(Guid unitId)
	{
		UnitDefinition registeredDefinition = mAccess.unitManager?.unitDefinitions.Values
			.FirstOrDefault(definition => definition.Id == unitId);
		if (registeredDefinition != null)
		{
			return EnsureFallbackComponents(registeredDefinition);
		}

		Func<IEnumerable<IUnitBehavior>> behaviorFactory = mAccess.unitManager == null
			? () => Array.Empty<IUnitBehavior>()
			: mAccess.unitManager.CreateKnownBehaviors;
		StoredUnit storedUnit = mAccess.entityFrameworkManager
			.GetUnits()
			.FirstOrDefault(unit => unit.Id == unitId);
		return storedUnit == null
			? null
			: EnsureFallbackComponents(mAccess.entityFrameworkManager.ToUnitDefinition(storedUnit, behaviorFactory));
	}

	UnitDefinition EnsureFallbackComponents(UnitDefinition definition)
	{
		if (definition.Name == "marineGun" && definition.SpriteAttachments.Count == 0)
		{
			definition.SpriteAttachments.Add(new UnitSpriteAttachmentData
			{
				Name = "barrel",
				SpriteSetKey = "GunBarrel",
				Order = 0
			});
		}

		return definition;
	}

	Control CreateSavedSpriteRow(StoredSprite storedSprite)
	{
		Button rowButton = new Button();
		rowButton.Text = "";
		mAccess.styleManager.applyButtonStyle(rowButton, "secondary");
		rowButton.CustomMinimumSize = new Vector2(130, 58);
		rowButton.Pressed += () =>
		{
			AddSpriteAttachment(storedSprite);
			mAccess.windowManager.closeWindow("Saved Sprites", false);
		};

		HBoxContainer row = new HBoxContainer();
		row.CustomMinimumSize = new Vector2(130, 58);
		row.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		row.OffsetLeft = 6f;
		row.OffsetRight = -6f;
		row.MouseFilter = Control.MouseFilterEnum.Ignore;
		rowButton.AddChild(row);

		TextureRect preview = new TextureRect();
		preview.CustomMinimumSize = new Vector2(54, 54);
		preview.Texture = mAccess.spriteCreatorManager.CreateStoredSpritePreview(storedSprite);
		preview.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		preview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		preview.MouseFilter = Control.MouseFilterEnum.Ignore;
		row.AddChild(preview);

		Label nameLabel = new Label();
		nameLabel.Text = storedSprite.Name;
		nameLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		nameLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
		mAccess.styleManager.applyTextStyle(nameLabel, "default");
		row.AddChild(nameLabel);

		return rowButton;
	}

	public void AddSpriteAttachment(StoredSprite storedSprite)
	{
		AddSpriteAttachment(storedSprite, Vector2.Zero);
	}

	public void AddSpriteAttachment(StoredSprite storedSprite, Vector2 position)
	{
		selectedSpriteId = storedSprite.Id;
		UnitSpriteAttachmentData attachment = new UnitSpriteAttachmentData
		{
			StoredSpriteId = storedSprite.Id,
			Name = storedSprite.Name,
			Order = activeUnit.SpriteAttachments.Count,
			Scale = Vector2.One,
			Traits = new List<UnitDataTrait>
			{
				new UnitDataTrait { Key = "hitboxEnabled", ValueType = "bool", ValueJson = "true" }
			}
		};
		SetAttachmentPosition(attachment, position);
		activeUnit.SpriteAttachments.Add(attachment);
		MarkActiveUnitChanged();
	}

	public void AddSubUnitAttachment(StoredUnit storedUnit, Vector2 position)
	{
		if (activeUnit == null || storedUnit.Id == activeUnit.Id)
		{
			return;
		}

		activeUnit.SubUnitAttachments.Add(new UnitSubUnitAttachmentData
		{
			ChildUnitId = storedUnit.Id,
			Name = storedUnit.Name,
			Position = position,
			Order = activeUnit.SubUnitAttachments.Count,
			ParametersJson = "{}",
			Traits = new List<UnitDataTrait>
			{
				new UnitDataTrait { Key = "positionX", ValueType = "number", ValueJson = JsonSerializer.Serialize(position.X) },
				new UnitDataTrait { Key = "positionY", ValueType = "number", ValueJson = JsonSerializer.Serialize(position.Y) }
			}
		});
		MarkActiveUnitChanged();
	}

	public void MoveSpriteAttachment(UnitSpriteAttachmentData attachment, Vector2 position)
	{
		SetAttachmentPosition(attachment, position);
		MarkActiveUnitChanged();
	}

	public void MoveSubUnitAttachment(UnitSubUnitAttachmentData attachment, Vector2 position)
	{
		SetAttachmentPosition(attachment, position);
		MarkActiveUnitChanged();
	}

	void SetAttachmentPosition(UnitSpriteAttachmentData attachment, Vector2 position)
	{
		attachment.Position = position;
		SetTrait(attachment.Traits, "positionX", "number", JsonSerializer.Serialize(position.X));
		SetTrait(attachment.Traits, "positionY", "number", JsonSerializer.Serialize(position.Y));
	}

	void SetAttachmentPosition(UnitSubUnitAttachmentData attachment, Vector2 position)
	{
		attachment.Position = position;
		SetTrait(attachment.Traits, "positionX", "number", JsonSerializer.Serialize(position.X));
		SetTrait(attachment.Traits, "positionY", "number", JsonSerializer.Serialize(position.Y));
	}

	void SetTrait(List<UnitDataTrait> traits, string key, string valueType, string valueJson)
	{
		UnitDataTrait trait = traits.FirstOrDefault(trait => trait.Key == key);
		if (trait == null)
		{
			traits.Add(new UnitDataTrait
			{
				Key = key,
				ValueType = valueType,
				ValueJson = valueJson
			});
			return;
		}

		trait.ValueType = valueType;
		trait.ValueJson = valueJson;
	}

	void MarkActiveUnitChanged()
	{
		currentUnitVersion++;
		RegisterActiveUnit();
		unitChanged?.Invoke(this, EventArgs.Empty);
	}

	void RegisterActiveUnit()
	{
		if (activeUnit == null || mAccess.entityFrameworkManager == null)
		{
			return;
		}

		mAccess.entityFrameworkManager.RegisterUnsavedObject
		(
			activeUnit.Id,
			activeUnit.Name,
			savedUnitVersion,
			() => currentUnitVersion,
			() => SaveActiveUnit(),
			() => mAccess.entityFrameworkManager.UnregisterUnsavedObject(activeUnit.Id)
		);
	}
}
