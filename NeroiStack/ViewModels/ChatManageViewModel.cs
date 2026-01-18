using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Interface;
using NeroiStack.Agent.Model;
using CommunityToolkit.Mvvm.Messaging;
using NeroiStack.Messages;

namespace NeroiStack.ViewModels;

public sealed partial class ChatManageViewModel : ViewModelBase
{
	private readonly IChatManageService _chatService;
	private readonly IAgentManageService _agentService;
	private readonly IChatInstanceService _instanceService;

	[ObservableProperty]
	private ObservableCollection<ChatVM> _chats = new();

	[ObservableProperty]
	private ObservableCollection<ChatAgentSelectionViewModel> _availableAgents = new();

	[ObservableProperty]
	private ObservableCollection<ConfiguredAgentViewModel> _configuredAgents = new();

	[ObservableProperty]
	private ObservableCollection<ConfiguredHandoffViewModel> _configuredHandoffs = new();

	[ObservableProperty]
	private ChatVM? _currentChat;

	[ObservableProperty]
	private bool _isModalOpen;

	[ObservableProperty]
	private bool _isEditorVisible;

	[ObservableProperty]
	private bool _isDeleteConfirmVisible;

	[ObservableProperty]
	private string _modalTitle = string.Empty;

	[ObservableProperty]
	private string _validationMessage = string.Empty;

	[ObservableProperty]
	private bool _isValidationMessageVisible;

	[ObservableProperty]
	private bool _isCreating;

	public AgentOrchestrationType[] AgentOrchestrationTypes => Enum.GetValues<AgentOrchestrationType>();

	// Helper properties for UI binding to switch views
	public AgentOrchestrationType SelectedOrchestrationType
	{
		get => CurrentChat?.AgentOrchestrationType ?? AgentOrchestrationType.Single;
		set
		{
			if (CurrentChat != null && CurrentChat.AgentOrchestrationType != value)
			{
				CurrentChat.AgentOrchestrationType = value;
				OnPropertyChanged();
				NotifyModeChanged();
				ResetAgentsToDefault();
			}
		}
	}

	private void ResetAgentsToDefault()
	{
		ConfiguredAgents.Clear();
		ConfiguredAgents.Add(new ConfiguredAgentViewModel(this, AvailableAgents) { Order = 0 });
		ConfiguredHandoffs.Clear();
	}

	public bool IsSingleMode => SelectedOrchestrationType == AgentOrchestrationType.Single;
	public bool IsSequentialMode => SelectedOrchestrationType == AgentOrchestrationType.Sequential;
	public bool IsConcurrentMode => SelectedOrchestrationType == AgentOrchestrationType.Concurrent;
	public bool IsGroupChatMode => SelectedOrchestrationType == AgentOrchestrationType.GroupChat;
	public bool IsMagenticMode => SelectedOrchestrationType == AgentOrchestrationType.Magentic;
	public bool IsHandoffMode => SelectedOrchestrationType == AgentOrchestrationType.Handoff;

	partial void OnCurrentChatChanged(ChatVM? value)
	{
		OnPropertyChanged(nameof(SelectedOrchestrationType));
		NotifyModeChanged();
	}

	private void NotifyModeChanged()
	{
		OnPropertyChanged(nameof(IsSingleMode));
		OnPropertyChanged(nameof(IsSequentialMode));
		OnPropertyChanged(nameof(IsConcurrentMode));
		OnPropertyChanged(nameof(IsGroupChatMode));
		OnPropertyChanged(nameof(IsMagenticMode));
		OnPropertyChanged(nameof(IsHandoffMode));
	}

	public ChatManageViewModel(IChatManageService chatService, IAgentManageService agentService, IChatInstanceService instanceService)
	{
		_chatService = chatService;
		_agentService = agentService;
		_instanceService = instanceService;
		LoadChatsCommand.Execute(null);
	}

	// Default constructor for Design Time
	public ChatManageViewModel() : this(null!, null!, null!) { }

	[RelayCommand]
	private async Task LoadChatsAsync()
	{
		if (_chatService == null) return;
		var list = await _chatService.GetAsync();
		Chats.Clear();
		foreach (var c in list)
		{
			Chats.Add(c);
		}
	}

