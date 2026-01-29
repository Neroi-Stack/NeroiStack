using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeroiStack.Agent.AutomationTest.Models;
using NeroiStack.Agent.AutomationTest.Services;


namespace NeroiStack.ViewModels.AutomationTest;

public partial class TestCaseDetailViewModel : ViewModelBase
{
    private readonly IAutomationTestService _testService;

    [ObservableProperty]
    private TestCase? _testCase;

    [ObservableProperty]
    private ObservableCollection<TestStep> _steps = [];

    public TestCaseDetailViewModel(IAutomationTestService testService)
    {
        _testService = testService;
    }

    partial void OnTestCaseChanged(TestCase? value)
    {
        if (value != null)
        {
            Steps = new ObservableCollection<TestStep>(value.Steps);
        }
        else
        {
            Steps = [];
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (TestCase == null) return;
        TestCase.Steps = [.. Steps];
        await _testService.SaveTestCaseAsync(TestCase);
    }

    [RelayCommand]
    private void AddStep()
    {
        if (TestCase == null) return;
        Steps.Add(new TestStep { Title = "New Step" });
    }

    [RelayCommand]
    private void RemoveStep(TestStep step)
    {
        Steps.Remove(step);
    }

    [RelayCommand]
    private async Task RunTestAsync()
    {
        if (TestCase == null) return;
        await SaveAsync();
        await _testService.RunTestCaseAsync(TestCase.Id);

        // Refresh
        var updated = await _testService.GetTestCaseByIdAsync(TestCase.Id);
        if (updated != null)
        {
            TestCase = updated;
        }
    }
}
