using System;
using System.Collections.Generic;

namespace NeroiStack.Agent.AutomationTest.Models;


public class TestCase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<TestStep> Steps { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastRunAt { get; set; }
}
