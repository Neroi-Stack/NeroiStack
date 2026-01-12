using System;

namespace NeroiStack.ViewModels;

public class NavigationItem
{
	public string Name { get; set; } = string.Empty;
	public string Text { get; set; } = string.Empty;
	public string Icon { get; set; } = "Chat";
	public Type ViewType { get; set; } = typeof(object);
	public object? Tag { get; set; }
	public System.Windows.Input.ICommand? DeleteCommand { get; set; }
}
