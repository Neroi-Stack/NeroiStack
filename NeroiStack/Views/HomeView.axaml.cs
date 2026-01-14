using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NeroiStack.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        DataContext = new ViewModels.HomeViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
