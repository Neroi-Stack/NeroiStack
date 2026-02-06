using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Interface;
using NeroiStack.Agent.Model;

namespace NeroiStack.ViewModels;

public sealed partial class AgentManageViewModel : ViewModelBase
{
	private readonly IAgentManageService _agentService;
	private readonly IPluginManageService _pluginService;

	[ObservableProperty]
	private ObservableCollection<AgentVM> _agents = new();

	[ObservableProperty]
	private ObservableCollection<AgentPluginSelectionViewModel> _availablePlugins = new();

	[ObservableProperty]
	private AgentVM? _currentAgent;

	[ObservableProperty]
	private bool _isModalOpen;

	[ObservableProperty]
	private bool _isEditorVisible;

	[ObservableProperty]
	private bool _isDeleteConfirmVisible;

	[ObservableProperty]
	private string _modalTitle = string.Empty;

	[ObservableProperty]
	private bool _isCreating;

	[ObservableProperty]
	private bool _isPluginEditorVisible;

	[ObservableProperty]
	private PluginBaseVM? _currentPlugin;

	[ObservableProperty]
	private PluginMcpHttpVM? _currentMcpHttp;
	[ObservableProperty]
	private ChPluginMcpStdioVM? _currentMcpStdio;
	[ObservableProperty]
	private ChPluginOpenApiVM? _currentOpenApi;

	[ObservableProperty]
	private string _mcpStdioArguments = string.Empty;

	[ObservableProperty]
	private bool _isPluginCreating;

	[ObservableProperty]
	private PluginType _selectedPluginType = PluginType.McpHttp;

	public System.Collections.Generic.List<PluginType> PluginTypes { get; } = System.Enum.GetValues<PluginType>().Cast<PluginType>().ToList();

	public AgentManageViewModel(IAgentManageService agentService, IPluginManageService pluginService)
	{
		_agentService = agentService;
		_pluginService = pluginService;
		LoadAgentsCommand.Execute(null);
	}

	// Default constructor for Design Time
	public AgentManageViewModel() : this(null!, null!) { }

	[RelayCommand]
	private async Task LoadAgentsAsync()
	{
		if (_agentService == null) return;
		var list = await _agentService.GetAsync();
		Agents.Clear();
		foreach (var p in list)
		{
			Agents.Add(p);
		}
	}

	private async Task LoadPluginsForEditorAsync(int[]? selectedIds = null)
	{
		if (_pluginService == null) return;
		selectedIds ??= [];
		var plugins = await _pluginService.GetPluginsDropDownAsync();
		AvailablePlugins.Clear();
		foreach (var p in plugins)
		{
			AvailablePlugins.Add(new AgentPluginSelectionViewModel
			{
				Id = p.Id,
				Name = p.Name ?? "Unknown",
				IsSelected = selectedIds.Contains(p.Id)
			});
		}
	}

	[RelayCommand]
	private async Task CreateAsync()
	{
		IsCreating = true;
		ModalTitle = "New Agent";
		CurrentAgent = new AgentVM { Name = "New Agent", Description = "Description", Instructions = "You are a helpful assistant.", IsEnabled = true };
		await LoadPluginsForEditorAsync();
		OpenEditor();
	}

	[RelayCommand]
	private async Task EditAsync(AgentVM agent)
	{
		IsCreating = false;
		ModalTitle = "Edit Agent";

		try
		{
			var fullAgent = await _agentService.GetByIdAsync(agent.Id);
			CurrentAgent = fullAgent;
			await LoadPluginsForEditorAsync(fullAgent.PluginIds);
		}
		catch
		{
			CurrentAgent = agent;
			await LoadPluginsForEditorAsync(agent.PluginIds);
		}

		OpenEditor();
	}

	private void OpenEditor()
	{
		IsEditorVisible = true;
		IsDeleteConfirmVisible = false;
		IsModalOpen = true;
	}

	[RelayCommand]
	private void Delete(AgentVM agent)
	{
		CurrentAgent = agent;
		IsEditorVisible = false;
		IsDeleteConfirmVisible = true;
		IsModalOpen = true;
	}

	[RelayCommand]
	private void CreatePlugin()
	{
		IsPluginCreating = true;
		SelectedPluginType = PluginType.McpHttp;
		InitializeNewPlugin(SelectedPluginType);
		IsPluginEditorVisible = true;
		// Ensure modal background is open if not already (it should be since we are in Agent Edit)
		IsModalOpen = true;
	}

	partial void OnSelectedPluginTypeChanged(PluginType value)
	{
		if (IsPluginCreating)
		{
			InitializeNewPlugin(value);
		}
	}

