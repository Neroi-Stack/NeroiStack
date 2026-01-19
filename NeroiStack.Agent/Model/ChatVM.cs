using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;

namespace NeroiStack.Agent.Model;

public class ChatVM
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public bool IsStreamable { get; set; } = true;
	public DateTime CreatedAt { get; set; }
	public bool IsEnabled { get; set; }
	public AgentOrchestrationType AgentOrchestrationType { get; set; }
	public List<ChatAgentVM> Agents { get; set; } = [];
	public List<ChatHandoffVM> Handoffs { get; set; } = [];

	public static explicit operator ChatVM(ChChat chat) => new()
	{
		Id = chat.Id,
		Name = chat.Name,
		IsStreamable = chat.IsStreamable,
		CreatedAt = chat.CreatedAt,
		IsEnabled = chat.IsEnabled,
		AgentOrchestrationType = chat.AgentOrchestrationType
	};
}

public class ChatHandoffVM
{
	public int FromAgentId { get; set; }
	public int ToAgentId { get; set; }
	public string? Description { get; set; }
}