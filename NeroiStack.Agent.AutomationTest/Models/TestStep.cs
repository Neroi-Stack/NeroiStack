using System;

namespace NeroiStack.Agent.AutomationTest.Models;


public class TestStep
{
    public Guid StepId { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Expectation { get; set; } = string.Empty;
    public string? Result { get; set; }
    public string? Note { get; set; }
}
