using NeroiStack.Agent.Enum;

namespace NeroiStack.Agent.Model;

public class InvokeChatRequest
{
	public int ChatInstanceId { get; init; }
	public string Text { get; init; } = string.Empty;
	public SupplierEnum Supplier { get; init; } = SupplierEnum.OpenAI;
	public int ModelId { get; init; }
	public string ModelName { get; init; } = string.Empty;
	public Func<string, Task>? OnChunk { get; set; }
	public CancellationToken Ct { get; init; } = default;
}