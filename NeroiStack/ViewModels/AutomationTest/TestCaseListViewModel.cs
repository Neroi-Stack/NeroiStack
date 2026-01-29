using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeroiStack.Agent.AutomationTest.Models;
using NeroiStack.Agent.AutomationTest.Services;


namespace NeroiStack.ViewModels.AutomationTest;

public partial class TestCaseListViewModel : ViewModelBase
{
    private readonly IAutomationTestService _testService;

    [ObservableProperty]
    private ObservableCollection<TestCase> _testCases = [];

    [ObservableProperty]
    private TestCase? _selectedTestCase;

    public TestCaseListViewModel(IAutomationTestService testService)
    {
        _testService = testService;
    }

    public async Task LoadTestCasesAsync()
    {
        var cases = await _testService.GetTestCasesAsync();
        TestCases = new ObservableCollection<TestCase>(cases);
    }

    [RelayCommand]
    private async Task CreateNewTestCaseAsync()
    {
        var newCase = new TestCase { Name = "New Test Case" };
        await _testService.SaveTestCaseAsync(newCase);
        TestCases.Insert(0, newCase);
        SelectedTestCase = newCase;
    }

    [RelayCommand]
    private async Task DeleteTestCaseAsync(TestCase? testCase)
    {
        if (testCase == null) return;
        await _testService.DeleteTestCaseAsync(testCase.Id);
        TestCases.Remove(testCase);
        if (SelectedTestCase == testCase) SelectedTestCase = null;
    }
}
