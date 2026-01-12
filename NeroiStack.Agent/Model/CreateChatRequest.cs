using NeroiStack.Agent.Enum;

namespace NeroiStack.Agent.Model;

public class CreateChatRequest
{
	public string? Name { get; set; }
	public bool IsEnabled { get; set; } = true;
	public AgentOrchestrationType AgentOrchestrationType { get; set; }
	public List<ChatAgentVM> Agents { get; set; } = [];
	public List<ChatHandoffVM> Handoffs { get; set; } = [];
}