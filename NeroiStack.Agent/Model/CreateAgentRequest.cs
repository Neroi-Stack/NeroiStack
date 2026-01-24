namespace NeroiStack.Agent.Model;

public class CreateAgentRequest
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? Instructions { get; set; }
	public double? Temperature { get; set; }
	public double? TopP { get; set; }
	public int? TopK { get; set; }
	public int? MaxTokens { get; set; }
	public double? PresencePenalty { get; set; }
	public double? FrequencyPenalty { get; set; }
	public long? Seed { get; set; }
	public string? StopSequences { get; set; }
	public string? ResponseFormat { get; set; }
	public string? PromptTemplate { get; set; }
	public bool IsEnabled { get; set; }
	public int[] PluginIds { get; set; } = [];
}