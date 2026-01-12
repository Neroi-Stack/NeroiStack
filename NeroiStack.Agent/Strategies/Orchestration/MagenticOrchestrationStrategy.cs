#pragma warning disable SKEXP0110, SKEXP0001
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Magentic;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;
using SKAgent = Microsoft.SemanticKernel.Agents.Agent;

namespace NeroiStack.Agent.Strategies.Orchestration;

public class MagenticOrchestrationStrategy : IOrchestrationStrategy
{
	public bool CanHandle(AgentOrchestrationType type) => type == AgentOrchestrationType.Magentic;

	public async Task<string> ExecuteAsync(ChatSession session, InvokeChatRequest request, IChatContext chatContext)
	{
		StandardMagenticManager manager = new StandardMagenticManager(
			session.Kernel.GetRequiredService<IChatCompletionService>(),
			new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
		{
			MaximumInvocationCount = 10,
		};

		async ValueTask responseCallback(ChatMessageContent response)
		{
			if (request.OnChunk != null) await request.OnChunk(response.Content ?? "");
		}

		MagenticOrchestration orchestration = new MagenticOrchestration(
			manager,
			session.Agents.Cast<SKAgent>().ToArray())
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
