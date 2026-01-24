using Microsoft.EntityFrameworkCore;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Interface;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Service;

public class AgentManageService(IChatContext chatContext) : IAgentManageService
{
	public async Task<List<AgentVM>> GetAsync()
	{
		var agents = await chatContext.Agents.Select(agent => (AgentVM)agent).ToListAsync();
		var agentIds = agents.Select(a => a.Id).ToList();
		var plugins = await chatContext.AgentPlugins.Where(ap => agentIds.Contains(ap.AgentId)).ToListAsync();

		foreach (var agent in agents)
		{
			agent.PluginIds = plugins.Where(p => p.AgentId == agent.Id).Select(p => p.PluginId).ToArray();
		}
		return agents;
	}

	public async Task<AgentVM> GetByIdAsync(int agentId)
	{
		var agent = await chatContext.Agents.FindAsync(agentId) ?? throw new Exception($"Agent with ID {agentId} not found.");
		var vm = (AgentVM)agent;
		vm.PluginIds = await chatContext.AgentPlugins.Where(ap => ap.AgentId == agentId).Select(ap => ap.PluginId).ToArrayAsync();
		return vm;
	}

	public async Task<AgentVM> CreateAsync(CreateAgentRequest agentRequest)
	{
		var agent = new ChAgent
		{
			Name = agentRequest.Name,
			Description = agentRequest.Description,
			Instructions = agentRequest.Instructions,
			Temperature = agentRequest.Temperature,
			TopP = agentRequest.TopP,
			TopK = agentRequest.TopK,
			MaxTokens = agentRequest.MaxTokens,
			PresencePenalty = agentRequest.PresencePenalty,
			FrequencyPenalty = agentRequest.FrequencyPenalty,
			Seed = agentRequest.Seed,
			StopSequences = agentRequest.StopSequences,
			ResponseFormat = agentRequest.ResponseFormat,
			PromptTemplate = agentRequest.PromptTemplate,
			IsEnabled = agentRequest.IsEnabled
		};
		await chatContext.Agents.AddAsync(agent);
		await chatContext.SaveChangesAsync();
		await chatContext.AgentPlugins.AddRangeAsync(agentRequest.PluginIds.Select(pluginId => new ChAgentPlugin
		{
			AgentId = agent.Id,
			PluginId = pluginId
		}));
		await chatContext.SaveChangesAsync();
		return (AgentVM)agent;
	}

	public async Task UpdateAsync(AgentVM agentVm)
	{
		var agent = await chatContext.Agents.FindAsync(agentVm.Id) ?? throw new Exception($"Agent with ID {agentVm.Id} not found.");
		agent.Name = agentVm.Name;
		agent.Description = agentVm.Description;
		agent.Instructions = agentVm.Instructions;
		agent.Temperature = agentVm.Temperature;
		agent.TopP = agentVm.TopP;
		agent.TopK = agentVm.TopK;
		agent.MaxTokens = agentVm.MaxTokens;
		agent.PresencePenalty = agentVm.PresencePenalty;
		agent.FrequencyPenalty = agentVm.FrequencyPenalty;
		agent.Seed = agentVm.Seed;
		agent.StopSequences = agentVm.StopSequences;
		agent.ResponseFormat = agentVm.ResponseFormat;
		agent.PromptTemplate = agentVm.PromptTemplate;
		agent.IsEnabled = agentVm.IsEnabled;
		await chatContext.SaveChangesAsync();
		var agentPluginIds = await chatContext.AgentPlugins.Where(at => at.AgentId == agentVm.Id).Select(at => at.PluginId).ToListAsync();
		var toAdd = agentVm.PluginIds.Except(agentPluginIds);
		var toRemove = agentPluginIds.Except(agentVm.PluginIds);
		if (toAdd.Any())
		{
			await chatContext.AgentPlugins.AddRangeAsync(toAdd.Select(pluginId => new ChAgentPlugin
			{
				AgentId = agentVm.Id,
				PluginId = pluginId
			}));
		}
		if (toRemove.Any())
		{
			var toRemoveEntities = await chatContext.AgentPlugins.Where(at => at.AgentId == agentVm.Id && toRemove.Contains(at.PluginId)).ToListAsync();
			chatContext.AgentPlugins.RemoveRange(toRemoveEntities);
		}
		await chatContext.SaveChangesAsync();
	}

	public async Task DeleteAsync(int agentId)
	{
		var agent = await chatContext.Agents.FindAsync(agentId) ?? throw new Exception($"Agent with ID {agentId} not found.");
		chatContext.Agents.Remove(agent);
		await chatContext.SaveChangesAsync();
	}

	public async Task<List<AgentDropdownVM>> GetDropdownAsync()
		=> await chatContext.Agents.Where(x => x.IsEnabled).Select(agent => (AgentDropdownVM)agent).ToListAsync();
}