	private void InitializeNewPlugin(PluginType type)
	{
		switch (type)
		{
			case PluginType.McpHttp:
				CurrentPlugin = new PluginMcpHttpVM { Type = PluginType.McpHttp, Name = "New MCP HTTP", IsEnabled = true };
				CurrentMcpHttp = (PluginMcpHttpVM)CurrentPlugin;
				break;
			case PluginType.McpStdio:
				CurrentPlugin = new ChPluginMcpStdioVM { Type = PluginType.McpStdio, Name = "New MCP Stdio", IsEnabled = true, Arguments = new System.Collections.Generic.List<string>() };
				CurrentMcpStdio = (ChPluginMcpStdioVM)CurrentPlugin;
				McpStdioArguments = string.Empty;
				break;
			case PluginType.OpenApi:
				CurrentPlugin = new ChPluginOpenApiVM { Type = PluginType.OpenApi, Name = "New OpenAPI", IsEnabled = true };
				CurrentOpenApi = (ChPluginOpenApiVM)CurrentPlugin;
				break;
		}
	}

	[RelayCommand]
	private void ClosePluginModal()
	{
		IsPluginEditorVisible = false;
		// Do not set IsModalOpen=false, because we return to Agent Editor
		CurrentPlugin = null;
	}

	[RelayCommand]
	private async Task SavePluginAsync()
	{
		if (CurrentPlugin == null) return;

		// Process fields back to model
		if (CurrentPlugin is ChPluginMcpStdioVM mcpStdio)
		{
			mcpStdio.Arguments = McpStdioArguments
				.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries)
				.ToList();
		}

		if (IsPluginCreating)
		{
			var id = await _pluginService.CreatePluginAsync(CurrentPlugin);
			CurrentPlugin.Id = id;
			// Refresh plugin list for agent editor

			// We want to add this plugin to the Checked list in Agent Editor?
			// Or just refresh the available list.
			var oldSelection = AvailablePlugins.Where(p => p.IsSelected).Select(p => p.Id).ToList();
			oldSelection.Add(id);
			await LoadPluginsForEditorAsync(oldSelection.ToArray());
		}
		// If editing (not implemented yet in this view, but for completeness)

		ClosePluginModal();
	}

	[RelayCommand]
	private async Task ConfirmDeleteAsync()
	{
		if (CurrentAgent != null)
		{
			await _agentService.DeleteAsync(CurrentAgent.Id);
			Agents.Remove(CurrentAgent);
		}
		CloseModal();
	}

	[RelayCommand]
	private async Task SaveAsync()
	{
		if (CurrentAgent == null) return;

		var selectedPluginIds = AvailablePlugins.Where(p => p.IsSelected).Select(p => p.Id).ToArray();
		CurrentAgent.PluginIds = selectedPluginIds;

		if (IsCreating)
		{
			var request = new CreateAgentRequest
			{
				Name = CurrentAgent.Name,
				Description = CurrentAgent.Description,
				Instructions = CurrentAgent.Instructions,
				Temperature = CurrentAgent.Temperature,
				TopP = CurrentAgent.TopP,
				TopK = CurrentAgent.TopK,
				MaxTokens = CurrentAgent.MaxTokens,
				PresencePenalty = CurrentAgent.PresencePenalty,
				FrequencyPenalty = CurrentAgent.FrequencyPenalty,
				Seed = CurrentAgent.Seed,
				StopSequences = CurrentAgent.StopSequences,
				ResponseFormat = CurrentAgent.ResponseFormat,
				PromptTemplate = CurrentAgent.PromptTemplate,
				PluginIds = selectedPluginIds,
				IsEnabled = true
			};
			var newAgent = await _agentService.CreateAsync(request);
			newAgent.PluginIds = selectedPluginIds;
			Agents.Add(newAgent);
		}
		else
		{
			await _agentService.UpdateAsync(CurrentAgent);
			// Refresh in list
			var index = Agents.IndexOf(Agents.FirstOrDefault(p => p.Id == CurrentAgent.Id)!);
			if (index >= 0)
			{
				Agents[index] = CurrentAgent;
			}
		}
		CloseModal();
	}

	[RelayCommand]
	private async Task ToggleEnableAsync(AgentVM agent)
	{
		// Property is already changed by UI binding, just save it
		await _agentService.UpdateAsync(agent);
	}

	[RelayCommand]
	private void CloseModal()
	{
		IsModalOpen = false;
		IsEditorVisible = false;
		IsDeleteConfirmVisible = false;
		CurrentAgent = null;
		AvailablePlugins.Clear();
	}
}

public partial class AgentPluginSelectionViewModel : ObservableObject
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;

	[ObservableProperty]
	private bool _isSelected;
}