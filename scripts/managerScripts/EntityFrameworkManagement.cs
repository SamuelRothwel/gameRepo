using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class StoredSprite
{
	public Guid Id { get; set; }
	public string Name { get; set; } = "";
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
	public override void setup()
	{
		SetupDatabase();
	}

	public void SetupDatabase()
	{
		using GameDbContext context = new GameDbContext();
		context.Database.EnsureCreated();
	}

	public Guid SaveSprite(string name, List<StoredSpriteLayer> layers)
	{
		using GameDbContext context = new GameDbContext();
		StoredSprite sprite = new StoredSprite
		{
			Id = Guid.NewGuid(),
			Name = name,
			Layers = layers
		};

		foreach (StoredSpriteLayer layer in sprite.Layers)
		{
			layer.Id = Guid.NewGuid();
			layer.StoredSpriteId = sprite.Id;
		}

		context.Sprites.Add(sprite);
		context.SaveChanges();
		return sprite.Id;
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
				Layers = sprite.Layers.OrderBy(layer => layer.Order).ToList()
			})
			.OrderBy(sprite => sprite.Name)
			.ToList();
	}
}
