using NeroiStack.Agent.Enum;

namespace NeroiStack.Agent.Data.Entities;

public class ChPlugin
{
	public int Id { get; set; }
	public PluginType Type { get; set; }
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? Source { get; set; }
	public bool IsEnabled { get; set; }
}