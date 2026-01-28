using Godot;
using System;
using Microsoft.EntityFrameworkCore;

public class GameData
{
	public int Id { get; set; }
	public string PlayerName { get; set; } = "";
	public int Score { get; set; }
}

public class GameDbContext : DbContext
{
	public DbSet<GameData> Players { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder options)
	{
		options.UseSqlite("Data Source=user://game_data.db");
	}
}
public partial class EntityFrameworkManagement : managerNode
{
	



}
