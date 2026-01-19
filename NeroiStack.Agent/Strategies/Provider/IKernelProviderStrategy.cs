using Microsoft.SemanticKernel;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies.Provider;

public interface IKernelProviderStrategy
{
	bool CanHandle(SupplierEnum supplier);
	void Configure(IKernelBuilder builder, string modelId, KeyVM apiKey);
	dynamic CreateExecutionSettings(AgentVM? agent = null);
}
