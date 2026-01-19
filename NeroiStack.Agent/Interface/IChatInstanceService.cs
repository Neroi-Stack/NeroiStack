using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Interface;

public interface IChatInstanceService
{
	Task<List<ChatInstanceVM>> GetInstancesAsync();
	Task<ChatInstanceVM> CreateInstanceAsync(int chatId);
	Task DeleteInstanceAsync(int instanceId);
	Task UpdateSelectedModelAsync(int instanceId, string selectedModel);
	Task UpdateChatInstanceNameAsync(int instanceId, string name);
}
