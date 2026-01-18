using NeroiStack.Agent.Data.Entities;

namespace NeroiStack.Agent.Model;

public class AgentVM
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? Instructions { get; set; }
	public string? Kernel { get; set; }
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
		TopK = agent.TopK,
		MaxTokens = agent.MaxTokens,
		PresencePenalty = agent.PresencePenalty,
		FrequencyPenalty = agent.FrequencyPenalty,
		Seed = agent.Seed,
		StopSequences = agent.StopSequences,
		ResponseFormat = agent.ResponseFormat,
		PromptTemplate = agent.PromptTemplate,
		CreatedAt = agent.CreatedAt,
		IsEnabled = agent.IsEnabled
	};
}