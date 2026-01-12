using NeroiStack.Agent.Enum;

namespace NeroiStack.Agent.Model;

public class KeyVM
{
	public Guid Id { get; set; }
	public SupplierEnum Supplier { get; set; }
	public string? Endpoint { get; set; }
	public string? Key { get; set; }
	public List<string> Models { get; set; } = new();
}