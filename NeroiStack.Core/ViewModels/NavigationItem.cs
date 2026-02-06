using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NeroiStack.ViewModels;

public partial class NavigationItem : ObservableObject
{
	[ObservableProperty]
	private bool _isSelected;

	public NavigationItem()
	{

	}

	public string Name { get; set; } = string.Empty;
	public string Text { get; set; } = string.Empty;
	public string Icon { get; set; } = "Chat";
	public Type ViewType { get; set; } = typeof(object);
	public object? Tag { get; set; }
	public System.Windows.Input.ICommand? DeleteCommand { get; set; }
}
