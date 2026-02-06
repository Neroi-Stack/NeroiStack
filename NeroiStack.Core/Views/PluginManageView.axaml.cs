using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NeroiStack.ViewModels;

namespace NeroiStack.Views;

public partial class PluginManageView : UserControl
{
	public PluginManageView()
	{
		InitializeComponent();
		if (!Design.IsDesignMode)
		{
			DataContext = App.Current.Services?.GetRequiredService<PluginManageViewModel>();
		}
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
