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

	public PromptExecutionSettings CreateExecutionSettings(AgentVM? agent = null)
	{
		return new OpenAIPromptExecutionSettings
		{
			Temperature = agent?.Temperature ?? 0.7,
			TopP = agent?.TopP ?? 0.9,
			MaxTokens = agent?.MaxTokens ?? 2048,
			PresencePenalty = agent?.PresencePenalty ?? 0,
			FrequencyPenalty = agent?.FrequencyPenalty ?? 0,
			ResponseFormat = null,
			ChatSystemPrompt = null,
			FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
		};
	}
}
