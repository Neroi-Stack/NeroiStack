using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml.MarkupExtensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Interface;
using NeroiStack.Agent.Model;

namespace NeroiStack.ViewModels;

public sealed partial class PluginManageViewModel : ViewModelBase
{
	private readonly IPluginManageService _pluginService;

	[ObservableProperty]
	private ObservableCollection<PluginBaseVM> _plugins = new();

	[ObservableProperty]
	private PluginBaseVM? _currentPlugin;

	// Type casting helpers for UI binding
	[ObservableProperty]
	private PluginMcpHttpVM? _currentMcpHttp;
	[ObservableProperty]
	private ChPluginMcpStdioVM? _currentMcpStdio;
	[ObservableProperty]
	private ChPluginOpenApiVM? _currentOpenApi;
	[ObservableProperty]
	private ChPluginSqlVM? _currentSql;

	[ObservableProperty]
	private string _mcpStdioArguments = string.Empty;

	[ObservableProperty]
	private bool _isModalOpen;

	[ObservableProperty]
	private bool _isEditorVisible;

	[ObservableProperty]
	private bool _isDeleteConfirmVisible;

	[ObservableProperty]
	private string _modalTitle = string.Empty;

	[ObservableProperty]
	private bool _isPluginCreating;

	[ObservableProperty]
	private PluginType _selectedPluginType = PluginType.McpHttp;

	public List<PluginType> PluginTypes { get; } = Enum.GetValues<PluginType>().Cast<PluginType>().ToList();

	public PluginManageViewModel(IPluginManageService pluginService)
	{
		_pluginService = pluginService;
		LoadPluginsCommand.Execute(null);
	}

	// Default constructor for Design Time
	public PluginManageViewModel() : this(null!) { }

	[RelayCommand]
	private async Task LoadPluginsAsync()
	{
		if (_pluginService == null) return;
		var list = await _pluginService.GetPluginsAsync();
		Plugins.Clear();
		foreach (var p in list)
		{
			Plugins.Add(p);
		}
	}

	[RelayCommand]
	private void Create()
	{
		IsPluginCreating = true;
		ModalTitle = "New Plugin";
		SelectedPluginType = PluginType.McpHttp; // Default
		InitializeNewPlugin(SelectedPluginType);

		OpenEditor();
	}

