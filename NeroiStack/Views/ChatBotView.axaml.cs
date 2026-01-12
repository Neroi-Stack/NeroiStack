using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NeroiStack.ViewModels;

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
}
