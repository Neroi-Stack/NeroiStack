using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NeroiStack.Views.AutomationTest;

public partial class TestCaseListView : UserControl
{
	public TestCaseListView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}

