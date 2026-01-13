using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Interface;

public interface IChatManageService
{
	Task<List<ChatVM>> GetAsync();
	Task<ChatVM> GetByIdAsync(int chatId);
	Task<ChatVM> CreateAsync(CreateChatRequest createChatRequest);
	Task<ChatVM> UpdateAsync(ChatVM chatVm);
	Task DeleteAsync(int chatId);
}