using Avalonia.Controls;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Messaging;
using NeroiStack.Messages;

namespace NeroiStack.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		WeakReferenceMessenger.Default.Register<ToggleSidebarMessage>(this, (r, m) => ToggleSidebar());
	}

	private void ToggleSidebar()
	{
		// If SidebarColumn width > 0, collapse it; otherwise restore to 260
		var col = RootGrid.ColumnDefinitions[0];
		if (col.Width.Value > 0)
		{
			col.Width = new GridLength(0);
			Splitter.IsVisible = false;
		}
		else
		{
			col.Width = new GridLength(260);
			Splitter.IsVisible = true;
		}
	}
}