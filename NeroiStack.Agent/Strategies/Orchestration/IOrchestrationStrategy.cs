namespace NeroiStack.Agent.Strategies.Orchestration;

using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;
using NeroiStack.Agent.Data;
using System.Threading.Tasks;

public interface IOrchestrationStrategy
{
	bool CanHandle(AgentOrchestrationType type);
	Task<string> ExecuteAsync(ChatSession session, InvokeChatRequest request, IChatContext chatContext);
}
