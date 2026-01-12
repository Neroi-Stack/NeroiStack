using NeroiStack.Agent.Enum;

namespace NeroiStack.Agent.Data.Entities;

public class ChChat
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public DateTime CreatedAt { get; set; }
	public AgentOrchestrationType AgentOrchestrationType { get; set; }
	public bool IsEnabled { get; set; } = true;
}