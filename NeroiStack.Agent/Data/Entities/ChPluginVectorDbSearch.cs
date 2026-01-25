using NeroiStack.Agent.Enum;

namespace NeroiStack.Agent.Data.Entities;

public class ChPluginVectorDbSearch
{
	public int Id { get; set; }
	public int PluginId { get; set; }
	public string? EmbeddedKeyId { get; set; }
	public string? ModelName { get; set; }
	public int Dimension { get; set; }
	public VectorDbType DbType { get; set; }
}