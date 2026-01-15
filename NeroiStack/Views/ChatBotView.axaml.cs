using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Microsoft.Extensions.DependencyInjection;
using NeroiStack.ViewModels;
using System;
using System.Linq;

namespace NeroiStack.Views;

public partial class ChatBotView : UserControl
{
	public ChatBotView()
	{
		InitializeComponent();
		if (!Design.IsDesignMode)
		{
			DataContext = App.Current.Services?.GetService<ChatBotViewModel>() ?? new ChatBotViewModel();
		}
	}

	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);
		if (DataContext is ChatBotViewModel vm)
		{
			vm.OnAddFilesRequested = async () =>
			{
				var topLevel = TopLevel.GetTopLevel(this);
				if (topLevel == null) return [];

				var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
				{
					Title = "Select Images",
					AllowMultiple = true,
					FileTypeFilter = [Avalonia.Platform.Storage.FilePickerFileTypes.ImageAll]
				});

				return files.Select(f => f.Path.LocalPath);
			};
		}
	}

	protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);
		var messageInput = this.FindControl<TextBox>("MessageInput");
		if (messageInput != null)
		{
			messageInput.RemoveHandler(InputElement.KeyDownEvent, MessageInput_KeyDown);
			messageInput.AddHandler(InputElement.KeyDownEvent, MessageInput_KeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
		}
	}

	private async void MessageInput_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
	{
		if (sender is not TextBox textBox) return;
		if (e.Key == Key.Enter)
		{
			if (e.KeyModifiers == KeyModifiers.None)
			{
				if (DataContext is ChatBotViewModel vm && vm.SendMessageCommand.CanExecute(null))
				{
					vm.SendMessageCommand.Execute(null);
					e.Handled = true;
				}
			}
		}
	}
}
