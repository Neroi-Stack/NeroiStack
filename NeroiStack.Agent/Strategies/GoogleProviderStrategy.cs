using Microsoft.SemanticKernel;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

using Microsoft.SemanticKernel.Connectors.Google;

namespace NeroiStack.Agent.Strategies;

public class GoogleProviderStrategy : IKernelProviderStrategy
{
	public bool CanHandle(SupplierEnum supplier) => supplier == SupplierEnum.Google;

	public void Configure(IKernelBuilder builder, string modelId, KeyVM apiKey)
	{
		builder.AddGoogleAIGeminiChatCompletion(modelId, apiKey?.Key ?? "");
	}

	public PromptExecutionSettings CreateExecutionSettings(AgentVM? agent = null)
	{
		return new GeminiPromptExecutionSettings
		{
			Temperature = agent?.Temperature ?? 0.7,
			TopP = agent?.TopP ?? 0.9,
			MaxTokens = agent?.MaxTokens ?? 2048,
			FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
		};
	}
}
