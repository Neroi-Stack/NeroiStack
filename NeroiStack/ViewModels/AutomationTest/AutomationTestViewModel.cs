using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NeroiStack.Agent.AutomationTest.Services;


namespace NeroiStack.ViewModels.AutomationTest;

public partial class AutomationTestViewModel : ViewModelBase
{
    [ObservableProperty]
    private TestCaseListViewModel _listViewModel;

    [ObservableProperty]
    private TestCaseDetailViewModel _detailViewModel;

    public AutomationTestViewModel(IAutomationTestService testService)
    {
        _listViewModel = new TestCaseListViewModel(testService);
        _detailViewModel = new TestCaseDetailViewModel(testService);

        // Sync selection
        _listViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(TestCaseListViewModel.SelectedTestCase))
            {
                DetailViewModel.TestCase = ListViewModel.SelectedTestCase;
            }
        };

        // Load data
        _ = _listViewModel.LoadTestCasesAsync();
    }
}
