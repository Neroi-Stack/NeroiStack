namespace NeroiStack.Agent.Model;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Agents;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;

public class ChatSession
{
	public int ChatInstanceId { get; set; }
	public Kernel Kernel { get; set; } = null!;
	public SupplierEnum? CurrentSupplier { get; set; }
	public string? CurrentModelName { get; set; }
	public List<ChatCompletionAgent> Agents { get; set; } = [];
	public ChatHistory History { get; set; } = new();
	public List<object> Resources { get; set; } = [];
	public AgentOrchestrationType OrchestrationType { get; set; }
	public ChChat ChatConfig { get; set; } = null!;
}
