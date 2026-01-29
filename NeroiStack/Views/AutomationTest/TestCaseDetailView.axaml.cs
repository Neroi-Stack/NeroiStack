using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NeroiStack.Views.AutomationTest;

public partial class TestCaseDetailView : UserControl
{
	public TestCaseDetailView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}

