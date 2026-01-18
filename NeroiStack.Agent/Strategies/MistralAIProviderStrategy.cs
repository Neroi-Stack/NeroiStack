#pragma warning disable SKEXP0070
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.MistralAI;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies;

public class MistralAIProviderStrategy : IKernelProviderStrategy
{
	public bool CanHandle(SupplierEnum supplier) => supplier == SupplierEnum.MistralAI;

	public void Configure(IKernelBuilder builder, string modelId, KeyVM apiKey)
	{
		builder.AddOllamaChatCompletion(
		   modelId: modelId,
		   endpoint: new Uri(apiKey.Endpoint ?? "http://localhost:11434")
		);
	}

	public PromptExecutionSettings CreateExecutionSettings(AgentVM? agent = null)
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
