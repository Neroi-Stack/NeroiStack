using Microsoft.SemanticKernel;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace NeroiStack.Agent.Strategies;

public class OpenAIProviderStrategy : IKernelProviderStrategy
{
	public bool CanHandle(SupplierEnum supplier) => supplier == SupplierEnum.OpenAI;

	public void Configure(IKernelBuilder builder, string modelId, KeyVM apiKey)
	{
		builder.AddOpenAIChatCompletion(modelId, apiKey?.Key ?? "");
	}

	public dynamic CreateExecutionSettings(AgentVM? agent = null)
	{
		return new OpenAIPromptExecutionSettings
		{
			Temperature = agent?.Temperature,
			TopP = agent?.TopP,
			MaxTokens = agent?.MaxTokens,
			PresencePenalty = agent?.PresencePenalty,
			FrequencyPenalty = agent?.FrequencyPenalty,
			Seed = agent?.Seed,
			StopSequences = agent?.StopSequences?.Split(';', StringSplitOptions.RemoveEmptyEntries),
			ResponseFormat = agent?.ResponseFormat,
			ChatSystemPrompt = null,
			FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
		};
	}
}
