using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using NeroiStack.Agent.AutomationTest.Models;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Factories;
using NeroiStack.Agent.Model;
using Microsoft.EntityFrameworkCore;



namespace NeroiStack.Agent.AutomationTest.Services;


public interface IAutomationTestService
{
    Task<List<TestCase>> GetTestCasesAsync();
    Task<TestCase?> GetTestCaseByIdAsync(Guid id);
    Task SaveTestCaseAsync(TestCase testCase);
    Task DeleteTestCaseAsync(Guid id);
    Task RunTestCaseAsync(Guid id);
    Task RunAllTestCasesAsync();
}

public class AutomationTestService : IAutomationTestService
{
    private readonly string _storagePath;
    private readonly IKernelFactory _kernelFactory;
    private readonly IChatContext _chatContext;

    public AutomationTestService(IKernelFactory kernelFactory, IChatContext chatContext)
    {
        _kernelFactory = kernelFactory;
        _chatContext = chatContext;
        // Define storage path, e.g., in AppData or local project directory
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _storagePath = Path.Combine(appData, "NeroiStack", "AutomationTests");

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }


    public async Task<List<TestCase>> GetTestCasesAsync()
    {
        var files = Directory.GetFiles(_storagePath, "*.json");
        var testCases = new List<TestCase>();

        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var testCase = JsonSerializer.Deserialize<TestCase>(json);
                if (testCase != null)
                {
                    testCases.Add(testCase);
                }
            }
            catch
            {
                // Log or handle deserialization error
            }
        }

        return testCases.OrderByDescending(tc => tc.CreatedAt).ToList();
    }

    public async Task<TestCase?> GetTestCaseByIdAsync(Guid id)
    {
        var filePath = Path.Combine(_storagePath, $"{id}.json");
        if (!File.Exists(filePath)) return null;

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<TestCase>(json);
    }

    public async Task SaveTestCaseAsync(TestCase testCase)
    {
        var filePath = Path.Combine(_storagePath, $"{testCase.Id}.json");
        var json = JsonSerializer.Serialize(testCase, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task DeleteTestCaseAsync(Guid id)
    {
        var filePath = Path.Combine(_storagePath, $"{id}.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public async Task RunTestCaseAsync(Guid id)
    {
        var testCase = await GetTestCaseByIdAsync(id);
        if (testCase == null) return;

        testCase.LastRunAt = DateTime.Now;

        try
        {
            // 1. Setup Kernel with Playwright MCP
            var session = new ChatSession
            {
                ChatInstanceId = 0, // Virtual session
                CurrentSupplier = SupplierEnum.OpenAI, // Default for judgment
            };

            // Try to find a valid key for LLM judgment
            var keys = await _chatContext.Keys.ToListAsync();
            var key = keys.FirstOrDefault();
            if (key == null) throw new Exception("No API key found for LLM judgment.");

            var (kernel, settings) = await _kernelFactory.CreateKernelAsync(_chatContext, session, key.Supplier, "gpt-4o", CancellationToken.None);

            // 2. Add Playwright MCP Plugin
            var transport = new StdioClientTransport(new()
            {
                Name = "Playwright",
                Command = "npx",
                Arguments = ["-y", "@modelcontextprotocol/server-playwright"]
            });

            var mcpClient = await McpClient.CreateAsync(transport);
            session.Resources.Add(transport);
            session.Resources.Add(mcpClient);

            var tools = await mcpClient.ListToolsAsync();
            var functions = tools.Select(t => t.AsKernelFunction()).ToList();
            kernel.Plugins.AddFromFunctions("Browser", functions);


            // 3. Execute Steps
            foreach (var step in testCase.Steps)
            {
                step.Result = "Running...";
                await SaveTestCaseAsync(testCase);

                var prompt = $@"
Navigate to {testCase.Url} if not already there.
Perform this action: {step.Action}
Expected: {step.Expectation}
Report back what happened.
";
                var result = await kernel.InvokePromptAsync(prompt, new KernelArguments(new OpenAIPromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions }));
                step.Result = result.ToString();
                await SaveTestCaseAsync(testCase);
            }

            // 4. Final LLM Judgment
            var executionLog = string.Join("\n", testCase.Steps.Select(s => $"Step: {s.Title}\nAction: {s.Action}\nResult: {s.Result}"));
            var judgmentPrompt = $@"
Analyze this test execution:
URL: {testCase.Url}
Log:
{executionLog}

For each step, determine if it met its individual expectation.
Finally, for each step, update its 'Final Result' (Pass/Fail) and 'Note'.
Respond in JSON format for each step:
[
  {{ ""StepId"": ""guid"", ""Result"": ""Pass/Fail"", ""Note"": ""reason"" }}
]
";
            var judgmentResult = await kernel.InvokePromptAsync(judgmentPrompt);

            // For now, let's just store the full judgment in a summary note if we can't parse easily
            foreach (var step in testCase.Steps)
            {
                step.Note = "Judgment result received. See step details.";
            }

            if (testCase.Steps.Any())
            {
                testCase.Steps[0].Note = judgmentResult.ToString();
            }

        }
        catch (Exception ex)
        {
            foreach (var step in testCase.Steps)
            {
                step.Result = "Error";
                step.Note = ex.Message;
            }
        }
        finally
        {
            await SaveTestCaseAsync(testCase);
        }
    }

    public async Task RunAllTestCasesAsync()
    {
        var cases = await GetTestCasesAsync();
        foreach (var tc in cases)
        {
            await RunTestCaseAsync(tc.Id);
        }
    }
}

