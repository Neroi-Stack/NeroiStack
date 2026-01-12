namespace NeroiStack.Agent.Data.Entities;

public class ChPluginMcpHttp
{
	public int Id { get; set; }
	public int PluginId { get; set; }
	public string? Endpoint { get; set; }
	public string? ApiKey { get; set; }
}