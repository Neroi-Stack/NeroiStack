namespace NeroiStack.Agent.Data.Entities;

public class ChPluginOpenApi
{
	public int Id { get; set; }
	public int PluginId { get; set; }
	public int AuthType { get; set; }
	public string? ApiKey { get; set; }
	public string? BearerToken { get; set; }
	public string? Uri { get; set; }
	public string? FilePath { get; set; }
}