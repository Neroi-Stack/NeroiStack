using Microsoft.SemanticKernel;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies.Orchestration;

public class SingleOrchestrationStrategy : IOrchestrationStrategy
{
	public bool CanHandle(AgentOrchestrationType type) => type == AgentOrchestrationType.Single;

	public async Task<string> ExecuteAsync(ChatSession session, InvokeChatRequest request, IChatContext chatContext)
	{
		var agent = session.Agents.FirstOrDefault() ?? throw new Exception("No agent");
		string resultText = string.Empty;
		await foreach (StreamingChatMessageContent content in agent.InvokeStreamingAsync(session.History, cancellationToken: request.Ct))
		{
			resultText += content.Content;
			if (request.OnChunk != null) await request.OnChunk(content.Content ?? "");
		}
		return resultText;
	}
}
