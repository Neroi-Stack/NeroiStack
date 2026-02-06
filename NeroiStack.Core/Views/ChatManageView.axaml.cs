using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NeroiStack.ViewModels;

namespace NeroiStack.Views;

public partial class ChatManageView : UserControl
{
	public ChatManageView()
	{
		InitializeComponent();
		if (!Design.IsDesignMode)
		{
			DataContext = ServiceLocator.Services?.GetRequiredService<ChatManageViewModel>();
		}
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}