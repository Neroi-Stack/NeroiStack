using NeroiStack.Agent.Data.Entities;

namespace NeroiStack.Agent.Model;

public class AgentVM
{
	public int Id { get; set; }
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
	public DateTime CreatedAt { get; set; }
	public bool IsEnabled { get; set; }
	public int[] PluginIds { get; set; } = [];

	public static explicit operator AgentVM(ChAgent agent) => new()
	{
		Id = agent.Id,
		Name = agent.Name,
		Description = agent.Description,
		Instructions = agent.Instructions,
		Kernel = agent.Kernel,
		Temperature = agent.Temperature,
		TopP = agent.TopP,
		MaxTokens = agent.MaxTokens,
		PresencePenalty = agent.PresencePenalty,
		FrequencyPenalty = agent.FrequencyPenalty,
		ResponseFormat = agent.ResponseFormat,
		PromptTemplate = agent.PromptTemplate,
		CreatedAt = agent.CreatedAt,
		IsEnabled = agent.IsEnabled
	};
}