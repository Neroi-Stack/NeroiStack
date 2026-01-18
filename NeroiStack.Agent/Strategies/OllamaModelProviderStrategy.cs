#pragma warning disable SKEXP0070
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies;

public class OllamaModelProviderStrategy : IKernelProviderStrategy
{
	public bool CanHandle(SupplierEnum supplier) => supplier == SupplierEnum.Ollama;

	public void Configure(IKernelBuilder builder, string modelId, KeyVM apiKey)
	{
		builder.AddOllamaChatCompletion(
		   modelId: modelId,
		   endpoint: new Uri(apiKey.Endpoint ?? "http://localhost:11434")
		);
	}

	public PromptExecutionSettings CreateExecutionSettings(AgentVM? agent = null)
	{
		return new OllamaPromptExecutionSettings
		{
			Temperature = (float?)agent?.Temperature,
			TopP = (float?)agent?.TopP,
			TopK = agent?.TopK,
			NumPredict = agent?.MaxTokens,
			Stop = agent?.StopSequences != null ? new List<string>(agent.StopSequences.Split(';', StringSplitOptions.RemoveEmptyEntries)) : null,
			FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
		};
	}
}