	private async Task LoadEditorDataAsync(ChatVM? chat)
	{
		if (_agentService == null) return;

		var allAgents = await _agentService.GetDropdownAsync();

		AvailableAgents.Clear();
		foreach (var a in allAgents)
		{
			AvailableAgents.Add(new ChatAgentSelectionViewModel
			{
				Id = a.Id,
				Name = a.Name ?? "Unknown"
			});
		}

		AvailableAgents.Add(new ChatAgentSelectionViewModel { Id = -1, Name = "➕ Create Agent" });

		ConfiguredAgents.Clear();
		ConfiguredHandoffs.Clear();

		if (chat != null)
		{
			if (chat.Agents != null && chat.Agents.Any())
			{
				foreach (var ea in chat.Agents.OrderBy(x => x.Order))
				{
					var selection = AvailableAgents.FirstOrDefault(x => x.Id == ea.AgentId);
					var vm = new ConfiguredAgentViewModel(this, AvailableAgents)
					{
						IsPrimary = ea.IsPrimary,
						Order = ea.Order,
						SelectedAgent = selection
					};
					ConfiguredAgents.Add(vm);
				}
			}
			else
			{
				ConfiguredAgents.Add(new ConfiguredAgentViewModel(this, AvailableAgents) { Order = 0 });
			}

			if (chat.Handoffs != null)
			{
				foreach (var h in chat.Handoffs)
				{
					var fromAgent = AvailableAgents.FirstOrDefault(x => x.Id == h.FromAgentId);
					var toAgent = AvailableAgents.FirstOrDefault(x => x.Id == h.ToAgentId);
					var vm = new ConfiguredHandoffViewModel(this, AvailableAgents)
					{
						SelectedFromAgent = fromAgent,
						SelectedToAgent = toAgent,
						Description = h.Description ?? ""
					};
					ConfiguredHandoffs.Add(vm);
				}
			}
		}
		else
		{
			// Default to one empty block for new chats or empty list
			ConfiguredAgents.Add(new ConfiguredAgentViewModel(this, AvailableAgents) { Order = 0 });
		}
	}

	[RelayCommand]
	private void AddAgent()
	{
		ConfiguredAgents.Add(new ConfiguredAgentViewModel(this, AvailableAgents)
		{
			Order = ConfiguredAgents.Count
		});
	}

	[RelayCommand]
	private void AddHandoff()
	{
		ConfiguredHandoffs.Add(new ConfiguredHandoffViewModel(this, AvailableAgents));
	}

	[RelayCommand]
	public void RemoveAgent(ConfiguredAgentViewModel agent)
	{
		ConfiguredAgents.Remove(agent);
		ReorderAgents();
	}

	[RelayCommand]
	public void RemoveHandoff(ConfiguredHandoffViewModel handoff)
	{
		ConfiguredHandoffs.Remove(handoff);
	}

	// ToggleAgent removed as we use direct add/remove now

	public void MoveAgentUp(ConfiguredAgentViewModel agent)
	{
		var index = ConfiguredAgents.IndexOf(agent);
		if (index > 0)
		{
			ConfiguredAgents.Move(index, index - 1);
			ReorderAgents();
		}
	}

	public void MoveAgentDown(ConfiguredAgentViewModel agent)
	{
		var index = ConfiguredAgents.IndexOf(agent);
		if (index < ConfiguredAgents.Count - 1)
		{
			ConfiguredAgents.Move(index, index + 1);
			ReorderAgents();
		}
	}

	private void ReorderAgents()
	{
		for (int i = 0; i < ConfiguredAgents.Count; i++)
		{
			ConfiguredAgents[i].Order = i;
		}
	}

	[RelayCommand]
	private async Task CreateInstanceAsync(ChatVM chat)
	{
		if (chat == null) return;

		try
		{
			var instance = await _instanceService.CreateInstanceAsync(chat.Id);

			WeakReferenceMessenger.Default.Send(new AddChatInstanceMessage(new ChatInstanceInfo
			{
				ChatId = chat.Id,
				Name = chat.Name ?? "New Chat",
				InstanceId = instance.Id
			}));
		}
		catch (Exception ex)
		{
			// TODO: Handle error
			Console.WriteLine(ex.Message);
		}
	}

	[RelayCommand]
	private async Task CreateAsync()
	{
		IsCreating = true;
		ModalTitle = "New Chat Class";
		CurrentChat = new ChatVM { Name = "New Chat", IsEnabled = true, AgentOrchestrationType = AgentOrchestrationType.Single };
		await LoadEditorDataAsync(null);
		OpenEditor();
	}

