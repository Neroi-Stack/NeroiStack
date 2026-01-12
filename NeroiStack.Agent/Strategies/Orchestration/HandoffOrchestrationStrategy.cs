#pragma warning disable SKEXP0110, SKEXP0001
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.EntityFrameworkCore;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;
using SKAgent = Microsoft.SemanticKernel.Agents.Agent;

namespace NeroiStack.Agent.Strategies.Orchestration;

public class HandoffOrchestrationStrategy : IOrchestrationStrategy
{
	public bool CanHandle(AgentOrchestrationType type) => type == AgentOrchestrationType.Handoff;

	public async Task<string> ExecuteAsync(ChatSession session, InvokeChatRequest request, IChatContext chatContext)
	{
		// Fetch transitions here using DB Context
		var transitions = await chatContext.ChatHandoffs
			.Where(h => h.ChatId == session.ChatConfig.Id)
			.ToListAsync(request.Ct);

		// We need to map DB Agent ID to SK Agent instance.
		var instance = await chatContext.ChatInstances.FindAsync([session.ChatInstanceId], request.Ct);
		var agentDefs = await chatContext.ChatAgents
		   .Join(chatContext.Agents, ca => ca.AgentId, a => a.Id, (ca, a) => new { ca, a })
		   .Where(x => x.ca.ChatId == instance!.ChatId)
		   .OrderBy(x => x.ca.Order)
		   .ToListAsync(request.Ct);

		var skAgentMapById = new Dictionary<int, ChatCompletionAgent>();
		for (int i = 0; i < agentDefs.Count && i < session.Agents.Count; i++)
		{
			skAgentMapById[agentDefs[i].a.Id] = session.Agents[i];
		}

		var entryAgent = session.Agents.FirstOrDefault();
		// Find Primary
		var primaryDef = agentDefs.FirstOrDefault(x => x.ca.IsPrimary);
		if (primaryDef != null && skAgentMapById.TryGetValue(primaryDef.a.Id, out ChatCompletionAgent? value))
			entryAgent = value;

		if (entryAgent == null) throw new Exception("Handoff orchestration requires at least one agent.");

		var orchestrationHandoffs = OrchestrationHandoffs.StartWith(entryAgent);

		foreach (var t in transitions)
		{
			if (skAgentMapById.TryGetValue(t.FromAgentId, out var from) &&
				skAgentMapById.TryGetValue(t.ToAgentId, out var to))
			{
				orchestrationHandoffs.Add(from, to, t.Description ?? "");
			}
		}

		async ValueTask responseCallback(ChatMessageContent response)
		{
			if (request.OnChunk != null) await request.OnChunk(response.Content ?? "");
		}

		HandoffOrchestration orchestration = new(
			orchestrationHandoffs,
			session.Agents.Cast<SKAgent>().ToArray())
		{
			ResponseCallback = responseCallback
		};

		InProcessRuntime runtime = new();
		await runtime.StartAsync();

		var result = await orchestration.InvokeAsync(request.Text, runtime);
		var resultText = await result.GetValueAsync();

		await runtime.RunUntilIdleAsync();
		return resultText;
	}
}
