using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using LibGit2Sharp;
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

public class StoredAnimation
{
	public Guid Id { get; set; }
	public string Name { get; set; } = "";
	public string AnimationType { get; set; } = "";
	public float Duration { get; set; }
	public List<StoredAnimationVariable> Variables { get; set; } = new();
	public List<StoredAnimationPropertyRequirement> PropertyRequirements { get; set; } = new();
	public List<StoredAnimationTransformation> Transformations { get; set; } = new();
}

public class StoredAnimationVariable
{
	public Guid Id { get; set; }
	public Guid StoredAnimationId { get; set; }
	public StoredAnimation StoredAnimation { get; set; }
	public string Name { get; set; } = "";
	public string Source { get; set; } = "";
}

public class StoredAnimationPropertyRequirement
{
	public Guid Id { get; set; }
	public Guid StoredAnimationId { get; set; }
	public StoredAnimation StoredAnimation { get; set; }
	public string TargetName { get; set; } = "";
	public string TargetTypeName { get; set; } = "";
	public string PropertyName { get; set; } = "";
	public string ValueTypeName { get; set; } = "";
	public string InterfaceName { get; set; } = "";
}

public class StoredAnimationTransformation
{
	public Guid Id { get; set; }
	public Guid StoredAnimationId { get; set; }
	public StoredAnimation StoredAnimation { get; set; }
	public string PropertyName { get; set; } = "";
	public string LoopVariable { get; set; } = "";
	public string LoopCountVariable { get; set; } = "";
	public float StartTime { get; set; }
	public float EndTime { get; set; }
	public string StartValue { get; set; } = "";
	public string EndValue { get; set; } = "";
	public string FunctionType { get; set; } = "";
}

