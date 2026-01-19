using Microsoft.SemanticKernel;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

using Microsoft.SemanticKernel.Connectors.Google;

namespace NeroiStack.Agent.Strategies.Provider;

public class GoogleProviderStrategy : IKernelProviderStrategy
{
	public bool CanHandle(SupplierEnum supplier) => supplier == SupplierEnum.Google;

	public void Configure(IKernelBuilder builder, string modelId, KeyVM apiKey)
	{
		builder.AddGoogleAIGeminiChatCompletion(modelId, apiKey?.Key ?? "");
	}

	public dynamic CreateExecutionSettings(AgentVM? agent = null)
	{
		return new GeminiPromptExecutionSettings
		{
			Temperature = agent?.Temperature,
			TopP = agent?.TopP,
			TopK = agent?.TopK,
			MaxTokens = agent?.MaxTokens,
			StopSequences = agent?.StopSequences?.Split(';', StringSplitOptions.RemoveEmptyEntries),
			FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
		};
	}
}
