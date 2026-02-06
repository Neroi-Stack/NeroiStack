using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NeroiStack.Messages;
using NeroiStack.Views;

namespace NeroiStack.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
	[RelayCommand]
	private void NavigateToChats()
	{
		// Navigates to the Chat Management / Classes view
		WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(ChatManageView)));
	}

	[RelayCommand]
	private void NavigateToAgents()
	{
		// Navigates to Agent Management
		WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(AgentManageView)));
	}

	[RelayCommand]
	private void NavigateToPlugins()
	{
		// Navigates to Plugin Management
		WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(PluginManageView)));
	}

	[RelayCommand]
	private void NavigateToSettings()
	{
		// Navigates to Settings
		WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(KeyManagementView)));
	}
}
