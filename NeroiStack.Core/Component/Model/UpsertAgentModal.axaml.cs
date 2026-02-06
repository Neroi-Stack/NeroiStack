using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NeroiStack.Component.Model;

public partial class UpsertAgentModal : UserControl
{
	public UpsertAgentModal()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
