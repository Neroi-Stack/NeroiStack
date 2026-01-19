using Microsoft.SemanticKernel;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace NeroiStack.Agent.Strategies.Provider;

public class AzureOpenAIProviderStrategy : IKernelProviderStrategy
{
	public bool CanHandle(SupplierEnum supplier) => supplier == SupplierEnum.AzureOpenAI;

	public void Configure(IKernelBuilder builder, string modelId, KeyVM apiKey)
	{
		builder.AddAzureOpenAIChatCompletion(modelId, apiKey?.Key ?? "", apiKey?.Endpoint ?? "");
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
			ResponseFormat = agent?.ResponseFormat, // Assuming string or object handling elsewhere, but property exists
			ChatSystemPrompt = null, // System prompt usually handled in chat history
			FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
		};
	}
}
