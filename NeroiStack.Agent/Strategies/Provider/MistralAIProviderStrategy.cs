#pragma warning disable SKEXP0070
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.MistralAI;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies.Provider;

public class MistralAIProviderStrategy : IKernelProviderStrategy
{
	public bool CanHandle(SupplierEnum supplier) => supplier == SupplierEnum.MistralAI;

	public void Configure(IKernelBuilder builder, string modelId, KeyVM apiKey)
	{
		builder.AddMistralChatCompletion(
		   modelId: modelId,
		   apiKey: apiKey.Key ?? string.Empty,
		   endpoint: apiKey.Endpoint != null ? new Uri(apiKey.Endpoint) : null
		);
	}

	public dynamic CreateExecutionSettings(AgentVM? agent = null)
	{
		return new MistralAIPromptExecutionSettings
		{
			Temperature = agent?.Temperature ?? 0.7,
			TopP = agent?.TopP ?? 1.0,
			RandomSeed = (int?)agent?.Seed,
			MaxTokens = agent?.MaxTokens,
			Stop = agent?.StopSequences != null ? new List<string>(agent.StopSequences.Split(';', StringSplitOptions.RemoveEmptyEntries)) : null,
			ResponseFormat = agent?.ResponseFormat ?? "text",
			FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
		};
	}
}
