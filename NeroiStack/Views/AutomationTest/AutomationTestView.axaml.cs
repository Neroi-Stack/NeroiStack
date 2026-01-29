using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace NeroiStack.Views.AutomationTest;

public partial class AutomationTestView : UserControl
{
	public AutomationTestView()
	{
		InitializeComponent();
		if (!Design.IsDesignMode)
		{
			DataContext = App.Current.Services?.GetRequiredService<NeroiStack.ViewModels.AutomationTest.AutomationTestViewModel>();
		}
	}



	private void InitializeComponent()
	{
		Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
	}
}

