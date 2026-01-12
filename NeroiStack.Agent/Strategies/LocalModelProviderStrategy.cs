using Microsoft.SemanticKernel;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace NeroiStack.Agent.Strategies;

public class LocalModelProviderStrategy : IKernelProviderStrategy
{
	public bool CanHandle(SupplierEnum supplier) => supplier == SupplierEnum.LocalModel;

	public void Configure(IKernelBuilder builder, string modelId, KeyVM apiKey)
	{
		builder.AddOpenAIChatCompletion(
		   apiKey: null,
		   modelId: modelId,
		   endpoint: new Uri(apiKey.Endpoint ?? "http://localhost:8080/v1/chat/completions"),
		   httpClient: new HttpClient() { Timeout = TimeSpan.FromSeconds(600) });
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
