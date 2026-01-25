using NeroiStack.Agent.Enum;

namespace NeroiStack.Agent.Data.Entities;

public class ChPluginVectorRedisSearch
{
	public int Id { get; set; }
	public int PluginVectorDbId { get; set; }
	public string? Host { get; set; }
	public int Port { get; set; }
}