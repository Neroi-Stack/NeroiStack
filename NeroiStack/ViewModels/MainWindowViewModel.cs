using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using NeroiStack.Messages;
using NeroiStack.Agent.Interface;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NeroiStack.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	private readonly IChatInstanceService? _instanceService;
	public string Greeting { get; } = "Welcome to Avalonia!";

	public object Home { get; set; } = new[]
	{
		new { Name = "Home", Text = "Home", Icon = "Home", ViewType = typeof(Views.HomeView) },
	};
	public object Menus { get; set; } = new[]
	{
		new { Name = "ChatClasses", Text = "Chat Classes", Icon = "SettingsChat", ViewType = typeof(Views.ChatManageView) },
		new { Name = "Agents", Text = "Agents", Icon = "Person", ViewType = typeof(Views.AgentManageView) },
		new { Name = "Plugins", Text = "Plugins", Icon = "PuzzlePiece", ViewType = typeof(Views.PluginManageView) },
	};

	public object SecondaryMenus { get; set; } = new[]
	{
		new { Name = "Key Model", Text = "Key Model", Icon = "Key", ViewType = typeof(Views.KeyManagementView) },
	};

	public ObservableCollection<NavigationItem> ChatMenus { get; set; } = new();

	private string _title = "";
	public string Title
	{
		get => _title;
		set => SetProperty(ref _title, value);
	}

	private Control? _currentPage;
	public Control? CurrentPage
	{
		get => _currentPage;
		set => SetProperty(ref _currentPage, value);
	}

	public ICommand MenuCommand { get; }

	public MainWindowViewModel(IChatInstanceService instanceService)
	{
		_instanceService = instanceService;
		MenuCommand = new RelayCommand(OnMenu);

		// Register for Chat Instance messages
		WeakReferenceMessenger.Default.Register<AddChatInstanceMessage>(this, (r, m) =>
		{
			var info = m.Value;
			ChatMenus.Insert(0, new NavigationItem
			{
				Name = $"Instance_{info.InstanceId}",
				Text = string.IsNullOrEmpty(info.ChatInstanceName) ? $"{info.Name} #{info.InstanceId}" : info.ChatInstanceName,
				Icon = "Bot",
				ViewType = typeof(Views.ChatBotView),
				Tag = info,
				DeleteCommand = DeleteChatInstanceCommand
			});
		});

		// Register for Navigation messages
		WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
		{
			var viewType = m.Value;
			NavigateTo(viewType);
		});

		InitializeAsync();

		// Set default page to Home
		Title = "Home";
		CurrentPage = new Views.HomeView();
	}

	// Default constructor for design/preview
	public MainWindowViewModel() : this(null!) { }

	private async void InitializeAsync()
	{
		if (_instanceService == null) return;

		try
		{
			var instances = await _instanceService.GetInstancesAsync();
			foreach (var inst in instances)
			{
				ChatMenus.Add(new NavigationItem
				{
					Name = $"Instance_{inst.Id}",
					Text = string.IsNullOrEmpty(inst.ChatInstanceName) ? $"{inst.Name} #{inst.Id}" : inst.ChatInstanceName,
					Icon = "Bot",
					ViewType = typeof(Views.ChatBotView),
					Tag = new ChatInstanceInfo
					{
						ChatId = inst.ChatId,
						InstanceId = inst.Id,
						Name = inst.Name,
						ChatInstanceName = inst.ChatInstanceName
					},
					DeleteCommand = DeleteChatInstanceCommand
				});
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error loading instances: {ex.Message}");
		}
	}

	[RelayCommand]
	private async Task DeleteChatInstanceAsync(NavigationItem item)
	{
		if (item?.Tag is ChatInstanceInfo info && _instanceService != null)
		{
			try
			{
				await _instanceService.DeleteInstanceAsync(info.InstanceId);
				ChatMenus.Remove(item);

				// If we deleted the current page, clear it or go home?
				// For now, simple logic: if current page is this chat instance, maybe clear it.
				// But detecting that is a bit loose. Leaving as is, user will just see the page until they navigate away.
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error deleting instance: {ex.Message}");
			}
		}
	}

	[RelayCommand]
	private static void OpenGitHub()
	{
		var url = "https://github.com/Neroi-Stack/NeroiStack/issues";
		try
		{
			var psi = new ProcessStartInfo
			{
				FileName = url,
				UseShellExecute = true
			};
			Process.Start(psi);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to open URL {url}: {ex.Message}");
		}
	}

	private void NavigateTo(Type viewType)
	{
		if (typeof(Control).IsAssignableFrom(viewType))
		{
			CurrentPage = Activator.CreateInstance(viewType) as Control;
		}
	}

	private void OnMenu(object? item)
	{
		if (item == null) return;

		if (item is NavigationItem navItem)
		{
			Title = navItem.Text;
			if (navItem.ViewType != null && typeof(Control).IsAssignableFrom(navItem.ViewType))
			{
				var view = (Control)Activator.CreateInstance(navItem.ViewType)!;

				// If it's a Chat instance, initialize the ViewModel
				if (navItem.Tag is ChatInstanceInfo info && view.DataContext is ChatBotViewModel vm)
				{
					vm.InitializeChat(info.ChatId, info.InstanceId, string.IsNullOrEmpty(info.ChatInstanceName) ? info.Name : info.ChatInstanceName);
				}

				CurrentPage = view;
			}
			return;
		}

		var textProp = item.GetType().GetProperty("Text");
		if (textProp?.GetValue(item) is string t) Title = t;

		var vtProp = item.GetType().GetProperty("ViewType");
		if (vtProp?.GetValue(item) is Type viewType && typeof(Control).IsAssignableFrom(viewType))
		{
			CurrentPage = Activator.CreateInstance(viewType) as Control;
			return;
		}

		var clickProp = item.GetType().GetProperty("Click");
		if (clickProp?.GetValue(item) is string methodName)
		{
			var mi = GetType().GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
			mi?.Invoke(this, null);
		}
	}
}
