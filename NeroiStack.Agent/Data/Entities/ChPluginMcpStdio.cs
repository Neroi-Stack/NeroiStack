namespace NeroiStack.Agent.Data.Entities;

public class ChPluginMcpStdio
{
	public int Id { get; set; }
	public int PluginId { get; set; }
	public string? Command { get; set; }
	public List<string>? Arguments { get; set; }
}