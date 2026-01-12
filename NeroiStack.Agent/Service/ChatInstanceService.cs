using Microsoft.EntityFrameworkCore;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Interface;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Service;

public class ChatInstanceService(IChatContext context) : IChatInstanceService
{
	private readonly IChatContext _context = context;

	public async Task<List<ChatInstanceVM>> GetInstancesAsync()
	{
		var instances = await _context.ChatInstances
			.OrderByDescending(x => x.CreatedAt)
			.ToListAsync();

		var viewModels = new List<ChatInstanceVM>();
		foreach (var instance in instances)
		{
			var vm = (ChatInstanceVM)instance;
			// Get the Chat Name
			var chat = await _context.Chats.FindAsync(instance.ChatId);
			if (chat != null)
			{
				vm.Name = chat.Name ?? $"Chat {instance.ChatId}";
			}
			viewModels.Add(vm);
		}
		return viewModels;
	}

	public async Task<ChatInstanceVM> CreateInstanceAsync(int chatId)
	{
		var entity = new ChChatInstance
		{
			ChatId = chatId,
			CreatedAt = DateTime.Now
		};

		await _context.ChatInstances.AddAsync(entity);
		await _context.SaveChangesAsync();

		var vm = (ChatInstanceVM)entity;
		var chat = await _context.Chats.FindAsync(chatId);
		if (chat != null)
		{
			vm.Name = chat.Name ?? $"Chat {chatId}";
		}
		return vm;
	}

	public async Task DeleteInstanceAsync(int instanceId)
	{
		var entity = await _context.ChatInstances.FindAsync(instanceId);
		if (entity != null)
		{
			_context.ChatInstances.Remove(entity);
			await _context.SaveChangesAsync();
		}
	}

	public async Task UpdateSelectedModelAsync(int instanceId, string selectedModel)
	{
		var entity = await _context.ChatInstances.FindAsync(instanceId);
		if (entity != null)
		{
			entity.SelectedModel = selectedModel;
			await _context.SaveChangesAsync();
		}
	}
}
