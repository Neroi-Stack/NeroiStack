using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Interface;

public interface IPluginManageService
{
	Task<List<PluginBaseVM>> GetPluginsAsync();
	Task<PluginBaseVM?> GetPluginAsync(int id);
	Task<int> CreatePluginAsync(PluginBaseVM plugin);
	Task UpdatePluginAsync(PluginBaseVM plugin);
	Task DeletePluginAsync(int id);
	Task<List<PluginBaseDropDownVM>> GetPluginsDropDownAsync();
}
