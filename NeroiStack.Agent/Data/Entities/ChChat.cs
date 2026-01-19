using NeroiStack.Agent.Enum;

namespace NeroiStack.Agent.Data.Entities;

public class ChChat
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public bool IsStreamable { get; set; } = true;
	public DateTime CreatedAt { get; set; }
	public AgentOrchestrationType AgentOrchestrationType { get; set; }
	public bool IsEnabled { get; set; } = true;
}