using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NeroiStack.ViewModels;

namespace NeroiStack.Views;

public partial class AgentManageView : UserControl
{
	public AgentManageView()
	{
		InitializeComponent();
		if (!Design.IsDesignMode)
		{
			DataContext = ServiceLocator.Services?.GetRequiredService<AgentManageViewModel>();
		}
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
