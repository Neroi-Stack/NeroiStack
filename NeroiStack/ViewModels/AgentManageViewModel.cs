using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NeroiStack.Agent.Interface;
using NeroiStack.Agent.Model;
using NeroiStack.Messages;

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
		CurrentAgent = new AgentVM { Name = "New Agent", Description = "Description", Instructions = "You are a helpful assistant.", Kernel = "LLM", IsEnabled = true };
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
		CloseModal();
		WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(Views.PluginManageView)));
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
				Kernel = CurrentAgent.Kernel,
				Temperature = CurrentAgent.Temperature,
				TopP = CurrentAgent.TopP,
				MaxTokens = CurrentAgent.MaxTokens,
				PresencePenalty = CurrentAgent.PresencePenalty,
				FrequencyPenalty = CurrentAgent.FrequencyPenalty,
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