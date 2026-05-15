using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class StoredSprite
{
	public Guid Id { get; set; }
	public string Name { get; set; } = "";
	public int Version { get; set; }
	public List<StoredSpriteLayer> Layers { get; set; } = new List<StoredSpriteLayer>();
}

public class StoredSpriteLayer
{
	public Guid Id { get; set; }
	public Guid StoredSpriteId { get; set; }
	public StoredSprite StoredSprite { get; set; }
	public string Name { get; set; } = "";
	public int Order { get; set; }
	public string Color { get; set; } = "";
	public string CoordinatesJson { get; set; } = "";
}

public class StoredUnit
{
	public Guid Id { get; set; }
	public string Name { get; set; } = "";
	public int Version { get; set; }
	public string CommandType { get; set; } = "";
	public float Radius { get; set; }
	public float DetectionRadius { get; set; }
	public float MaxHP { get; set; }
	public List<StoredUnitTrait> Traits { get; set; } = new();
	public List<StoredUnitBehavior> Behaviors { get; set; } = new();
	public List<StoredUnitAbility> Abilities { get; set; } = new();
	public List<StoredUnitSpriteAttachment> SpriteAttachments { get; set; } = new();
	public List<StoredUnitSubUnitAttachment> SubUnitAttachments { get; set; } = new();
}

public class StoredUnitTrait
{
	public Guid Id { get; set; }
	public Guid StoredUnitId { get; set; }
	public StoredUnit StoredUnit { get; set; }
	public string Key { get; set; } = "";
	public string ValueType { get; set; } = "";
	public string ValueJson { get; set; } = "";
}

public class StoredUnitBehavior
{
	public Guid Id { get; set; }
	public Guid StoredUnitId { get; set; }
	public StoredUnit StoredUnit { get; set; }
	public string CommandName { get; set; } = "";
	public string BehaviorName { get; set; } = "";
	public int Order { get; set; }
	public string ParametersJson { get; set; } = "";
}

public class StoredUnitAbility
{
	public Guid Id { get; set; }
	public Guid StoredUnitId { get; set; }
	public StoredUnit StoredUnit { get; set; }
	public string AbilityName { get; set; } = "";
	public string BehaviorName { get; set; } = "";
	public string ParametersJson { get; set; } = "";
}

public class StoredUnitSpriteAttachment
{
	public Guid Id { get; set; }
	public Guid StoredUnitId { get; set; }
	public StoredUnit StoredUnit { get; set; }
	public Guid? StoredSpriteId { get; set; }
	public StoredSprite StoredSprite { get; set; }
	public string Name { get; set; } = "";
	public float PositionX { get; set; }
	public float PositionY { get; set; }
	public float Rotation { get; set; }
	public float ScaleX { get; set; } = 1;
	public float ScaleY { get; set; } = 1;
	public int Order { get; set; }
	public string SpriteSetKey { get; set; } = "";
	public List<StoredUnitSpriteTrait> Traits { get; set; } = new();
}

public class StoredUnitSpriteTrait
{
	public Guid Id { get; set; }
	public Guid StoredUnitSpriteAttachmentId { get; set; }
	public StoredUnitSpriteAttachment StoredUnitSpriteAttachment { get; set; }
	public string Key { get; set; } = "";
	public string ValueType { get; set; } = "";
	public string ValueJson { get; set; } = "";
}

public class StoredUnitSubUnitAttachment
{
	public Guid Id { get; set; }
	public Guid ParentUnitId { get; set; }
	public StoredUnit ParentUnit { get; set; }
	public Guid ChildUnitId { get; set; }
	public StoredUnit ChildUnit { get; set; }
	public string Name { get; set; } = "";
	public float PositionX { get; set; }
	public float PositionY { get; set; }
	public float Rotation { get; set; }
	public int Order { get; set; }
	public string ParametersJson { get; set; } = "";
	public List<StoredUnitSubUnitTrait> Traits { get; set; } = new();
}

public class StoredUnitSubUnitTrait
{
	public Guid Id { get; set; }
	public Guid StoredUnitSubUnitAttachmentId { get; set; }
	public StoredUnitSubUnitAttachment StoredUnitSubUnitAttachment { get; set; }
	public string Key { get; set; } = "";
	public string ValueType { get; set; } = "";
	public string ValueJson { get; set; } = "";
}