	[RelayCommand]
	private async Task EditAsync(ChatVM chat)
	{
		IsCreating = false;
		ModalTitle = "Edit Chat Class";

		try
		{
			var fullChat = await _chatService.GetByIdAsync(chat.Id);
			CurrentChat = fullChat;
			await LoadEditorDataAsync(fullChat);
		}
		catch
		{
			CurrentChat = chat;
			await LoadEditorDataAsync(chat);
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
	private void Delete(ChatVM chat)
	{
		CurrentChat = chat;
		IsEditorVisible = false;
		IsDeleteConfirmVisible = true;
		IsModalOpen = true;
	}

	[RelayCommand]
	private async Task ConfirmDeleteAsync()
	{
		if (CurrentChat != null)
		{
			await _chatService.DeleteAsync(CurrentChat.Id);
			Chats.Remove(CurrentChat);
		}
		CloseModal();
	}

	[RelayCommand]
	private async Task ToggleEnableAsync(ChatVM chat)
	{
		var updatedChat = await _chatService.UpdateAsync(chat);
		if (updatedChat != null)
		{
			var existing = Chats.FirstOrDefault(c => c.Id == updatedChat.Id);
			if (existing != null)
			{
				var idx = Chats.IndexOf(existing);
				if (idx >= 0) Chats[idx] = updatedChat;
			}
		}
	}

	private bool ValidateInput()
	{
		ValidationMessage = string.Empty;
		IsValidationMessageVisible = false;

		// Common validation: Check null agents
		if (ConfiguredAgents.Any(x => x.SelectedAgent == null))
		{
			ValidationMessage = "Please select an agent for all configured slots.";
			IsValidationMessageVisible = true;
			return false;
		}

		// Mode specific validation
		switch (SelectedOrchestrationType)
		{
			case AgentOrchestrationType.Single:
				if (ConfiguredAgents.Count < 1)
				{
					ValidationMessage = "Single Agent mode requires at least one agent.";
					IsValidationMessageVisible = true;
					return false;
				}
				// Only first one is used, but allowing multiple is confusing.
				if (ConfiguredAgents.Count > 1)
				{
					ValidationMessage = "Single Agent mode should strictly have only one agent. Please remove others.";
					IsValidationMessageVisible = true;
					return false;
				}
				break;

			case AgentOrchestrationType.Sequential:
			case AgentOrchestrationType.Concurrent:
			case AgentOrchestrationType.GroupChat:
			case AgentOrchestrationType.Magentic:
				if (ConfiguredAgents.Count < 1)
				{
					ValidationMessage = "This mode requires at least one participating agent.";
					IsValidationMessageVisible = true;
					return false;
				}
				break;

			case AgentOrchestrationType.Handoff:
				if (ConfiguredAgents.Count < 1)
				{
					ValidationMessage = "Handoff mode requires participating agents.";
					IsValidationMessageVisible = true;
					return false;
				}
				if (ConfiguredAgents.Count(x => x.IsPrimary) != 1)
				{
					ValidationMessage = "Handoff mode requires exactly one 'Entry Point' agent.";
					IsValidationMessageVisible = true;
					return false;
				}

				if (ConfiguredHandoffs.Any(x => x.SelectedFromAgent == null || x.SelectedToAgent == null))
				{
					ValidationMessage = "Please complete all handoff transitions.";
					IsValidationMessageVisible = true;
					return false;
				}
				break;
		}

		return true;
	}

	[RelayCommand]
	private async Task SaveAsync()
	{
		if (CurrentChat == null) return;

		if (!ValidateInput()) return;

		// Re-number orders just to be safe
		ReorderAgents();

		var agentsToSave = ConfiguredAgents
			.Where(ca => ca.Id > 0) // Filter out unselected which theoretically caught by validate but safe check
			.Select(ca => new ChatAgentVM
			{
				AgentId = ca.Id,
				// Primary is forced to false for modes that don't support IsPrimary property in logic
				// But Handoff checks IsPrimary. GroupChat doesn't use it in existing logic. Single uses first.
				// We preserve it mainly for Handoff.
				IsPrimary = IsHandoffMode ? ca.IsPrimary : false,
				Order = ca.Order
			}).ToList();

		var handoffsToSave = ConfiguredHandoffs
			.Where(h => h.SelectedFromAgent != null && h.SelectedToAgent != null)
			.Select(h => new ChatHandoffVM
			{
				FromAgentId = h.SelectedFromAgent!.Id,
				ToAgentId = h.SelectedToAgent!.Id,
				Description = h.Description
			}).ToList();

		CurrentChat.Agents = agentsToSave;
		CurrentChat.Handoffs = handoffsToSave;

		if (IsCreating)
		{
			var request = new CreateChatRequest
			{
				Name = CurrentChat.Name,
				IsEnabled = CurrentChat.IsEnabled,
				AgentOrchestrationType = CurrentChat.AgentOrchestrationType,
				Agents = agentsToSave,
				Handoffs = handoffsToSave
			};
			var newChat = await _chatService.CreateAsync(request);
			Chats.Add(newChat);
		}
		else
		{
			await _chatService.UpdateAsync(CurrentChat);
			// Refresh in list
			var index = Chats.IndexOf(Chats.FirstOrDefault(c => c.Id == CurrentChat.Id)!);
			if (index >= 0)
			{
				Chats[index] = CurrentChat;
			}
		}
		CloseModal();
	}

	[RelayCommand]
	public void CloseModal()
	{
		IsModalOpen = false;
		IsEditorVisible = false;
		IsDeleteConfirmVisible = false;
		CurrentChat = null;
		AvailableAgents.Clear();
		ConfiguredAgents.Clear();
		ConfiguredHandoffs.Clear();
	}
}

public partial class ConfiguredHandoffViewModel : ObservableObject
{
	private readonly ChatManageViewModel _parent;

	public ObservableCollection<ChatAgentSelectionViewModel> AvailableList { get; }

	public ConfiguredHandoffViewModel(ChatManageViewModel parent, ObservableCollection<ChatAgentSelectionViewModel> availableList)
	{
		_parent = parent;
		AvailableList = availableList;
	}

	[ObservableProperty]
	private ChatAgentSelectionViewModel? _selectedFromAgent;

	partial void OnSelectedFromAgentChanged(ChatAgentSelectionViewModel? value)
	{
		if (value != null && value.Id == -1)
		{
			_parent.CloseModal();
			WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(Views.AgentManageView)));
		}
	}

