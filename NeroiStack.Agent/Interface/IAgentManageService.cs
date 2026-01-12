using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Interface;

public interface IAgentManageService
{
	Task<List<AgentVM>> GetAsync();
	Task<AgentVM> GetByIdAsync(int agentId);
	Task<AgentVM> CreateAsync(CreateAgentRequest agentRequest);
	Task UpdateAsync(AgentVM agent);
	Task DeleteAsync(int agentId);
	Task<List<AgentDropdownVM>> GetDropdownAsync();
}