public class GameDbContext : DbContext
{
	public DbSet<StoredSprite> Sprites { get; set; }
	public DbSet<StoredSpriteLayer> SpriteLayers { get; set; }
	public DbSet<StoredUnit> Units { get; set; }
	public DbSet<StoredUnitTrait> UnitTraits { get; set; }
	public DbSet<StoredUnitBehavior> UnitBehaviors { get; set; }
	public DbSet<StoredUnitAbility> UnitAbilities { get; set; }
	public DbSet<StoredUnitSpriteAttachment> UnitSpriteAttachments { get; set; }
	public DbSet<StoredUnitSpriteTrait> UnitSpriteTraits { get; set; }
	public DbSet<StoredUnitSubUnitAttachment> UnitSubUnitAttachments { get; set; }
	public DbSet<StoredUnitSubUnitTrait> UnitSubUnitTraits { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder options)
	{
		string databasePath = ProjectSettings.GlobalizePath("user://game_data.db");
		options.UseSqlite($"Data Source={databasePath}");
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<StoredSprite>()
			.HasKey(sprite => sprite.Id);

		modelBuilder.Entity<StoredSpriteLayer>()
			.HasKey(layer => layer.Id);

		modelBuilder.Entity<StoredSprite>()
			.HasMany(sprite => sprite.Layers)
			.WithOne(layer => layer.StoredSprite)
			.HasForeignKey(layer => layer.StoredSpriteId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<StoredUnit>()
			.HasKey(unit => unit.Id);

		modelBuilder.Entity<StoredUnitTrait>()
			.HasKey(trait => trait.Id);

		modelBuilder.Entity<StoredUnitBehavior>()
			.HasKey(behavior => behavior.Id);

		modelBuilder.Entity<StoredUnitAbility>()
			.HasKey(ability => ability.Id);

		modelBuilder.Entity<StoredUnitSpriteAttachment>()
			.HasKey(attachment => attachment.Id);

		modelBuilder.Entity<StoredUnitSpriteTrait>()
			.HasKey(trait => trait.Id);

		modelBuilder.Entity<StoredUnitSubUnitAttachment>()
			.HasKey(attachment => attachment.Id);

		modelBuilder.Entity<StoredUnitSubUnitTrait>()
			.HasKey(trait => trait.Id);

		modelBuilder.Entity<StoredUnit>()
			.HasMany(unit => unit.Traits)
			.WithOne(trait => trait.StoredUnit)
			.HasForeignKey(trait => trait.StoredUnitId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<StoredUnit>()
			.HasMany(unit => unit.Behaviors)
			.WithOne(behavior => behavior.StoredUnit)
			.HasForeignKey(behavior => behavior.StoredUnitId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<StoredUnit>()
			.HasMany(unit => unit.Abilities)
			.WithOne(ability => ability.StoredUnit)
			.HasForeignKey(ability => ability.StoredUnitId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<StoredUnit>()
			.HasMany(unit => unit.SpriteAttachments)
			.WithOne(attachment => attachment.StoredUnit)
			.HasForeignKey(attachment => attachment.StoredUnitId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<StoredUnitSpriteAttachment>()
			.HasOne(attachment => attachment.StoredSprite)
			.WithMany()
			.HasForeignKey(attachment => attachment.StoredSpriteId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<StoredUnitSpriteAttachment>()
			.HasMany(attachment => attachment.Traits)
			.WithOne(trait => trait.StoredUnitSpriteAttachment)
			.HasForeignKey(trait => trait.StoredUnitSpriteAttachmentId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<StoredUnit>()
			.HasMany(unit => unit.SubUnitAttachments)
			.WithOne(attachment => attachment.ParentUnit)
			.HasForeignKey(attachment => attachment.ParentUnitId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<StoredUnitSubUnitAttachment>()
			.HasOne(attachment => attachment.ChildUnit)
			.WithMany()
			.HasForeignKey(attachment => attachment.ChildUnitId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<StoredUnitSubUnitAttachment>()
			.HasMany(attachment => attachment.Traits)
			.WithOne(trait => trait.StoredUnitSubUnitAttachment)
			.HasForeignKey(trait => trait.StoredUnitSubUnitAttachmentId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}

public partial class EntityFrameworkManagement : managerNode
{
	public Dictionary<Guid, UnsavedObjectRegistration> unsavedObjects;
	public override void setup()
	{
		unsavedObjects = new Dictionary<Guid, UnsavedObjectRegistration>();
		SetupDatabase();
	}

	public void SetupDatabase()
	{
		using GameDbContext context = new GameDbContext();
		context.Database.EnsureCreated();
		EnsureSpriteVersionColumn(context);
		EnsureUnitTables(context);
	}

	void EnsureSpriteVersionColumn(GameDbContext context)
	{
		try
		{
			context.Database.ExecuteSqlRaw("ALTER TABLE Sprites ADD COLUMN Version INTEGER NOT NULL DEFAULT 0");
		}
		catch
		{
		}
	}

	void EnsureUnitTables(GameDbContext context)
	{
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS Units (
				Id TEXT NOT NULL CONSTRAINT PK_Units PRIMARY KEY,
				Name TEXT NOT NULL,
				Version INTEGER NOT NULL,
				CommandType TEXT NOT NULL,
				Radius REAL NOT NULL,
				DetectionRadius REAL NOT NULL,
				MaxHP REAL NOT NULL
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS UnitTraits (
				Id TEXT NOT NULL CONSTRAINT PK_UnitTraits PRIMARY KEY,
				StoredUnitId TEXT NOT NULL,
				Key TEXT NOT NULL,
				ValueType TEXT NOT NULL,
				ValueJson TEXT NOT NULL,
				CONSTRAINT FK_UnitTraits_Units_StoredUnitId FOREIGN KEY (StoredUnitId) REFERENCES Units (Id) ON DELETE CASCADE
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS UnitBehaviors (
				Id TEXT NOT NULL CONSTRAINT PK_UnitBehaviors PRIMARY KEY,
				StoredUnitId TEXT NOT NULL,
				CommandName TEXT NOT NULL,
				BehaviorName TEXT NOT NULL,
				"Order" INTEGER NOT NULL,
				ParametersJson TEXT NOT NULL,
				CONSTRAINT FK_UnitBehaviors_Units_StoredUnitId FOREIGN KEY (StoredUnitId) REFERENCES Units (Id) ON DELETE CASCADE
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS UnitAbilities (
				Id TEXT NOT NULL CONSTRAINT PK_UnitAbilities PRIMARY KEY,
				StoredUnitId TEXT NOT NULL,
				AbilityName TEXT NOT NULL,
				BehaviorName TEXT NOT NULL,
				ParametersJson TEXT NOT NULL,
				CONSTRAINT FK_UnitAbilities_Units_StoredUnitId FOREIGN KEY (StoredUnitId) REFERENCES Units (Id) ON DELETE CASCADE
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS UnitSpriteAttachments (
				Id TEXT NOT NULL CONSTRAINT PK_UnitSpriteAttachments PRIMARY KEY,
				StoredUnitId TEXT NOT NULL,
				StoredSpriteId TEXT NULL,
				Name TEXT NOT NULL,
				PositionX REAL NOT NULL,
				PositionY REAL NOT NULL,
				Rotation REAL NOT NULL,
				ScaleX REAL NOT NULL,
				ScaleY REAL NOT NULL,
				"Order" INTEGER NOT NULL,
				SpriteSetKey TEXT NOT NULL,
				CONSTRAINT FK_UnitSpriteAttachments_Units_StoredUnitId FOREIGN KEY (StoredUnitId) REFERENCES Units (Id) ON DELETE CASCADE,
				CONSTRAINT FK_UnitSpriteAttachments_Sprites_StoredSpriteId FOREIGN KEY (StoredSpriteId) REFERENCES Sprites (Id) ON DELETE SET NULL
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS UnitSpriteTraits (
				Id TEXT NOT NULL CONSTRAINT PK_UnitSpriteTraits PRIMARY KEY,
				StoredUnitSpriteAttachmentId TEXT NOT NULL,
				Key TEXT NOT NULL,
				ValueType TEXT NOT NULL,
				ValueJson TEXT NOT NULL,
				CONSTRAINT FK_UnitSpriteTraits_UnitSpriteAttachments_StoredUnitSpriteAttachmentId FOREIGN KEY (StoredUnitSpriteAttachmentId) REFERENCES UnitSpriteAttachments (Id) ON DELETE CASCADE
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS UnitSubUnitAttachments (
				Id TEXT NOT NULL CONSTRAINT PK_UnitSubUnitAttachments PRIMARY KEY,
				ParentUnitId TEXT NOT NULL,
				ChildUnitId TEXT NOT NULL,
				Name TEXT NOT NULL,
				PositionX REAL NOT NULL,
				PositionY REAL NOT NULL,
				Rotation REAL NOT NULL,
				"Order" INTEGER NOT NULL,
				ParametersJson TEXT NOT NULL,
				CONSTRAINT FK_UnitSubUnitAttachments_Units_ParentUnitId FOREIGN KEY (ParentUnitId) REFERENCES Units (Id) ON DELETE CASCADE,
				CONSTRAINT FK_UnitSubUnitAttachments_Units_ChildUnitId FOREIGN KEY (ChildUnitId) REFERENCES Units (Id) ON DELETE RESTRICT
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS UnitSubUnitTraits (
				Id TEXT NOT NULL CONSTRAINT PK_UnitSubUnitTraits PRIMARY KEY,
				StoredUnitSubUnitAttachmentId TEXT NOT NULL,
				Key TEXT NOT NULL,
				ValueType TEXT NOT NULL,
				ValueJson TEXT NOT NULL,
				CONSTRAINT FK_UnitSubUnitTraits_UnitSubUnitAttachments_StoredUnitSubUnitAttachmentId FOREIGN KEY (StoredUnitSubUnitAttachmentId) REFERENCES UnitSubUnitAttachments (Id) ON DELETE CASCADE
			)
			""");
	}

	public int SaveSprite(Guid id, string name, int version, List<StoredSpriteLayer> layers)
	{
		using GameDbContext context = new GameDbContext();
		using var transaction = context.Database.BeginTransaction();
		StoredSprite sprite = context.Sprites.FirstOrDefault(sprite => sprite.Id == id);

		if (sprite == null)
		{
			sprite = new StoredSprite
			{
				Id = id,
				Name = name,
				Version = version,
			};
			context.Sprites.Add(sprite);
		}
		else
		{
			sprite.Name = name;
			sprite.Version = version;
			context.SpriteLayers
				.Where(layer => layer.StoredSpriteId == id)
				.ExecuteDelete();
		}

		foreach (StoredSpriteLayer layer in layers)
		{
			layer.Id = Guid.NewGuid();
			layer.StoredSpriteId = id;
			layer.StoredSprite = null;
		}

		context.SpriteLayers.AddRange(layers);
		context.SaveChanges();
		transaction.Commit();
		UpdateRegisteredSavedVersion(id, version);
		return version;
	}

	public List<StoredSprite> GetSprites()
	{
		using GameDbContext context = new GameDbContext();
		return context.Sprites
			.Include(sprite => sprite.Layers)
			.Select(sprite => new StoredSprite
			{
				Id = sprite.Id,
				Name = sprite.Name,
				Version = sprite.Version,
				Layers = sprite.Layers.OrderBy(layer => layer.Order).ToList()
			})
			.OrderBy(sprite => sprite.Name)
			.ToList();
	}

	public void EnsureUnitDefinition(UnitDefinition definition)
	{
		using GameDbContext context = new GameDbContext();
		StoredUnit unit = context.Units.FirstOrDefault(unit => unit.Name == definition.Name);
		if (unit == null)
		{
			SaveUnit(definition);
			return;
		}

		definition.Id = unit.Id;
		definition.Version = unit.Version;
	}

	public int SaveUnit(UnitDefinition definition)
	{
		using GameDbContext context = new GameDbContext();
		using var transaction = context.Database.BeginTransaction();
		StoredUnit unit = context.Units.FirstOrDefault(unit => unit.Id == definition.Id);

		if (unit == null)
		{
			unit = context.Units.FirstOrDefault(unit => unit.Name == definition.Name);
		}

		if (unit == null)
		{
			unit = new StoredUnit
			{
				Id = definition.Id == Guid.Empty ? Guid.NewGuid() : definition.Id
			};
			context.Units.Add(unit);
		}
		else
		{
			definition.Id = unit.Id;
			DeleteUnitChildren(context, unit.Id);
		}

		unit.Name = definition.Name;
		unit.Version = definition.Version;
		unit.CommandType = definition.CommandType;
		unit.Radius = definition.Radius;
		unit.DetectionRadius = definition.DetectionRadius;
		unit.MaxHP = definition.MaxHP;

		context.SaveChanges();
		context.UnitTraits.AddRange(CreateUnitTraits(unit.Id, definition));
		context.UnitBehaviors.AddRange(CreateUnitBehaviors(unit.Id, definition));
		context.UnitAbilities.AddRange(CreateUnitAbilities(unit.Id, definition));

		foreach (StoredUnitSpriteAttachment attachment in CreateUnitSpriteAttachments(unit.Id, definition))
		{
			context.UnitSpriteAttachments.Add(attachment);
			context.UnitSpriteTraits.AddRange(attachment.Traits);
			attachment.Traits = new List<StoredUnitSpriteTrait>();
		}

		foreach (StoredUnitSubUnitAttachment attachment in CreateUnitSubUnitAttachments(unit.Id, definition))
		{
			context.UnitSubUnitAttachments.Add(attachment);
			context.UnitSubUnitTraits.AddRange(attachment.Traits);
			attachment.Traits = new List<StoredUnitSubUnitTrait>();
		}

		context.SaveChanges();
		transaction.Commit();
		UpdateRegisteredSavedVersion(unit.Id, definition.Version);
		return definition.Version;
	}

	void DeleteUnitChildren(GameDbContext context, Guid unitId)
	{
		List<Guid> spriteAttachmentIds = context.UnitSpriteAttachments
			.Where(attachment => attachment.StoredUnitId == unitId)
			.Select(attachment => attachment.Id)
			.ToList();
		List<Guid> subUnitAttachmentIds = context.UnitSubUnitAttachments
			.Where(attachment => attachment.ParentUnitId == unitId)
			.Select(attachment => attachment.Id)
			.ToList();

		context.UnitSpriteTraits
			.Where(trait => spriteAttachmentIds.Contains(trait.StoredUnitSpriteAttachmentId))
			.ExecuteDelete();
		context.UnitSubUnitTraits
			.Where(trait => subUnitAttachmentIds.Contains(trait.StoredUnitSubUnitAttachmentId))
			.ExecuteDelete();
		context.UnitTraits.Where(trait => trait.StoredUnitId == unitId).ExecuteDelete();
		context.UnitBehaviors.Where(behavior => behavior.StoredUnitId == unitId).ExecuteDelete();
		context.UnitAbilities.Where(ability => ability.StoredUnitId == unitId).ExecuteDelete();
		context.UnitSpriteAttachments.Where(attachment => attachment.StoredUnitId == unitId).ExecuteDelete();
		context.UnitSubUnitAttachments.Where(attachment => attachment.ParentUnitId == unitId).ExecuteDelete();
	}

	IEnumerable<StoredUnitTrait> CreateUnitTraits(Guid unitId, UnitDefinition definition)
	{
		foreach (KeyValuePair<string, float> trait in definition.NumericalTraits)
		{
			yield return new StoredUnitTrait
			{
				Id = Guid.NewGuid(),
				StoredUnitId = unitId,
				Key = trait.Key,
				ValueType = "number",
				ValueJson = JsonSerializer.Serialize(trait.Value)
			};
		}
		foreach (KeyValuePair<string, string> trait in definition.DescriptiveTraits)
		{
			yield return new StoredUnitTrait
			{
				Id = Guid.NewGuid(),
				StoredUnitId = unitId,
				Key = trait.Key,
				ValueType = "text",
				ValueJson = JsonSerializer.Serialize(trait.Value)
			};
		}
	}

	IEnumerable<StoredUnitBehavior> CreateUnitBehaviors(Guid unitId, UnitDefinition definition)
	{
		return definition.BehaviorProfile.GetBehaviorData()
			.Select(behavior => new StoredUnitBehavior
			{
				Id = Guid.NewGuid(),
				StoredUnitId = unitId,
				CommandName = behavior.CommandName,
				BehaviorName = behavior.BehaviorName,
				Order = behavior.Order,
				ParametersJson = behavior.ParametersJson
			});
	}

	IEnumerable<StoredUnitAbility> CreateUnitAbilities(Guid unitId, UnitDefinition definition)
	{
		return definition.Abilities.Select(ability => new StoredUnitAbility
		{
			Id = Guid.NewGuid(),
			StoredUnitId = unitId,
			AbilityName = ability.AbilityName,
			BehaviorName = ability.BehaviorName,
			ParametersJson = ability.ParametersJson
		});
	}

	IEnumerable<StoredUnitSpriteAttachment> CreateUnitSpriteAttachments(Guid unitId, UnitDefinition definition)
	{
		foreach (UnitSpriteAttachmentData attachment in definition.SpriteAttachments)
		{
			Guid attachmentId = attachment.Id == Guid.Empty ? Guid.NewGuid() : attachment.Id;
			yield return new StoredUnitSpriteAttachment
			{
				Id = attachmentId,
				StoredUnitId = unitId,
				StoredSpriteId = attachment.StoredSpriteId,
				Name = attachment.Name,
				PositionX = attachment.Position.X,
				PositionY = attachment.Position.Y,
				Rotation = attachment.Rotation,
				ScaleX = attachment.Scale.X,
				ScaleY = attachment.Scale.Y,
				Order = attachment.Order,
				SpriteSetKey = attachment.SpriteSetKey,
				Traits = attachment.Traits.Select(trait => CreateSpriteTrait(attachmentId, trait)).ToList()
			};
		}
	}

	StoredUnitSpriteTrait CreateSpriteTrait(Guid attachmentId, UnitDataTrait trait)
	{
		return new StoredUnitSpriteTrait
		{
			Id = Guid.NewGuid(),
			StoredUnitSpriteAttachmentId = attachmentId,
			Key = trait.Key,
			ValueType = trait.ValueType,
			ValueJson = trait.ValueJson
		};
	}

	IEnumerable<StoredUnitSubUnitAttachment> CreateUnitSubUnitAttachments(Guid unitId, UnitDefinition definition)
	{
		foreach (UnitSubUnitAttachmentData attachment in definition.SubUnitAttachments)
		{
			Guid attachmentId = attachment.Id == Guid.Empty ? Guid.NewGuid() : attachment.Id;
			yield return new StoredUnitSubUnitAttachment
			{
				Id = attachmentId,
				ParentUnitId = unitId,
				ChildUnitId = attachment.ChildUnitId,
				Name = attachment.Name,
				PositionX = attachment.Position.X,
				PositionY = attachment.Position.Y,
				Rotation = attachment.Rotation,
				Order = attachment.Order,
				ParametersJson = attachment.ParametersJson,
				Traits = attachment.Traits.Select(trait => CreateSubUnitTrait(attachmentId, trait)).ToList()
			};
		}
	}

	StoredUnitSubUnitTrait CreateSubUnitTrait(Guid attachmentId, UnitDataTrait trait)
	{
		return new StoredUnitSubUnitTrait
		{
			Id = Guid.NewGuid(),
			StoredUnitSubUnitAttachmentId = attachmentId,
			Key = trait.Key,
			ValueType = trait.ValueType,
			ValueJson = trait.ValueJson
		};
	}

	public List<UnitDefinition> GetUnitDefinitions(Func<IEnumerable<IUnitBehavior>> behaviorFactory)
	{
		return GetUnits().Select(unit => ToUnitDefinition(unit, behaviorFactory)).ToList();
	}

	public List<StoredUnit> GetUnits()
	{
		using GameDbContext context = new GameDbContext();
		List<StoredUnit> units = context.Units
			.Include(unit => unit.Traits)
			.Include(unit => unit.Behaviors)
			.Include(unit => unit.Abilities)
			.Include(unit => unit.SpriteAttachments)
				.ThenInclude(attachment => attachment.Traits)
			.Include(unit => unit.SubUnitAttachments)
				.ThenInclude(attachment => attachment.Traits)
			.AsSplitQuery()
			.ToList();

		return units
			.Select(unit => new StoredUnit
			{
				Id = unit.Id,
				Name = unit.Name,
				Version = unit.Version,
				CommandType = unit.CommandType,
				Radius = unit.Radius,
				DetectionRadius = unit.DetectionRadius,
				MaxHP = unit.MaxHP,
				Traits = unit.Traits.ToList(),
				Behaviors = unit.Behaviors.OrderBy(behavior => behavior.Order).ToList(),
				Abilities = unit.Abilities.ToList(),
				SpriteAttachments = unit.SpriteAttachments.OrderBy(attachment => attachment.Order).ToList(),
				SubUnitAttachments = unit.SubUnitAttachments.OrderBy(attachment => attachment.Order).ToList()
			})
			.OrderBy(unit => unit.Name)
			.ToList();
	}

	public UnitDefinition ToUnitDefinition(StoredUnit storedUnit, Func<IEnumerable<IUnitBehavior>> behaviorFactory)
	{
		UnitDefinition definition = new UnitDefinition
		{
			Id = storedUnit.Id,
			Name = storedUnit.Name,
			Version = storedUnit.Version,
			CommandType = storedUnit.CommandType,
			Radius = storedUnit.Radius,
			DetectionRadius = storedUnit.DetectionRadius,
			MaxHP = storedUnit.MaxHP,
			BehaviorFactory = behaviorFactory
		};

		foreach (StoredUnitTrait trait in storedUnit.Traits)
		{
			ApplyTrait(definition, trait.Key, trait.ValueType, trait.ValueJson);
		}

		foreach (IGrouping<string, StoredUnitBehavior> command in storedUnit.Behaviors
			.OrderBy(behavior => behavior.Order)
			.GroupBy(behavior => behavior.CommandName))
		{
			definition.BehaviorProfile.SetCommand(command.Key, command.Select(behavior => behavior.BehaviorName).ToArray());
		}

		definition.Abilities = storedUnit.Abilities.Select(ability => new UnitAbilityData
		{
			AbilityName = ability.AbilityName,
			BehaviorName = ability.BehaviorName,
			ParametersJson = ability.ParametersJson
		}).ToList();

		definition.SpriteAttachments = storedUnit.SpriteAttachments.Select(attachment =>
		{
			List<UnitDataTrait> traits = attachment.Traits.Select(ToDataTrait).ToList();
			return new UnitSpriteAttachmentData
			{
				Id = attachment.Id,
				StoredSpriteId = attachment.StoredSpriteId,
				Name = attachment.Name,
				Position = ReadPositionTraits(traits, new Vector2(attachment.PositionX, attachment.PositionY)),
				Rotation = attachment.Rotation,
				Scale = new Vector2(attachment.ScaleX, attachment.ScaleY),
				Order = attachment.Order,
				SpriteSetKey = attachment.SpriteSetKey,
				Traits = traits
			};
		}).ToList();

		definition.SubUnitAttachments = storedUnit.SubUnitAttachments.Select(attachment => new UnitSubUnitAttachmentData
		{
			Id = attachment.Id,
			ChildUnitId = attachment.ChildUnitId,
			Name = attachment.Name,
			Position = new Vector2(attachment.PositionX, attachment.PositionY),
			Rotation = attachment.Rotation,
			Order = attachment.Order,
			ParametersJson = attachment.ParametersJson,
			Traits = attachment.Traits.Select(ToDataTrait).ToList()
		}).ToList();

		return definition;
	}

	void ApplyTrait(UnitDefinition definition, string key, string valueType, string valueJson)
	{
		if (valueType == "number")
		{
			float value = JsonSerializer.Deserialize<float>(valueJson);
			definition.NumericalTraits[key] = value;
			return;
		}
		if (valueType == "text")
		{
			definition.DescriptiveTraits[key] = JsonSerializer.Deserialize<string>(valueJson) ?? "";
			return;
		}

		definition.DescriptiveTraits[key] = valueJson;
	}

	UnitDataTrait ToDataTrait(StoredUnitSpriteTrait trait)
	{
		return new UnitDataTrait
		{
			Key = trait.Key,
			ValueType = trait.ValueType,
			ValueJson = trait.ValueJson
		};
	}

	UnitDataTrait ToDataTrait(StoredUnitSubUnitTrait trait)
	{
		return new UnitDataTrait
		{
			Key = trait.Key,
			ValueType = trait.ValueType,
			ValueJson = trait.ValueJson
		};
	}

	Vector2 ReadPositionTraits(List<UnitDataTrait> traits, Vector2 fallback)
	{
		return new Vector2(
			ReadNumberTrait(traits, "positionX", fallback.X),
			ReadNumberTrait(traits, "positionY", fallback.Y));
	}

	float ReadNumberTrait(List<UnitDataTrait> traits, string key, float fallback)
	{
		UnitDataTrait trait = traits.FirstOrDefault(trait => trait.Key == key && trait.ValueType == "number");
		if (trait == null)
		{
			return fallback;
		}

		try
		{
			return JsonSerializer.Deserialize<float>(trait.ValueJson);
		}
		catch
		{
			return fallback;
		}
	}

	public void RegisterUnsavedObject(Guid id, string name, int savedVersion, Func<int> getCurrentVersion, Func<int> save, Action discard = null)
	{
		unsavedObjects[id] = new UnsavedObjectRegistration
		{
			Id = id,
			Name = name,
			SavedVersion = savedVersion,
			GetCurrentVersion = getCurrentVersion,
			Save = save,
			Discard = discard
		};
	}

	public void UnregisterUnsavedObject(Guid id)
	{
		unsavedObjects.Remove(id);
	}

	public void UpdateRegisteredSavedVersion(Guid id, int savedVersion)
	{
		if (unsavedObjects.ContainsKey(id))
		{
			unsavedObjects[id].SavedVersion = savedVersion;
		}
	}

	public bool CheckVersion(Guid id, bool popup = true)
	{
		bool result = false;
		bool completed = false;
		CheckVersion(id, canProceed =>
		{
			result = canProceed;
			completed = true;
		}, popup);
		return completed && result;
	}

	public void CheckVersion(Guid id, Action<bool> onComplete, bool popup = true)
	{
		if (!unsavedObjects.ContainsKey(id))
		{
			onComplete?.Invoke(true);
			return;
		}

		UnsavedObjectRegistration registration = unsavedObjects[id];
		if (registration.GetCurrentVersion() <= registration.SavedVersion)
		{
			onComplete?.Invoke(true);
			return;
		}

		if (!popup)
		{
			onComplete?.Invoke(false);
			return;
		}

		openUnsavedObjectWindow(registration, onComplete);
	}

	public void CheckUnsavedObjects(Action<bool> onComplete = null, bool popup = true)
	{
		List<Guid> ids = unsavedObjects.Keys.ToList();
		checkUnsavedObjectAtIndex(ids, 0, onComplete, popup);
	}

	void checkUnsavedObjectAtIndex(List<Guid> ids, int index, Action<bool> onComplete, bool popup)
	{
		if (index >= ids.Count)
		{
			onComplete?.Invoke(true);
			return;
		}

		Guid id = ids[index];
		CheckVersion(id, canProceed =>
		{
			if (!canProceed)
			{
				onComplete?.Invoke(false);
				return;
			}

			checkUnsavedObjectAtIndex(ids, index + 1, onComplete, popup);
		}, popup);
	}

	void openUnsavedObjectWindow(UnsavedObjectRegistration registration, Action<bool> onComplete)
	{
		VBoxContainer content = new VBoxContainer();
		content.CustomMinimumSize = new Vector2(300, 100);

		Label label = new Label();
		label.Text = registration.Name + " has unsaved changes.";
		label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		content.AddChild(label);

		HBoxContainer buttons = new HBoxContainer();
		content.AddChild(buttons);

		Button saveButton = new Button();
		saveButton.Text = "Save";
		saveButton.Pressed += () =>
		{
			int savedVersion = registration.Save();
			registration.SavedVersion = savedVersion;
			mAccess.windowManager.closeWindow("Unsaved Changes", false);
			onComplete?.Invoke(true);
		};
		buttons.AddChild(saveButton);

		Button dontSaveButton = new Button();
		dontSaveButton.Text = "Don't Save";
		dontSaveButton.Pressed += () =>
		{
			registration.Discard?.Invoke();
			UnregisterUnsavedObject(registration.Id);
			mAccess.windowManager.closeWindow("Unsaved Changes", false);
			onComplete?.Invoke(true);
		};
		buttons.AddChild(dontSaveButton);

		Button cancelButton = new Button();
		cancelButton.Text = "Cancel";
		cancelButton.Pressed += () =>
		{
			mAccess.windowManager.closeWindow("Unsaved Changes", false);
			onComplete?.Invoke(false);
		};
		buttons.AddChild(cancelButton);

		mAccess.windowManager.openWindow("Unsaved Changes", content, "transparentTopbarNoClose", false);
	}
}

public class UnsavedObjectRegistration
{
	public Guid Id { get; set; }
	public string Name { get; set; } = "";
	public int SavedVersion { get; set; }
	public Func<int> GetCurrentVersion { get; set; }
	public Func<int> Save { get; set; }
	public Action Discard { get; set; }
}
