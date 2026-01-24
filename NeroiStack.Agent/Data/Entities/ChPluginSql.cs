namespace NeroiStack.Agent.Data.Entities;

public class ChPluginSql
{
	public int Id { get; set; }

	public int PluginId { get; set; }

	public string? Provider { get; set; } = string.Empty; // SQLite, PostgreSQL, MySQL

	public string? ConnectionString { get; set; } = string.Empty;
}