public class StoredUnit
{
	public Guid Id { get; set; }
	public Guid GameId { get; set; }
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

public class StoredUnitComponentType
{
	public Guid Id { get; set; }
	public string TypeName { get; set; } = "";
	public string DisplayName { get; set; } = "";
	public string AssemblyName { get; set; } = "";
	public string Kind { get; set; } = "";
	public string DirectBaseTypeName { get; set; } = "";
	public Guid? DirectBaseTypeId { get; set; }
	public StoredUnitComponentType DirectBaseType { get; set; }
	public string SourceVersion { get; set; } = "";
	public List<StoredUnitComponentVariable> Variables { get; set; } = new();
	public List<StoredUnitComponentObjectMember> ObjectMembers { get; set; } = new();
}

public class StoredUnitComponentVariable
{
	public Guid Id { get; set; }
	public Guid StoredUnitComponentTypeId { get; set; }
	public StoredUnitComponentType StoredUnitComponentType { get; set; }
	public string Name { get; set; } = "";
	public string ValueTypeName { get; set; } = "";
	public string VariableKind { get; set; } = "";
	public bool IsPublic { get; set; }
	public bool CanRead { get; set; }
	public bool CanWrite { get; set; }
	public bool IsObjectReference { get; set; }
}

public class StoredUnitComponentObjectMember
{
	public Guid Id { get; set; }
	public Guid OwnerTypeId { get; set; }
	public StoredUnitComponentType OwnerType { get; set; }
	public string Name { get; set; } = "";
	public string MemberTypeName { get; set; } = "";
	public Guid? MemberTypeId { get; set; }
	public StoredUnitComponentType MemberType { get; set; }
	public string VariableKind { get; set; } = "";
	public bool IsCollection { get; set; }
	public string ElementTypeName { get; set; } = "";
}

public class StoredGameSetting
{
	public string Key { get; set; } = "";
	public string ValueJson { get; set; } = "";
}

public class StoredGame
{
	public Guid Id { get; set; }
	public string Name { get; set; } = "";
	public int Version { get; set; }
	public string DefaultSceneKey { get; set; } = "";
}

public class StoredGameScene
{
	public Guid Id { get; set; }
	public Guid GameId { get; set; }
	public string SceneKey { get; set; } = "";
	public string SceneTypeId { get; set; } = "";
	public string SceneResourcePath { get; set; } = "";
	public string UiKey { get; set; } = "";
	public string ModuleConfigJson { get; set; } = "{}";
	public string InitialEntitiesJson { get; set; } = "{}";
	public int Version { get; set; }
}

public class StoredGameCommandBinding
{
	public Guid Id { get; set; }
	public Guid GameId { get; set; }
	public string CommandType { get; set; } = "";
	public string ParentCommandType { get; set; } = "";
	public int KeyCode { get; set; }
	public string TargetTypesJson { get; set; } = "[]";
	public string CommandName { get; set; } = "";
}

public class StoredGameTeam
{
	public Guid Id { get; set; }
	public Guid GameId { get; set; }
	public int TeamIndex { get; set; }
	public string Name { get; set; } = "";
	public string EnemyTeamIndexesJson { get; set; } = "[]";
}

public class StoredSaveSlot
{
	public Guid Id { get; set; }
	public Guid GameId { get; set; }
	public string Name { get; set; } = "";
	public string SceneKey { get; set; } = "";
	public int Version { get; set; }
	public string StateJson { get; set; } = "{}";
	public DateTime UpdatedAtUtc { get; set; }
}

public class GameDbContext : DbContext
{
	public static string DatabasePath { get; set; } = ProjectSettings.GlobalizePath("user://game_data.db");
	public DbSet<StoredSprite> Sprites { get; set; }
	public DbSet<StoredSpriteLayer> SpriteLayers { get; set; }
	public DbSet<StoredAnimation> Animations { get; set; }
	public DbSet<StoredAnimationVariable> AnimationVariables { get; set; }
	public DbSet<StoredAnimationPropertyRequirement> AnimationPropertyRequirements { get; set; }
	public DbSet<StoredAnimationTransformation> AnimationTransformations { get; set; }
	public DbSet<StoredUnit> Units { get; set; }
	public DbSet<StoredUnitTrait> UnitTraits { get; set; }
	public DbSet<StoredUnitBehavior> UnitBehaviors { get; set; }
	public DbSet<StoredUnitAbility> UnitAbilities { get; set; }
	public DbSet<StoredUnitSpriteAttachment> UnitSpriteAttachments { get; set; }
	public DbSet<StoredUnitSpriteTrait> UnitSpriteTraits { get; set; }
	public DbSet<StoredUnitSubUnitAttachment> UnitSubUnitAttachments { get; set; }
	public DbSet<StoredUnitSubUnitTrait> UnitSubUnitTraits { get; set; }
	public DbSet<StoredUnitComponentType> UnitComponentTypes { get; set; }
	public DbSet<StoredUnitComponentVariable> UnitComponentVariables { get; set; }
	public DbSet<StoredUnitComponentObjectMember> UnitComponentObjectMembers { get; set; }
	public DbSet<StoredGameSetting> GameSettings { get; set; }
	public DbSet<StoredGame> Games { get; set; }
	public DbSet<StoredGameScene> GameScenes { get; set; }
	public DbSet<StoredGameCommandBinding> GameCommandBindings { get; set; }
	public DbSet<StoredGameTeam> GameTeams { get; set; }
	public DbSet<StoredSaveSlot> SaveSlots { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder options)
	{
		options.UseSqlite($"Data Source={DatabasePath}");
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

		modelBuilder.Entity<StoredAnimation>()
			.HasKey(animation => animation.Id);

		modelBuilder.Entity<StoredAnimationVariable>()
			.HasKey(variable => variable.Id);

		modelBuilder.Entity<StoredAnimationPropertyRequirement>()
			.HasKey(requirement => requirement.Id);

		modelBuilder.Entity<StoredAnimationTransformation>()
			.HasKey(transformation => transformation.Id);

		modelBuilder.Entity<StoredAnimation>()
			.HasMany(animation => animation.Variables)
			.WithOne(variable => variable.StoredAnimation)
			.HasForeignKey(variable => variable.StoredAnimationId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<StoredAnimation>()
			.HasMany(animation => animation.PropertyRequirements)
			.WithOne(requirement => requirement.StoredAnimation)
			.HasForeignKey(requirement => requirement.StoredAnimationId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<StoredAnimation>()
			.HasMany(animation => animation.Transformations)
			.WithOne(transformation => transformation.StoredAnimation)
			.HasForeignKey(transformation => transformation.StoredAnimationId)
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

		modelBuilder.Entity<StoredUnitComponentType>()
			.HasKey(type => type.Id);

		modelBuilder.Entity<StoredUnitComponentVariable>()
			.HasKey(variable => variable.Id);

		modelBuilder.Entity<StoredUnitComponentObjectMember>()
			.HasKey(member => member.Id);

		modelBuilder.Entity<StoredGameSetting>()
			.HasKey(setting => setting.Key);

		modelBuilder.Entity<StoredGame>().HasKey(game => game.Id);
		modelBuilder.Entity<StoredGameScene>().HasKey(scene => scene.Id);
		modelBuilder.Entity<StoredGameCommandBinding>().HasKey(binding => binding.Id);
		modelBuilder.Entity<StoredGameTeam>().HasKey(team => team.Id);
		modelBuilder.Entity<StoredSaveSlot>().HasKey(slot => slot.Id);

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

		modelBuilder.Entity<StoredUnitComponentType>()
			.HasIndex(type => type.TypeName)
			.IsUnique();

		modelBuilder.Entity<StoredUnitComponentType>()
			.HasOne(type => type.DirectBaseType)
			.WithMany()
			.HasForeignKey(type => type.DirectBaseTypeId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<StoredUnitComponentType>()
			.HasMany(type => type.Variables)
			.WithOne(variable => variable.StoredUnitComponentType)
			.HasForeignKey(variable => variable.StoredUnitComponentTypeId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<StoredUnitComponentType>()
			.HasMany(type => type.ObjectMembers)
			.WithOne(member => member.OwnerType)
			.HasForeignKey(member => member.OwnerTypeId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<StoredUnitComponentObjectMember>()
			.HasOne(member => member.MemberType)
			.WithMany()
			.HasForeignKey(member => member.MemberTypeId)
			.OnDelete(DeleteBehavior.SetNull);
	}
}

public partial class EntityFrameworkManagement : managerNode
{
	const string MainDatabaseName = "game_data.db";
	public static readonly Guid DefaultGameId = Guid.Parse("11111111-1111-1111-1111-111111111111");
	public static readonly Guid MobaGameId = Guid.Parse("22222222-2222-2222-2222-222222222222");
	const string TextDataFileName = "text data.json";
	const string GameSettingsKey = "game_settings";
	const string DefinitionKindTraitKey = "__definitionKind";
	const string ComponentAttachmentsTraitKey = "__componentAttachments";
	public Dictionary<Guid, UnsavedObjectRegistration> unsavedObjects;
	public DatabaseStorageTextData databaseTextData;
	public override void setup()
	{
		unsavedObjects = new Dictionary<Guid, UnsavedObjectRegistration>();
		SetupDatabaseStorage();
		SetupDatabase();
		ApplyStoredRuntimeSettings();
		mAccess.animationManager?.RegisterStoredDynamicAnimations(GetAnimations());
	}

	void SetupDatabaseStorage()
	{
		databaseTextData = LoadDatabaseTextData();
		string databaseName = MainDatabaseName;
		if (databaseTextData.UseBranchDatabases)
		{
			string branchName = GetCurrentBranchName();
			databaseName = CreateBranchDatabaseName(branchName);
			bool databaseIsRegistered = databaseTextData.ExistingDatabases.Contains(databaseName);
			bool databaseFileExists = File.Exists(GetDatabasePath(databaseName));
			if (!databaseIsRegistered || !databaseFileExists)
			{
				CopyMostRecentDatabase(databaseName, databaseTextData.MostRecentDatabaseName);
				if (!databaseIsRegistered)
				{
					databaseTextData.ExistingDatabases.Add(databaseName);
				}
			}
			databaseTextData.MostRecentDatabaseName = databaseName;
			SaveDatabaseTextData(databaseTextData);
		}

		GameDbContext.DatabasePath = GetDatabasePath(databaseName);
	}

	DatabaseStorageTextData LoadDatabaseTextData()
	{
		string textDataPath = GetTextDataPath();
		if (!File.Exists(textDataPath))
		{
			DatabaseStorageTextData defaultData = new DatabaseStorageTextData
			{
				MostRecentDatabaseName = MainDatabaseName,
				ExistingDatabases = new List<string> { MainDatabaseName },
				UseBranchDatabases = false
			};
			SaveDatabaseTextData(defaultData);
			return defaultData;
		}

		try
		{
			DatabaseStorageTextData loadedData = JsonSerializer.Deserialize<DatabaseStorageTextData>(File.ReadAllText(textDataPath), CreateTextDataJsonOptions()) ?? new DatabaseStorageTextData();
			loadedData.MostRecentDatabaseName = string.IsNullOrWhiteSpace(loadedData.MostRecentDatabaseName)
				? MainDatabaseName
				: loadedData.MostRecentDatabaseName;
			loadedData.ExistingDatabases ??= new List<string>();
			if (!loadedData.ExistingDatabases.Contains(MainDatabaseName))
			{
				loadedData.ExistingDatabases.Add(MainDatabaseName);
			}
			if (!loadedData.ExistingDatabases.Contains(loadedData.MostRecentDatabaseName))
			{
				loadedData.ExistingDatabases.Add(loadedData.MostRecentDatabaseName);
			}
			return loadedData;
		}
		catch
		{
			return new DatabaseStorageTextData
			{
				MostRecentDatabaseName = MainDatabaseName,
				ExistingDatabases = new List<string> { MainDatabaseName },
				UseBranchDatabases = false
			};
		}
	}

	void SaveDatabaseTextData(DatabaseStorageTextData textData)
	{
		string textDataPath = GetTextDataPath();
		Directory.CreateDirectory(Path.GetDirectoryName(textDataPath));
		File.WriteAllText(textDataPath, JsonSerializer.Serialize(textData, CreateTextDataJsonOptions()));
	}

	JsonSerializerOptions CreateTextDataJsonOptions()
	{
		return new JsonSerializerOptions
		{
			WriteIndented = true
		};
	}

	JsonSerializerOptions CreateDefinitionJsonOptions()
	{
		return new JsonSerializerOptions
		{
			IncludeFields = true
		};
	}

	string GetTextDataPath()
	{
		return ProjectSettings.GlobalizePath("user://" + TextDataFileName);
	}

	string GetDatabasePath(string databaseName)
	{
		return ProjectSettings.GlobalizePath("user://" + databaseName);
	}

	string GetCurrentBranchName()
	{
		try
		{
			string repositoryPath = Repository.Discover(ProjectSettings.GlobalizePath("res://"));
			if (string.IsNullOrEmpty(repositoryPath))
			{
				return "no_branch";
			}

			using Repository repository = new Repository(repositoryPath);
			return string.IsNullOrWhiteSpace(repository.Head.FriendlyName)
				? "detached_head"
				: repository.Head.FriendlyName;
		}
		catch
		{
			return "unknown_branch";
		}
	}

	string CreateBranchDatabaseName(string branchName)
	{
		string safeBranchName = Regex.Replace(branchName.ToLowerInvariant(), @"[^a-z0-9]+", "_").Trim('_');
		if (string.IsNullOrEmpty(safeBranchName))
		{
			safeBranchName = "unknown_branch";
		}

		return "game_data_" + safeBranchName + ".db";
	}

	void CopyMostRecentDatabase(string targetDatabaseName, string sourceDatabaseName)
	{
		string targetPath = GetDatabasePath(targetDatabaseName);
		if (File.Exists(targetPath))
		{
			return;
		}

		string sourcePath = GetDatabasePath(string.IsNullOrWhiteSpace(sourceDatabaseName) ? MainDatabaseName : sourceDatabaseName);
		if (!File.Exists(sourcePath))
		{
			sourcePath = GetDatabasePath(MainDatabaseName);
		}

		Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
		if (File.Exists(sourcePath))
		{
			File.Copy(sourcePath, targetPath);
		}
	}

	public void SetupDatabase()
	{
		using GameDbContext context = new GameDbContext();
		context.Database.EnsureCreated();
		EnsureSpriteVersionColumn(context);
		EnsureUnitTables(context);
		EnsureGameSettingsTable(context);
		EnsureGameSessionTables(context);
		EnsureDefaultGame(context);
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
			CREATE TABLE IF NOT EXISTS Animations (
				Id TEXT NOT NULL CONSTRAINT PK_Animations PRIMARY KEY,
				Name TEXT NOT NULL,
				AnimationType TEXT NOT NULL,
				Duration REAL NOT NULL
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS AnimationVariables (
				Id TEXT NOT NULL CONSTRAINT PK_AnimationVariables PRIMARY KEY,
				StoredAnimationId TEXT NOT NULL,
				Name TEXT NOT NULL,
				Source TEXT NOT NULL,
				CONSTRAINT FK_AnimationVariables_Animations_StoredAnimationId FOREIGN KEY (StoredAnimationId) REFERENCES Animations (Id) ON DELETE CASCADE
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS AnimationPropertyRequirements (
				Id TEXT NOT NULL CONSTRAINT PK_AnimationPropertyRequirements PRIMARY KEY,
				StoredAnimationId TEXT NOT NULL,
				TargetName TEXT NOT NULL,
				TargetTypeName TEXT NOT NULL,
				PropertyName TEXT NOT NULL,
				ValueTypeName TEXT NOT NULL,
				InterfaceName TEXT NOT NULL,
				CONSTRAINT FK_AnimationPropertyRequirements_Animations_StoredAnimationId FOREIGN KEY (StoredAnimationId) REFERENCES Animations (Id) ON DELETE CASCADE
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS AnimationTransformations (
				Id TEXT NOT NULL CONSTRAINT PK_AnimationTransformations PRIMARY KEY,
				StoredAnimationId TEXT NOT NULL,
				PropertyName TEXT NOT NULL,
				LoopVariable TEXT NOT NULL DEFAULT '',
				LoopCountVariable TEXT NOT NULL DEFAULT '',
				StartTime REAL NOT NULL,
				EndTime REAL NOT NULL,
				StartValue TEXT NOT NULL,
				EndValue TEXT NOT NULL,
				FunctionType TEXT NOT NULL,
				CONSTRAINT FK_AnimationTransformations_Animations_StoredAnimationId FOREIGN KEY (StoredAnimationId) REFERENCES Animations (Id) ON DELETE CASCADE
			)
			""");
		EnsureAnimationTransformationColumns(context);
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
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS UnitComponentTypes (
				Id TEXT NOT NULL CONSTRAINT PK_UnitComponentTypes PRIMARY KEY,
				TypeName TEXT NOT NULL,
				DisplayName TEXT NOT NULL,
				AssemblyName TEXT NOT NULL,
				Kind TEXT NOT NULL,
				DirectBaseTypeName TEXT NOT NULL,
				DirectBaseTypeId TEXT NULL,
				SourceVersion TEXT NOT NULL,
				CONSTRAINT FK_UnitComponentTypes_UnitComponentTypes_DirectBaseTypeId FOREIGN KEY (DirectBaseTypeId) REFERENCES UnitComponentTypes (Id) ON DELETE SET NULL
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE UNIQUE INDEX IF NOT EXISTS IX_UnitComponentTypes_TypeName ON UnitComponentTypes (TypeName)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS UnitComponentVariables (
				Id TEXT NOT NULL CONSTRAINT PK_UnitComponentVariables PRIMARY KEY,
				StoredUnitComponentTypeId TEXT NOT NULL,
				Name TEXT NOT NULL,
				ValueTypeName TEXT NOT NULL,
				VariableKind TEXT NOT NULL,
				IsPublic INTEGER NOT NULL,
				CanRead INTEGER NOT NULL,
				CanWrite INTEGER NOT NULL,
				IsObjectReference INTEGER NOT NULL,
				CONSTRAINT FK_UnitComponentVariables_UnitComponentTypes_StoredUnitComponentTypeId FOREIGN KEY (StoredUnitComponentTypeId) REFERENCES UnitComponentTypes (Id) ON DELETE CASCADE
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS UnitComponentObjectMembers (
				Id TEXT NOT NULL CONSTRAINT PK_UnitComponentObjectMembers PRIMARY KEY,
				OwnerTypeId TEXT NOT NULL,
				Name TEXT NOT NULL,
				MemberTypeName TEXT NOT NULL,
				MemberTypeId TEXT NULL,
				VariableKind TEXT NOT NULL,
				IsCollection INTEGER NOT NULL,
				ElementTypeName TEXT NOT NULL,
				CONSTRAINT FK_UnitComponentObjectMembers_UnitComponentTypes_OwnerTypeId FOREIGN KEY (OwnerTypeId) REFERENCES UnitComponentTypes (Id) ON DELETE CASCADE,
				CONSTRAINT FK_UnitComponentObjectMembers_UnitComponentTypes_MemberTypeId FOREIGN KEY (MemberTypeId) REFERENCES UnitComponentTypes (Id) ON DELETE SET NULL
			)
			""");
	}

	void EnsureGameSettingsTable(GameDbContext context)
	{
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS GameSettings (
				Key TEXT NOT NULL CONSTRAINT PK_GameSettings PRIMARY KEY,
				ValueJson TEXT NOT NULL
			)
			""");
	}

	void EnsureGameSessionTables(GameDbContext context)
	{
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS Games (
				Id TEXT NOT NULL CONSTRAINT PK_Games PRIMARY KEY,
				Name TEXT NOT NULL,
				Version INTEGER NOT NULL,
				DefaultSceneKey TEXT NOT NULL
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS GameScenes (
				Id TEXT NOT NULL CONSTRAINT PK_GameScenes PRIMARY KEY,
				GameId TEXT NOT NULL,
				SceneKey TEXT NOT NULL,
				SceneTypeId TEXT NOT NULL,
				SceneResourcePath TEXT NOT NULL,
				UiKey TEXT NOT NULL,
				ModuleConfigJson TEXT NOT NULL,
				InitialEntitiesJson TEXT NOT NULL,
				Version INTEGER NOT NULL
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE UNIQUE INDEX IF NOT EXISTS IX_GameScenes_GameId_SceneKey ON GameScenes (GameId, SceneKey)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS GameCommandBindings (
				Id TEXT NOT NULL CONSTRAINT PK_GameCommandBindings PRIMARY KEY,
				GameId TEXT NOT NULL,
				CommandType TEXT NOT NULL,
				ParentCommandType TEXT NOT NULL,
				KeyCode INTEGER NOT NULL,
				TargetTypesJson TEXT NOT NULL,
				CommandName TEXT NOT NULL
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS GameTeams (
				Id TEXT NOT NULL CONSTRAINT PK_GameTeams PRIMARY KEY,
				GameId TEXT NOT NULL,
				TeamIndex INTEGER NOT NULL,
				Name TEXT NOT NULL,
				EnemyTeamIndexesJson TEXT NOT NULL
			)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE UNIQUE INDEX IF NOT EXISTS IX_GameTeams_GameId_TeamIndex ON GameTeams (GameId, TeamIndex)
			""");
		context.Database.ExecuteSqlRaw("""
			CREATE TABLE IF NOT EXISTS SaveSlots (
				Id TEXT NOT NULL CONSTRAINT PK_SaveSlots PRIMARY KEY,
				GameId TEXT NOT NULL,
				Name TEXT NOT NULL,
				SceneKey TEXT NOT NULL,
				Version INTEGER NOT NULL,
				StateJson TEXT NOT NULL,
				UpdatedAtUtc TEXT NOT NULL
			)
			""");
		try
		{
			context.Database.ExecuteSqlRaw("ALTER TABLE Units ADD COLUMN GameId TEXT NOT NULL DEFAULT '11111111-1111-1111-1111-111111111111'");
		}
		catch
		{
		}
		context.Database.ExecuteSqlRaw("UPDATE Units SET GameId = '11111111-1111-1111-1111-111111111111' WHERE GameId IS NULL OR GameId = ''");
	}

	void EnsureDefaultGame(GameDbContext context)
	{
		if (!context.Games.Any(game => game.Id == DefaultGameId))
		{
			context.Games.Add(new StoredGame
			{
				Id = DefaultGameId,
				Name = "Cool Beats",
				Version = 1,
				DefaultSceneKey = "tactical"
			});
		}

		EnsureDefaultScene(context, "tactical", "tactical-selection", "res://Scenes/GameScenes/game_scene.tscn", "game",
			"{\"SpawnPlayerCamera\":true,\"Units\":[{\"DefinitionName\":\"marine\",\"TeamIndex\":0,\"PositionX\":-120,\"PositionY\":0},{\"DefinitionName\":\"marine\",\"TeamIndex\":1,\"PositionX\":120,\"PositionY\":0}]}" );
		EnsureDefaultScene(context, "spriteCreator", "sprite-authoring", "res://Scenes/GameScenes/MenuScene.tscn", "spriteCreator", "{}");
		EnsureDefaultScene(context, "unitCreator", "unit-authoring", "res://Scenes/GameScenes/MenuScene.tscn", "unitCreator", "{}");
		EnsureDefaultScene(context, "sandbox", "sandbox", "res://Scenes/GameScenes/sandbox_scene.tscn", "sandbox", "{}");
		EnsureDefaultCommandBindings(context);
		EnsureDefaultTeams(context);
		EnsureMobaGame(context);
		context.SaveChanges();
	}

	void EnsureMobaGame(GameDbContext context)
	{
		if (!context.Games.Any(game => game.Id == MobaGameId))
		{
			context.Games.Add(new StoredGame { Id = MobaGameId, Name = "MOBA — Destructible Sprite Arena", Version = 1, DefaultSceneKey = "moba" });
		}
		EnsureScene(context, MobaGameId, "moba", "moba-projectile-arena", "res://Scenes/GameScenes/moba_scene.tscn", "sandbox", "{}");
		// This is deliberately an idempotent data migration: early builds stored
		// the rule in InitialEntitiesJson, while GameSession reads ModuleConfigJson.
		StoredGameScene mobaScene = context.GameScenes.First(scene => scene.GameId == MobaGameId && scene.SceneKey == "moba");
		const string mobaRules = "{\"Rules\":{\"damageableSprites\":true,\"destructibleParts\":true}}";
		if (mobaScene.ModuleConfigJson != mobaRules)
		{
			mobaScene.ModuleConfigJson = mobaRules;
			mobaScene.Version++;
		}
		EnsureMobaDestructibleEnemyDefinition(context);
		EnsureTeams(context, MobaGameId, "Player", "Test Enemy");
	}

	void EnsureMobaDestructibleEnemyDefinition(GameDbContext context)
	{
		const string definitionName = "moba_test_enemy";
		DestructibleAssemblyDefinition definition = new DestructibleAssemblyDefinition
		{
			MaxBodyHealth = 40,
			Parts = new List<DestructiblePartDefinition>
			{
				new DestructiblePartDefinition { Id = "body", MaxHitPoints = 20, BodyDamageMultiplier = 0.5f, GridWidth = 3, GridHeight = 3, PixelSize = 32, MaterialColor = "df4a4a", WeakSpotX = -16, WeakSpotY = -16, WeakSpotWidth = 32, WeakSpotHeight = 32, WeakSpotMultiplier = 3 },
				new DestructiblePartDefinition { Id = "arm", PositionX = 54, PositionY = -70, MaxHitPoints = 2, BodyDamageMultiplier = 0.25f, GridWidth = 2, GridHeight = 2, PixelSize = 28, MaterialColor = "e9b44c", Effects = new List<PartEffectDefinition>
					{ new PartEffectDefinition { Type = PartEffectType.DisableAbility }, new PartEffectDefinition { Type = PartEffectType.ArmourPenalty, Value = 4 }, new PartEffectDefinition { Type = PartEffectType.MovementMultiplier, Value = 0.5f }, new PartEffectDefinition { Type = PartEffectType.Bleed } } }
			},
			Joints = new List<StructuralJointDefinition>
			{
				new StructuralJointDefinition { ParentPartId = "body", ChildPartId = "arm", ChildLoad = 0.5f, BreakImpulse = 4, ParentAnchors = new List<StructuralAnchorDefinition> { new StructuralAnchorDefinition { X = 2, Y = 0 }, new StructuralAnchorDefinition { X = 2, Y = 1 }, new StructuralAnchorDefinition { X = 2, Y = 2 } } }
			}
		};
		string definitionJson = JsonSerializer.Serialize(definition);
		StoredUnit unit = context.Units.FirstOrDefault(candidate => candidate.GameId == MobaGameId && candidate.Name == definitionName);
		if (unit == null)
		{
			unit = new StoredUnit { Id = Guid.NewGuid(), GameId = MobaGameId, Name = definitionName, Version = 1, CommandType = "", MaxHP = definition.MaxBodyHealth };
			context.Units.Add(unit);
		}
		StoredUnitTrait trait = context.UnitTraits.FirstOrDefault(candidate => candidate.StoredUnitId == unit.Id && candidate.Key == "destructibleParts");
		if (trait == null)
		{
			trait = new StoredUnitTrait { Id = Guid.NewGuid(), StoredUnitId = unit.Id, Key = "destructibleParts" };
			context.UnitTraits.Add(trait);
		}
		trait.ValueType = "text";
		trait.ValueJson = JsonSerializer.Serialize(definitionJson);
	}

	void EnsureDefaultScene(GameDbContext context, string key, string typeId, string scenePath, string uiKey, string initialEntitiesJson)
		=> EnsureScene(context, DefaultGameId, key, typeId, scenePath, uiKey, initialEntitiesJson);

	void EnsureScene(GameDbContext context, Guid gameId, string key, string typeId, string scenePath, string uiKey, string initialEntitiesJson)
	{
		if (context.GameScenes.Any(scene => scene.GameId == gameId && scene.SceneKey == key))
		{
			return;
		}
		context.GameScenes.Add(new StoredGameScene
		{
			Id = Guid.NewGuid(),
			GameId = gameId,
			SceneKey = key,
			SceneTypeId = typeId,
			SceneResourcePath = scenePath,
			UiKey = uiKey,
			InitialEntitiesJson = initialEntitiesJson,
			Version = 1
		});
	}

	void EnsureDefaultCommandBindings(GameDbContext context)
	{
		if (context.GameCommandBindings.Any(binding => binding.GameId == DefaultGameId))
		{
			return;
		}
		AddDefaultBinding(context, "", "", Key.None, Array.Empty<string>(), "");
		AddDefaultBinding(context, "commandable", "", Key.Backspace, new[] { "active" }, "idle");
		AddDefaultBinding(context, "rallyable", "commandable", Key.None, new[] { "ground", "team", "ally", "enemy" }, "move");
		AddDefaultBinding(context, "rallyable", "commandable", Key.P, new[] { "ground", "team", "ally", "enemy" }, "patrol");
		AddDefaultBinding(context, "rallyable", "commandable", Key.H, new[] { "active" }, "holdPosition");
		AddDefaultBinding(context, "attacker", "rallyable", Key.A, new[] { "ground" }, "attackMove");
		AddDefaultBinding(context, "barracks", "commandable", Key.A, new[] { "active" }, "train");
	}

	void AddDefaultBinding(GameDbContext context, string commandType, string parentCommandType, Key key, string[] targetTypes, string commandName)
	{
		context.GameCommandBindings.Add(new StoredGameCommandBinding
		{
			Id = Guid.NewGuid(),
			GameId = DefaultGameId,
			CommandType = commandType,
			ParentCommandType = parentCommandType,
			KeyCode = (int)key,
			TargetTypesJson = JsonSerializer.Serialize(targetTypes),
			CommandName = commandName
		});
	}

	void EnsureDefaultTeams(GameDbContext context)
		=> EnsureTeams(context, DefaultGameId, "Team 1", "Team 2");

	void EnsureTeams(GameDbContext context, Guid gameId, string firstName, string secondName)
	{
		if (context.GameTeams.Any(team => team.GameId == gameId))
		{
			return;
		}
		context.GameTeams.AddRange(
			new StoredGameTeam { Id = Guid.NewGuid(), GameId = gameId, TeamIndex = 0, Name = firstName, EnemyTeamIndexesJson = "[1]" },
			new StoredGameTeam { Id = Guid.NewGuid(), GameId = gameId, TeamIndex = 1, Name = secondName, EnemyTeamIndexesJson = "[0]" });
	}

	void EnsureAnimationTransformationColumns(GameDbContext context)
	{
		try
		{
			context.Database.ExecuteSqlRaw("ALTER TABLE AnimationTransformations ADD COLUMN LoopVariable TEXT NOT NULL DEFAULT ''");
		}
		catch
		{
		}
		try
		{
			context.Database.ExecuteSqlRaw("ALTER TABLE AnimationTransformations ADD COLUMN LoopCountVariable TEXT NOT NULL DEFAULT ''");
		}
		catch
		{
		}
	}

	public StoredGameSettings LoadGameSettings()
	{
		using GameDbContext context = new GameDbContext();
		StoredGameSetting setting = context.GameSettings.FirstOrDefault(setting => setting.Key == GameSettingsKey);
		if (setting == null || string.IsNullOrWhiteSpace(setting.ValueJson))
		{
			return null;
		}

		try
		{
			return JsonSerializer.Deserialize<StoredGameSettings>(setting.ValueJson, CreateTextDataJsonOptions()) ?? new StoredGameSettings();
		}
		catch
		{
			return null;
		}
	}

	public void SaveGameSettings(StoredGameSettings settings)
	{
		using GameDbContext context = new GameDbContext();
		StoredGameSetting setting = context.GameSettings.FirstOrDefault(setting => setting.Key == GameSettingsKey);
		if (setting == null)
		{
			setting = new StoredGameSetting
			{
				Key = GameSettingsKey
			};
			context.GameSettings.Add(setting);
		}

		setting.ValueJson = JsonSerializer.Serialize(settings, CreateTextDataJsonOptions());
		context.SaveChanges();
	}

	public StoredGame GetDefaultGame()
	{
		return GetGame(DefaultGameId);
	}

	// This is intentionally a curated start-menu list. Saved or authoring games
	// may exist in the database without becoming a launch option automatically.
	public List<StoredGame> GetStartGameTypes()
	{
		using GameDbContext context = new GameDbContext();
		return context.Games.Where(game => game.Id == DefaultGameId || game.Id == MobaGameId)
			.OrderBy(game => game.Id == DefaultGameId ? 0 : 1).ToList();
	}

	public StoredGame GetGame(Guid gameId)
	{
		using GameDbContext context = new GameDbContext();
		return context.Games.FirstOrDefault(game => game.Id == gameId);
	}

	public void SaveGame(StoredGame game)
	{
		using GameDbContext context = new GameDbContext();
		StoredGame existing = context.Games.FirstOrDefault(item => item.Id == game.Id);
		if (existing == null)
		{
			context.Games.Add(game);
		}
		else
		{
			existing.Name = game.Name;
			existing.Version = game.Version;
			existing.DefaultSceneKey = game.DefaultSceneKey;
		}
		context.SaveChanges();
	}

	public StoredGameScene GetGameScene(Guid gameId, string sceneKey)
	{
		using GameDbContext context = new GameDbContext();
		return context.GameScenes.FirstOrDefault(scene => scene.GameId == gameId && scene.SceneKey == sceneKey);
	}

	public void SaveGameScene(StoredGameScene scene)
	{
		using GameDbContext context = new GameDbContext();
		StoredGameScene existing = context.GameScenes.FirstOrDefault(item => item.Id == scene.Id);
		if (existing == null)
		{
			if (scene.Id == Guid.Empty) scene.Id = Guid.NewGuid();
			context.GameScenes.Add(scene);
		}
		else
		{
			existing.SceneKey = scene.SceneKey;
			existing.SceneTypeId = scene.SceneTypeId;
			existing.SceneResourcePath = scene.SceneResourcePath;
			existing.UiKey = scene.UiKey;
			existing.ModuleConfigJson = scene.ModuleConfigJson;
			existing.InitialEntitiesJson = scene.InitialEntitiesJson;
			existing.Version = scene.Version;
		}
		context.SaveChanges();
	}

	public List<StoredGameCommandBinding> GetGameCommandBindings(Guid gameId)
	{
		using GameDbContext context = new GameDbContext();
		return context.GameCommandBindings
			.Where(binding => binding.GameId == gameId)
			.OrderBy(binding => binding.CommandType)
			.ThenBy(binding => binding.KeyCode)
			.ToList();
	}

	public void SaveGameCommandBindings(Guid gameId, IEnumerable<StoredGameCommandBinding> bindings)
	{
		using GameDbContext context = new GameDbContext();
		context.GameCommandBindings.Where(binding => binding.GameId == gameId).ExecuteDelete();
		foreach (StoredGameCommandBinding binding in bindings)
		{
			binding.Id = binding.Id == Guid.Empty ? Guid.NewGuid() : binding.Id;
			binding.GameId = gameId;
			context.GameCommandBindings.Add(binding);
		}
		context.SaveChanges();
	}

	public List<StoredGameTeam> GetGameTeams(Guid gameId)
	{
		using GameDbContext context = new GameDbContext();
		return context.GameTeams.Where(team => team.GameId == gameId).OrderBy(team => team.TeamIndex).ToList();
	}

	public void SaveGameTeams(Guid gameId, IEnumerable<StoredGameTeam> teams)
	{
		using GameDbContext context = new GameDbContext();
		context.GameTeams.Where(team => team.GameId == gameId).ExecuteDelete();
		foreach (StoredGameTeam team in teams)
		{
			team.Id = team.Id == Guid.Empty ? Guid.NewGuid() : team.Id;
			team.GameId = gameId;
			context.GameTeams.Add(team);
		}
		context.SaveChanges();
	}

	public int SaveGameSession(Guid gameId, string name, string sceneKey, int version, string stateJson)
	{
		using GameDbContext context = new GameDbContext();
		StoredSaveSlot slot = context.SaveSlots.FirstOrDefault(item => item.GameId == gameId && item.Name == name);
		if (slot == null)
		{
			slot = new StoredSaveSlot { Id = Guid.NewGuid(), GameId = gameId, Name = name };
			context.SaveSlots.Add(slot);
		}
		slot.SceneKey = sceneKey;
		slot.Version = version;
		slot.StateJson = stateJson;
		slot.UpdatedAtUtc = DateTime.UtcNow;
		context.SaveChanges();
		return slot.Version;
	}

	public StoredSaveSlot GetGameSession(Guid gameId, string name)
	{
		using GameDbContext context = new GameDbContext();
		return context.SaveSlots.FirstOrDefault(slot => slot.GameId == gameId && slot.Name == name);
	}

	void ApplyStoredRuntimeSettings()
	{
		StoredGameSettings settings = LoadGameSettings();
		if (settings == null)
		{
			return;
		}

		int busIndex = AudioServer.GetBusIndex("Master");
		float linearVolume = Mathf.Max(settings.MasterVolumePercent / 100f, 0.0001f);
		AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(linearVolume));
		if (settings.ResolutionWidth > 0 && settings.ResolutionHeight > 0)
		{
			DisplayServer.WindowSetSize(new Vector2I(settings.ResolutionWidth, settings.ResolutionHeight));
		}
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

	public void SaveAnimation(StoredAnimation animation)
	{
		using GameDbContext context = new GameDbContext();
		using var transaction = context.Database.BeginTransaction();
		StoredAnimation storedAnimation = context.Animations.FirstOrDefault(storedAnimation => storedAnimation.Id == animation.Id);
		if (storedAnimation == null)
		{
			storedAnimation = context.Animations.FirstOrDefault(storedAnimation => storedAnimation.Name == animation.Name);
		}

		if (storedAnimation == null)
		{
			storedAnimation = new StoredAnimation
			{
				Id = animation.Id == Guid.Empty ? Guid.NewGuid() : animation.Id
			};
			context.Animations.Add(storedAnimation);
		}
		else
		{
			animation.Id = storedAnimation.Id;
			context.AnimationVariables
				.Where(variable => variable.StoredAnimationId == storedAnimation.Id)
				.ExecuteDelete();
			context.AnimationPropertyRequirements
				.Where(requirement => requirement.StoredAnimationId == storedAnimation.Id)
				.ExecuteDelete();
			context.AnimationTransformations
				.Where(transformation => transformation.StoredAnimationId == storedAnimation.Id)
				.ExecuteDelete();
		}

		storedAnimation.Name = animation.Name;
		storedAnimation.AnimationType = animation.AnimationType;
		storedAnimation.Duration = animation.Duration;
		context.SaveChanges();

		foreach (StoredAnimationVariable variable in animation.Variables)
		{
			variable.Id = Guid.NewGuid();
			variable.StoredAnimationId = storedAnimation.Id;
			variable.StoredAnimation = null;
		}

		foreach (StoredAnimationPropertyRequirement requirement in animation.PropertyRequirements)
		{
			requirement.Id = Guid.NewGuid();
			requirement.StoredAnimationId = storedAnimation.Id;
			requirement.StoredAnimation = null;
		}

		foreach (StoredAnimationTransformation transformation in animation.Transformations)
		{
			transformation.Id = Guid.NewGuid();
			transformation.StoredAnimationId = storedAnimation.Id;
			transformation.StoredAnimation = null;
		}

		context.AnimationVariables.AddRange(animation.Variables);
		context.AnimationPropertyRequirements.AddRange(animation.PropertyRequirements);
		context.AnimationTransformations.AddRange(animation.Transformations);
		context.SaveChanges();
		transaction.Commit();
	}

	public List<StoredAnimation> GetAnimations()
	{
		using GameDbContext context = new GameDbContext();
		return context.Animations
			.Include(animation => animation.Variables)
			.Include(animation => animation.PropertyRequirements)
			.Include(animation => animation.Transformations)
			.Select(animation => new StoredAnimation
			{
				Id = animation.Id,
				Name = animation.Name,
				AnimationType = animation.AnimationType,
				Duration = animation.Duration,
				Variables = animation.Variables.ToList(),
				PropertyRequirements = animation.PropertyRequirements.ToList(),
				Transformations = animation.Transformations
					.OrderBy(transformation => transformation.StartTime)
					.ToList()
			})
			.OrderBy(animation => animation.Name)
			.ToList();
	}

	public List<StoredUnitComponentType> EnsureUnitVariableMetadata(IEnumerable<Type> rootTypes)
	{
		List<UnitVariableTypeDefinition> definitions = UnitVariableMetadataScanner.Scan(rootTypes);
		using GameDbContext context = new GameDbContext();
		using var transaction = context.Database.BeginTransaction();

		Dictionary<string, StoredUnitComponentType> storedTypes = context.UnitComponentTypes
			.ToDictionary(type => type.TypeName);

		foreach (UnitVariableTypeDefinition definition in definitions)
		{
			if (!storedTypes.TryGetValue(definition.TypeName, out StoredUnitComponentType storedType))
			{
				storedType = new StoredUnitComponentType
				{
					Id = Guid.NewGuid(),
					TypeName = definition.TypeName
				};
				context.UnitComponentTypes.Add(storedType);
				storedTypes[definition.TypeName] = storedType;
			}

			storedType.DisplayName = definition.DisplayName;
			storedType.AssemblyName = definition.AssemblyName;
			storedType.Kind = definition.Kind;
			storedType.DirectBaseTypeName = definition.DirectBaseTypeName;
			storedType.SourceVersion = definition.SourceVersion;
		}
		context.SaveChanges();

		storedTypes = context.UnitComponentTypes.ToDictionary(type => type.TypeName);
		foreach (UnitVariableTypeDefinition definition in definitions)
		{
			StoredUnitComponentType storedType = storedTypes[definition.TypeName];
			storedType.DirectBaseTypeId = !string.IsNullOrEmpty(definition.DirectBaseTypeName) &&
				storedTypes.TryGetValue(definition.DirectBaseTypeName, out StoredUnitComponentType baseType)
				? baseType.Id
				: null;
		}
		context.SaveChanges();

		List<Guid> refreshedTypeIds = definitions.Select(definition => storedTypes[definition.TypeName].Id).ToList();
		context.UnitComponentObjectMembers
			.Where(member => refreshedTypeIds.Contains(member.OwnerTypeId))
			.ExecuteDelete();
		context.UnitComponentVariables
			.Where(variable => refreshedTypeIds.Contains(variable.StoredUnitComponentTypeId))
			.ExecuteDelete();

		foreach (UnitVariableTypeDefinition definition in definitions)
		{
			StoredUnitComponentType storedType = storedTypes[definition.TypeName];
			foreach (UnitVariableDefinition variable in definition.Variables)
			{
				context.UnitComponentVariables.Add(new StoredUnitComponentVariable
				{
					Id = Guid.NewGuid(),
					StoredUnitComponentTypeId = storedType.Id,
					Name = variable.Name,
					ValueTypeName = variable.ValueTypeName,
					VariableKind = variable.VariableKind,
					IsPublic = variable.IsPublic,
					CanRead = variable.CanRead,
					CanWrite = variable.CanWrite,
					IsObjectReference = variable.IsObjectReference
				});
			}

			foreach (UnitVariableObjectMemberDefinition objectMember in definition.ObjectMembers)
			{
				string referencedTypeName = string.IsNullOrEmpty(objectMember.ElementTypeName)
					? objectMember.MemberTypeName
					: objectMember.ElementTypeName;
				context.UnitComponentObjectMembers.Add(new StoredUnitComponentObjectMember
				{
					Id = Guid.NewGuid(),
					OwnerTypeId = storedType.Id,
					Name = objectMember.Name,
					MemberTypeName = objectMember.MemberTypeName,
					MemberTypeId = storedTypes.TryGetValue(referencedTypeName, out StoredUnitComponentType memberType) ? memberType.Id : null,
					VariableKind = objectMember.VariableKind,
					IsCollection = objectMember.IsCollection,
					ElementTypeName = objectMember.ElementTypeName
				});
			}
		}

		context.SaveChanges();
		transaction.Commit();
		return GetUnitVariableMetadata();
	}

	public List<StoredUnitComponentType> GetUnitVariableMetadata()
	{
		using GameDbContext context = new GameDbContext();
		return context.UnitComponentTypes
			.Include(type => type.DirectBaseType)
			.Include(type => type.Variables)
			.Include(type => type.ObjectMembers)
				.ThenInclude(member => member.MemberType)
			.AsSplitQuery()
			.Select(type => new StoredUnitComponentType
			{
				Id = type.Id,
				TypeName = type.TypeName,
				DisplayName = type.DisplayName,
				AssemblyName = type.AssemblyName,
				Kind = type.Kind,
				DirectBaseTypeName = type.DirectBaseTypeName,
				DirectBaseTypeId = type.DirectBaseTypeId,
				DirectBaseType = type.DirectBaseType,
				SourceVersion = type.SourceVersion,
				Variables = type.Variables.OrderBy(variable => variable.Name).ToList(),
				ObjectMembers = type.ObjectMembers.OrderBy(member => member.Name).ToList()
			})
			.OrderBy(type => type.TypeName)
			.ToList();
	}

	public void EnsureUnitDefinition(UnitDefinition definition, Guid? gameId = null)
	{
		Guid selectedGameId = gameId ?? DefaultGameId;
		using GameDbContext context = new GameDbContext();
		StoredUnit unit = context.Units.FirstOrDefault(unit => unit.GameId == selectedGameId && unit.Name == definition.Name);
		if (unit == null)
		{
			SaveUnit(definition, selectedGameId);
			return;
		}

		definition.Id = unit.Id;
		definition.Version = unit.Version;
	}

	public int SaveUnit(UnitDefinition definition, Guid? gameId = null)
	{
		Guid selectedGameId = gameId ?? DefaultGameId;
		using GameDbContext context = new GameDbContext();
		using var transaction = context.Database.BeginTransaction();
		StoredUnit unit = context.Units.FirstOrDefault(unit => unit.Id == definition.Id && unit.GameId == selectedGameId);

		if (unit == null)
		{
			unit = context.Units.FirstOrDefault(unit => unit.GameId == selectedGameId && unit.Name == definition.Name);
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
		unit.GameId = selectedGameId;
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
		yield return new StoredUnitTrait
		{
			Id = Guid.NewGuid(),
			StoredUnitId = unitId,
			Key = DefinitionKindTraitKey,
			ValueType = "text",
			ValueJson = JsonSerializer.Serialize(definition.DefinitionKind)
		};
		yield return new StoredUnitTrait
		{
			Id = Guid.NewGuid(),
			StoredUnitId = unitId,
			Key = ComponentAttachmentsTraitKey,
			ValueType = "json",
			ValueJson = JsonSerializer.Serialize(definition.ComponentAttachments, CreateDefinitionJsonOptions())
		};
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

	public List<UnitDefinition> GetUnitDefinitions(Func<IEnumerable<IUnitBehavior>> behaviorFactory, Guid? gameId = null)
	{
		return GetUnits(gameId).Select(unit => ToUnitDefinition(unit, behaviorFactory)).ToList();
	}

	public List<StoredUnit> GetUnits(Guid? gameId = null)
	{
		Guid selectedGameId = gameId ?? DefaultGameId;
		using GameDbContext context = new GameDbContext();
		List<StoredUnit> units = context.Units
			.Where(unit => unit.GameId == selectedGameId)
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
				GameId = unit.GameId,
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
		if (key == DefinitionKindTraitKey)
		{
			definition.DefinitionKind = JsonSerializer.Deserialize<string>(valueJson) ?? "unit";
			return;
		}
		if (key == ComponentAttachmentsTraitKey)
		{
			try
			{
				definition.ComponentAttachments = JsonSerializer.Deserialize<List<UnitComponentAttachmentData>>(valueJson, CreateDefinitionJsonOptions())
					?? new List<UnitComponentAttachmentData>();
			}
			catch
			{
				definition.ComponentAttachments = new List<UnitComponentAttachmentData>();
			}
			return;
		}
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
		mAccess.styleManager.applyTextStyle(label, "default");
		content.AddChild(label);

		HBoxContainer buttons = new HBoxContainer();
		content.AddChild(buttons);

		Button saveButton = new Button();
		saveButton.Text = "Save";
		mAccess.styleManager.applyButtonStyle(saveButton, "secondary");
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
		mAccess.styleManager.applyButtonStyle(dontSaveButton, "secondary");
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
		mAccess.styleManager.applyButtonStyle(cancelButton, "secondary");
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

public class DatabaseStorageTextData
{
	public string MostRecentDatabaseName { get; set; } = "game_data.db";
	public List<string> ExistingDatabases { get; set; } = new();
	public bool UseBranchDatabases { get; set; }
}

public class StoredGameSettings
{
	public int ActiveColorSchemeIndex { get; set; }
	public float MasterVolumePercent { get; set; } = 100f;
	public int ResolutionWidth { get; set; }
	public int ResolutionHeight { get; set; }
	public Dictionary<string, StoredColorValue> Colors { get; set; } = new();
	public Dictionary<string, StoredTextStyleSettings> TextStyles { get; set; } = new();
	public List<StoredColorSchemeSettings> ColorSchemes { get; set; } = new();
}

public class StoredColorSchemeSettings
{
	public string Name { get; set; } = "";
	public List<StoredColorValue> Colors { get; set; } = new();

	public StoredColorSchemeSettings()
	{
	}

	public StoredColorSchemeSettings(ColorScheme scheme)
	{
		Name = scheme.name;
		Colors = scheme.colors.Select(color => new StoredColorValue(color)).ToList();
	}
}

public class StoredColorValue
{
	public float R { get; set; }
	public float G { get; set; }
	public float B { get; set; }
	public float A { get; set; } = 1f;

	public StoredColorValue()
	{
	}

	public StoredColorValue(Color color)
	{
		R = color.R;
		G = color.G;
		B = color.B;
		A = color.A;
	}

	public Color ToColor()
	{
		return new Color(R, G, B, A);
	}
}

public class StoredTextStyleSettings
{
	public string ColorName { get; set; } = "";
	public string HoverColorName { get; set; } = "";
	public string PressedColorName { get; set; } = "";
	public string DisabledColorName { get; set; } = "";
	public int FontSize { get; set; }
	public string FontName { get; set; } = "";
	public bool Bold { get; set; }
	public bool Italic { get; set; }
	public bool Underline { get; set; }

	public StoredTextStyleSettings()
	{
	}

	public StoredTextStyleSettings(TextStyle style)
	{
		ColorName = style.colorName;
		HoverColorName = style.hoverColorName;
		PressedColorName = style.pressedColorName;
		DisabledColorName = style.disabledColorName;
		FontSize = style.fontSize;
		FontName = style.fontName;
		Bold = style.bold;
		Italic = style.italic;
		Underline = style.underline;
	}

	public TextStyle ToTextStyle(TextStyle fallback)
	{
		return new TextStyle
		(
			string.IsNullOrWhiteSpace(ColorName) ? fallback.colorName : ColorName,
			string.IsNullOrWhiteSpace(HoverColorName) ? fallback.hoverColorName : HoverColorName,
			string.IsNullOrWhiteSpace(PressedColorName) ? fallback.pressedColorName : PressedColorName,
			string.IsNullOrWhiteSpace(DisabledColorName) ? fallback.disabledColorName : DisabledColorName,
			FontSize <= 0 ? fallback.fontSize : FontSize,
			string.IsNullOrWhiteSpace(FontName) ? fallback.fontName : FontName,
			Bold,
			Italic,
			Underline
		);
	}
}
