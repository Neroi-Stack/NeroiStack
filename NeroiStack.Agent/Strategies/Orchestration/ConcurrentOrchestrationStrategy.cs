#pragma warning disable SKEXP0110, SKEXP0001
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;
using SKAgent = Microsoft.SemanticKernel.Agents.Agent;

namespace NeroiStack.Agent.Strategies.Orchestration;

public class ConcurrentOrchestrationStrategy : IOrchestrationStrategy
{
	public bool CanHandle(AgentOrchestrationType type) => type == AgentOrchestrationType.Concurrent;

	public async Task<string> ExecuteAsync(ChatSession session, InvokeChatRequest request, IChatContext chatContext)
	{
		ConcurrentOrchestration orchestration = new(
			session.Agents.Cast<SKAgent>().ToArray());

		InProcessRuntime runtime = new();
		await runtime.StartAsync();

		var result = await orchestration.InvokeAsync(request.Text, runtime);
		var outputs = await result.GetValueAsync();
		var resultText = string.Join("\n\n", outputs);

		await runtime.RunUntilIdleAsync();
		return resultText;
	}
}
