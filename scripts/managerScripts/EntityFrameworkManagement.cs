using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
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

public class GameDbContext : DbContext
{
	public DbSet<StoredSprite> Sprites { get; set; }
	public DbSet<StoredSpriteLayer> SpriteLayers { get; set; }

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
