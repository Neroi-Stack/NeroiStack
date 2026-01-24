using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Interface;

public interface IKeyManageService
{
	Task SaveKeyAsync(KeyVM keyVM);
	Task<List<KeyVM>> GetAllKeysAsync();
	Task DeleteKeyAsync(Guid id);
}