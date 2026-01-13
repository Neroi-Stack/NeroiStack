using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NeroiStack.Component.Model;

public partial class UpsertPluginModal : UserControl
{
	public UpsertPluginModal()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
