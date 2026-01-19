#pragma warning disable SKEXP0110, SKEXP0001
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;
using SKAgent = Microsoft.SemanticKernel.Agents.Agent;

namespace NeroiStack.Agent.Strategies.Orchestration;

public class GroupChatOrchestrationStrategy : IOrchestrationStrategy
{
	public bool CanHandle(AgentOrchestrationType type) => type == AgentOrchestrationType.GroupChat;

	public async Task<string> ExecuteAsync(ChatSession session, InvokeChatRequest request, IChatContext chatContext)
	{
		async ValueTask responseCallback(ChatMessageContent response)
		{
			if (request.OnChunk != null) await request.OnChunk(response.Content ?? "");
		}

		GroupChatOrchestration orchestration = new(
		   new RoundRobinGroupChatManager
		   {
			   MaximumInvocationCount = 10,
		   },
		   [.. session.Agents.Cast<SKAgent>()]
		)
		{
			ResponseCallback = responseCallback,
		};

		InProcessRuntime runtime = new();
		await runtime.StartAsync();

		var result = await orchestration.InvokeAsync(request.Text, runtime);
		var resultText = await result.GetValueAsync();

		await runtime.RunUntilIdleAsync();
		return resultText;
	}
}
