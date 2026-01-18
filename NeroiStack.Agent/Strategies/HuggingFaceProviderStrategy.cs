#pragma warning disable SKEXP0070
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Connectors.MistralAI;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies;

public class HuggingFaceProviderStrategy : IKernelProviderStrategy
{
	public bool CanHandle(SupplierEnum supplier) => supplier == SupplierEnum.HuggingFace;

	public void Configure(IKernelBuilder builder, string modelId, KeyVM apiKey)
	{
		builder.AddHuggingFaceChatCompletion(
		   model: modelId,
		   endpoint: new Uri(apiKey.Endpoint ?? "http://localhost:11434"),
		   apiKey: apiKey?.Key
		);
	}

	public PromptExecutionSettings CreateExecutionSettings(AgentVM? agent = null)
	{
		return new HuggingFacePromptExecutionSettings
		{
			Temperature = (float)(agent?.Temperature ?? 1.0),
			TopP = (float?)agent?.TopP,
			TopK = agent?.TopK,
			MaxTokens = agent?.MaxTokens,
			Seed = agent?.Seed,
			Stop = agent?.StopSequences != null ? new List<string>(agent.StopSequences.Split(';', StringSplitOptions.RemoveEmptyEntries)) : null,
			FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
		};
	}
}