	[ObservableProperty]
	private ChatAgentSelectionViewModel? _selectedToAgent;

	partial void OnSelectedToAgentChanged(ChatAgentSelectionViewModel? value)
	{
		if (value != null && value.Id == -1)
		{
			_parent.CloseModal();
			WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(Views.AgentManageView)));
		}
	}

	[ObservableProperty]
	private string _description = string.Empty;

	[RelayCommand]
	private void Remove() => _parent.RemoveHandoff(this);
}

public partial class ChatAgentSelectionViewModel : ObservableObject
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;

	// IsSelected is no longer needed but kept for compatibility if referenced, though logic moved.
	[ObservableProperty]
	private bool _isSelected;

	// For ComboBox display
	public override string ToString() => Name;
}

public partial class ConfiguredAgentViewModel : ObservableObject
{
	private readonly ChatManageViewModel _parent;

	public ObservableCollection<ChatAgentSelectionViewModel> AvailableList { get; }

	public ConfiguredAgentViewModel(ChatManageViewModel parent, ObservableCollection<ChatAgentSelectionViewModel> availableList)
	{
		_parent = parent;
		AvailableList = availableList;
	}

	[ObservableProperty]
	private int _id;

	[ObservableProperty]
	private string _name = string.Empty;

	[ObservableProperty]
	private ChatAgentSelectionViewModel? _selectedAgent;

	partial void OnSelectedAgentChanged(ChatAgentSelectionViewModel? value)
	{
		if (value != null)
		{
			if (value.Id == -1) // Create Agent
			{
				_parent.CloseModal();
				WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(Views.AgentManageView)));
				return;
			}

			Id = value.Id;
			Name = value.Name;
		}
	}

	[ObservableProperty]
	private bool _isPrimary;

	[ObservableProperty]
	private int _order;

	[RelayCommand]
	private void MoveUp() => _parent.MoveAgentUp(this);

	[RelayCommand]
	private void MoveDown() => _parent.MoveAgentDown(this);

	[RelayCommand]
	private void Remove() => _parent.RemoveAgent(this);
}
