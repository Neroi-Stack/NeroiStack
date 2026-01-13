
using Microsoft.EntityFrameworkCore;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Interface;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Service;

public class ChatManageService(IChatContext chatContext) : IChatManageService
{
	public async Task<List<ChatVM>> GetAsync()
	{
		var chats = await chatContext.Chats.Select(c => (ChatVM)c).ToListAsync();
		var chatIds = chats.Select(c => c.Id).ToList();
		var chatAgents = await chatContext.ChatAgents.Where(ca => chatIds.Contains(ca.ChatId)).ToListAsync();

		foreach (var chat in chats)
		{
			chat.Agents = chatAgents.Where(ca => ca.ChatId == chat.Id)
				.Select(ca => new ChatAgentVM
				{
					AgentId = ca.AgentId,
					Order = ca.Order,
					IsPrimary = ca.IsPrimary
				})
				.OrderBy(ca => ca.Order)
				.ToList();
		}
		return chats;
	}

	public async Task<ChatVM> CreateAsync(CreateChatRequest createChatRequest)
	{
		var chat = new ChChat
		{
			Name = createChatRequest.Name,
			IsEnabled = createChatRequest.IsEnabled,
			AgentOrchestrationType = createChatRequest.AgentOrchestrationType
		};
		await chatContext.Chats.AddAsync(chat);
		await chatContext.SaveChangesAsync();

		if (createChatRequest.Agents != null && createChatRequest.Agents.Any())
		{
			await chatContext.ChatAgents.AddRangeAsync(createChatRequest.Agents.Select(a => new ChChatAgent
			{
				ChatId = chat.Id,
				AgentId = a.AgentId,
				Order = a.Order,
				IsPrimary = a.IsPrimary
			}));
			await chatContext.SaveChangesAsync();
		}

		var vm = (ChatVM)chat;
		vm.Agents = createChatRequest.Agents ?? [];
		return vm;
	}

	public async Task<ChatVM> GetByIdAsync(int chatId)
	{
		var chat = await chatContext.Chats.FindAsync(chatId) ?? throw new Exception($"Chat with ID {chatId} not found.");
		var vm = (ChatVM)chat;
		vm.Agents = await chatContext.ChatAgents
			.Where(ca => ca.ChatId == chatId)
			.OrderBy(ca => ca.Order)
			.Select(ca => new ChatAgentVM
			{
				AgentId = ca.AgentId,
				Order = ca.Order,
				IsPrimary = ca.IsPrimary
			})
			.ToListAsync();

		vm.Handoffs = await chatContext.ChatHandoffs
			.Where(h => h.ChatId == chatId)
			.Select(h => new ChatHandoffVM
			{
				FromAgentId = h.FromAgentId,
				ToAgentId = h.ToAgentId,
				Description = h.Description
			})
			.ToListAsync();

		return vm;
	}

	public async Task<ChatVM> UpdateAsync(ChatVM chatVm)
	{
		var chat = await chatContext.Chats.FindAsync(chatVm.Id) ?? throw new Exception($"Chat with ID {chatVm.Id} not found.");
		chat.Name = chatVm.Name;
		chat.IsEnabled = chatVm.IsEnabled;
		chat.AgentOrchestrationType = chatVm.AgentOrchestrationType;
		await chatContext.SaveChangesAsync();

		// Full replacement strategy for agents to handle order/primary changes easily
		var currentAgents = await chatContext.ChatAgents.Where(ca => ca.ChatId == chatVm.Id).ToListAsync();
		chatContext.ChatAgents.RemoveRange(currentAgents);

		if (chatVm.Agents != null && chatVm.Agents.Any())
		{
			await chatContext.ChatAgents.AddRangeAsync(chatVm.Agents.Select(a => new ChChatAgent
			{
				ChatId = chatVm.Id,
				AgentId = a.AgentId,
				Order = a.Order,
				IsPrimary = a.IsPrimary
			}));
		}

		// Update Handoffs
		var currentHandoffs = await chatContext.ChatHandoffs.Where(h => h.ChatId == chatVm.Id).ToListAsync();
		chatContext.ChatHandoffs.RemoveRange(currentHandoffs);

		if (chatVm.Handoffs != null && chatVm.Handoffs.Any())
		{
			await chatContext.ChatHandoffs.AddRangeAsync(chatVm.Handoffs.Select(h => new ChChatHandoff
			{
				ChatId = chatVm.Id,
				FromAgentId = h.FromAgentId,
				ToAgentId = h.ToAgentId,
				Description = h.Description
			}));
		}

		await chatContext.SaveChangesAsync();
		return chatVm;
	}

	public async Task DeleteAsync(int chatId)
	{
		var chat = await chatContext.Chats.FindAsync(chatId) ?? throw new Exception($"Chat with ID {chatId} not found.");
		chatContext.Chats.Remove(chat);
		await chatContext.SaveChangesAsync();
	}
}