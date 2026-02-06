using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NeroiStack.ViewModels;

namespace NeroiStack.Views;

public partial class KeyManagementView : UserControl
{
	public KeyManagementView()
	{
		InitializeComponent();
		if (!Design.IsDesignMode)
		{
			DataContext = App.Current.Services?.GetRequiredService<KeyManagementViewModel>();
		}
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}