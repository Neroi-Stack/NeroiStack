namespace NeroiStack.Agent.Model;

public class CreateAgentRequest
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? Instructions { get; set; }
	public string? Kernel { get; set; }
	public double Temperature { get; set; } = 0.7;
	public double TopP { get; set; } = 0.9;
	public int MaxTokens { get; set; } = 2048;
	public double PresencePenalty { get; set; }
	public double FrequencyPenalty { get; set; }
	public string? ResponseFormat { get; set; }
	public string? PromptTemplate { get; set; }
	public bool IsEnabled { get; set; }
	public int[] PluginIds { get; set; } = [];
}