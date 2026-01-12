namespace NeroiStack.Agent.Data.Entities;

public class ChAgent
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? Instructions { get; set; }
	public string? Kernel { get; set; }
	public double Temperature { get; set; }
	public double TopP { get; set; }
	public int MaxTokens { get; set; }
	public double PresencePenalty { get; set; }
	public double FrequencyPenalty { get; set; }
	public string? ResponseFormat { get; set; }
	public string? PromptTemplate { get; set; }
	public DateTime CreatedAt { get; set; }
	public bool IsEnabled { get; set; } = true;
}