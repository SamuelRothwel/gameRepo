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
			DefinitionKind = "unit",
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

	public void CreateNewComponent()
	{
		activeUnit = new UnitDefinition
		{
			Name = "New Component",
			DefinitionKind = "component",
			CommandType = "",
			Radius = 0,
			DetectionRadius = 0,
			MaxHP = 1
		};
		currentUnitVersion = 0;
		savedUnitVersion = 0;
		RegisterActiveUnit();
		unitChanged?.Invoke(this, EventArgs.Empty);
	}

	public bool IsStoredComponent(StoredUnit storedUnit)
	{
		StoredUnitTrait kindTrait = storedUnit?.Traits.FirstOrDefault(trait => trait.Key == "__definitionKind");
		if (kindTrait != null)
		{
			try
			{
				return JsonSerializer.Deserialize<string>(kindTrait.ValueJson) == "component";
			}
			catch
			{
			}
		}
		return storedUnit?.Name == "marineGun";
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
		activeUnit = NormalizeDefinition(mAccess.entityFrameworkManager.ToUnitDefinition(storedUnit, behaviorFactory));
		if (activeUnit.DefinitionKind != "component")
		{
			mAccess.unitManager?.RegisterUnitDefinition(activeUnit);
		}
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
		savedUnitVersion = mAccess.entityFrameworkManager.SaveUnit(activeUnit, mAccess.unitManager.activeGameId);
		currentUnitVersion = savedUnitVersion;
		if (activeUnit.DefinitionKind != "component")
		{
			mAccess.unitManager?.RegisterUnitDefinition(activeUnit);
		}
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

		List<StoredUnit> units = mAccess.entityFrameworkManager.GetUnits(mAccess.unitManager.activeGameId)
			.Where(unit => unit.Name != "marineGun")
			.ToList();
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
			Func<IEnumerable<IUnitBehavior>> behaviorFactory = mAccess.unitManager == null
				? () => Array.Empty<IUnitBehavior>()
				: mAccess.unitManager.CreateKnownBehaviors;
			UnitDefinition definition = mAccess.entityFrameworkManager.ToUnitDefinition(storedUnit, behaviorFactory);
			UnitSpriteAttachmentData componentSprite = FindFirstComponentSprite(definition.ComponentAttachments);
			return componentSprite == null ? null : CreateAttachmentPreview(componentSprite);
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

	UnitSpriteAttachmentData FindFirstComponentSprite(IEnumerable<UnitComponentAttachmentData> components)
	{
		foreach (UnitComponentAttachmentData component in components.OrderBy(component => component.Order))
		{
			UnitSpriteAttachmentData sprite = component.SpriteAttachments
				.OrderBy(attachment => attachment.Order)
				.FirstOrDefault();
			if (sprite != null)
			{
				return sprite;
			}

			sprite = FindFirstComponentSprite(component.ChildComponents);
			if (sprite != null)
			{
				return sprite;
			}
		}
		return null;
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
			return NormalizeDefinition(registeredDefinition);
		}

		Func<IEnumerable<IUnitBehavior>> behaviorFactory = mAccess.unitManager == null
			? () => Array.Empty<IUnitBehavior>()
			: mAccess.unitManager.CreateKnownBehaviors;
		StoredUnit storedUnit = mAccess.entityFrameworkManager
			.GetUnits(mAccess.unitManager.activeGameId)
			.FirstOrDefault(unit => unit.Id == unitId);
		return storedUnit == null
			? null
			: NormalizeDefinition(mAccess.entityFrameworkManager.ToUnitDefinition(storedUnit, behaviorFactory));
	}

	UnitDefinition NormalizeDefinition(UnitDefinition definition)
	{
		return mAccess.unitManager == null
			? definition
			: mAccess.unitManager.NormalizeUnitDefinition(definition, ResolveDefinitionWithoutNormalization);
	}

	UnitDefinition ResolveDefinitionWithoutNormalization(Guid unitId)
	{
		UnitDefinition registeredDefinition = mAccess.unitManager?.unitDefinitions.Values
			.FirstOrDefault(definition => definition.Id == unitId);
		if (registeredDefinition != null)
		{
			return registeredDefinition;
		}

		Func<IEnumerable<IUnitBehavior>> behaviorFactory = mAccess.unitManager == null
			? () => Array.Empty<IUnitBehavior>()
			: mAccess.unitManager.CreateKnownBehaviors;
		StoredUnit storedUnit = mAccess.entityFrameworkManager
			.GetUnits(mAccess.unitManager.activeGameId)
			.FirstOrDefault(unit => unit.Id == unitId);
		return storedUnit == null
			? null
			: mAccess.entityFrameworkManager.ToUnitDefinition(storedUnit, behaviorFactory);
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

	public void AddComponentAttachment(StoredUnit storedComponent, Vector2 position)
	{
		if (activeUnit == null || storedComponent == null || storedComponent.Id == activeUnit.Id)
		{
			return;
		}

		Func<IEnumerable<IUnitBehavior>> behaviorFactory = mAccess.unitManager == null
			? () => Array.Empty<IUnitBehavior>()
			: mAccess.unitManager.CreateKnownBehaviors;
		UnitDefinition definition = mAccess.entityFrameworkManager.ToUnitDefinition(storedComponent, behaviorFactory);
		activeUnit.ComponentAttachments.Add(ToComponentAttachment(definition, position));
		MarkActiveUnitChanged();
	}

	UnitComponentAttachmentData ToComponentAttachment(UnitDefinition definition, Vector2 position)
	{
		UnitComponentAttachmentData component = new UnitComponentAttachmentData
		{
			Name = definition.Name,
			TypeName = definition.DescriptiveTraits.TryGetValue("componentType", out string typeName) ? typeName : "",
			Position = position,
			Order = activeUnit.ComponentAttachments.Count,
			SpriteAttachments = definition.SpriteAttachments.Select(CloneSpriteAttachment).ToList(),
			ChildComponents = definition.ComponentAttachments.Select(CloneComponentAttachment).ToList()
		};
		foreach (KeyValuePair<string, float> trait in definition.NumericalTraits)
		{
			component.Traits.Add(new UnitDataTrait
			{
				Key = trait.Key,
				ValueType = "number",
				ValueJson = JsonSerializer.Serialize(trait.Value)
			});
		}
		foreach (KeyValuePair<string, string> trait in definition.DescriptiveTraits)
		{
			component.Traits.Add(new UnitDataTrait
			{
				Key = trait.Key,
				ValueType = "text",
				ValueJson = JsonSerializer.Serialize(trait.Value)
			});
		}
		return component;
	}

	UnitComponentAttachmentData CloneComponentAttachment(UnitComponentAttachmentData source)
	{
		return new UnitComponentAttachmentData
		{
			Name = source.Name,
			TypeName = source.TypeName,
			Position = source.Position,
			Rotation = source.Rotation,
			Order = source.Order,
			Traits = source.Traits.Select(CloneTrait).ToList(),
			SpriteAttachments = source.SpriteAttachments.Select(CloneSpriteAttachment).ToList(),
			ChildComponents = source.ChildComponents.Select(CloneComponentAttachment).ToList()
		};
	}

	UnitSpriteAttachmentData CloneSpriteAttachment(UnitSpriteAttachmentData source)
	{
		return new UnitSpriteAttachmentData
		{
			StoredSpriteId = source.StoredSpriteId,
			Name = source.Name,
			Position = source.Position,
			Rotation = source.Rotation,
			Scale = source.Scale,
			Order = source.Order,
			SpriteSetKey = source.SpriteSetKey,
			Traits = source.Traits.Select(CloneTrait).ToList()
		};
	}

	UnitDataTrait CloneTrait(UnitDataTrait source)
	{
		return new UnitDataTrait
		{
			Key = source.Key,
			ValueType = source.ValueType,
			ValueJson = source.ValueJson
		};
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