	private void OpenEditor()
	{
		IsEditorVisible = true;
		IsDeleteConfirmVisible = false;
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
				CurrentPlugin = new ChPluginMcpStdioVM { Type = PluginType.McpStdio, Name = "New MCP Stdio", IsEnabled = true, Arguments = new List<string>() };
				CurrentMcpStdio = (ChPluginMcpStdioVM)CurrentPlugin;
				McpStdioArguments = string.Empty;
				break;
			case PluginType.OpenApi:
				CurrentPlugin = new ChPluginOpenApiVM { Type = PluginType.OpenApi, Name = "New OpenAPI", IsEnabled = true };
				CurrentOpenApi = (ChPluginOpenApiVM)CurrentPlugin;
				break;
			case PluginType.SqlAgentTool:
				CurrentPlugin = new ChPluginSqlVM { Type = PluginType.SqlAgentTool, Name = "New SQL Agent", IsEnabled = true, Provider = "Sqlite" };
				CurrentSql = (ChPluginSqlVM)CurrentPlugin;
				break;
		}
		NotifyTypeVisibility();
	}

	[RelayCommand]
	private async Task EditAsync(PluginBaseVM plugin)
	{
		IsPluginCreating = false;
		ModalTitle = "Edit Plugin";

		// Fetch full details
		var fullPlugin = await _pluginService.GetPluginAsync(plugin.Id);
		if (fullPlugin == null) return;

		CurrentPlugin = fullPlugin;
		SelectedPluginType = fullPlugin.Type;

		switch (fullPlugin)
		{
			case PluginMcpHttpVM mcpHttp:
				CurrentMcpHttp = mcpHttp;
				break;
			case ChPluginMcpStdioVM mcpStdio:
				CurrentMcpStdio = mcpStdio;
				McpStdioArguments = mcpStdio.Arguments != null ? string.Join(Environment.NewLine, mcpStdio.Arguments) : string.Empty;
				break;
			case ChPluginOpenApiVM openApi:
				CurrentOpenApi = openApi;
				break;
			case ChPluginSqlVM sql:
				CurrentSql = sql;
				break;
		}
		NotifyTypeVisibility();

		OpenEditor();
	}

	[RelayCommand]
	private void Delete(PluginBaseVM plugin)
	{
		CurrentPlugin = plugin;
		IsEditorVisible = false;
		IsDeleteConfirmVisible = true;
		IsModalOpen = true;
	}

	[RelayCommand]
	private async Task ConfirmDeleteAsync()
	{
		if (CurrentPlugin != null)
		{
			await _pluginService.DeletePluginAsync(CurrentPlugin.Id);
			Plugins.Remove(CurrentPlugin);
		}
		ClosePluginModal();
	}

	[RelayCommand]
	private async Task SavePluginAsync()
	{
		if (CurrentPlugin == null) return;

		// Process fields back to model
		if (CurrentPlugin is ChPluginMcpStdioVM mcpStdio)
		{
			mcpStdio.Arguments = McpStdioArguments.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
		}

		if (IsPluginCreating)
		{
			var id = await _pluginService.CreatePluginAsync(CurrentPlugin);
			CurrentPlugin.Id = id;
			Plugins.Add(CurrentPlugin);
		}
		else
		{
			await _pluginService.UpdatePluginAsync(CurrentPlugin);
			// Refresh in list
			var index = Plugins.IndexOf(Plugins.FirstOrDefault(p => p.Id == CurrentPlugin.Id)!);
			if (index >= 0)
			{
				Plugins[index] = CurrentPlugin;
			}
		}
		ClosePluginModal();
	}

	[RelayCommand]
	private void ClosePluginModal()
	{
		IsModalOpen = false;
		IsEditorVisible = false;
		IsDeleteConfirmVisible = false;
		CurrentPlugin = null;
		CurrentMcpHttp = null;
		CurrentMcpStdio = null;
		CurrentOpenApi = null;
		CurrentSql = null;
	}

	[RelayCommand]
	private async Task ToggleEnableAsync(PluginBaseVM plugin)
	{
		// Property is already changed by UI binding, just save it
		await _pluginService.UpdatePluginAsync(plugin);
	}


	// UI Helpers
	[ObservableProperty] private bool _isMcpHttp;
	[ObservableProperty] private bool _isMcpStdio;
	[ObservableProperty] private bool _isOpenApi;
	[ObservableProperty] private bool _isSqlAgentTool;

	// Helper for Auth Types visibility
	public bool IsApiKeyAuth => CurrentOpenApi?.AuthType == 1;
	public bool IsBearerTokenAuth => CurrentOpenApi?.AuthType == 2;

	public int OpenApiAuthType
	{
		get => CurrentOpenApi?.AuthType ?? 0;
		set
		{
			if (CurrentOpenApi != null && CurrentOpenApi.AuthType != value)
			{
				CurrentOpenApi.AuthType = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsApiKeyAuth));
				OnPropertyChanged(nameof(IsBearerTokenAuth));
			}
		}
	}


	private void NotifyTypeVisibility()
	{
		IsMcpHttp = SelectedPluginType == PluginType.McpHttp;
		IsMcpStdio = SelectedPluginType == PluginType.McpStdio;
		IsOpenApi = SelectedPluginType == PluginType.OpenApi;
		IsSqlAgentTool = SelectedPluginType == PluginType.SqlAgentTool;

		OnPropertyChanged(nameof(OpenApiAuthType));
		OnPropertyChanged(nameof(IsApiKeyAuth));
		OnPropertyChanged(nameof(IsBearerTokenAuth));
		OnPropertyChanged(nameof(IsSqlAgentTool));
	}

	// Hook into property changes if needed, but for now specific updates are handled in methods
}
