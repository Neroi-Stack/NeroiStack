using NeroiStack.Agent.Enum;

namespace NeroiStack.Agent.Data.Entities;

public class ChKey
{
	public Guid Id { get; set; }
	public SupplierEnum Supplier { get; set; }
	public string EncryptedKey { get; set; } = string.Empty;
	public string Endpoint { get; set; } = string.Empty;
	public KeyType KeyType { get; set; }
	public virtual ICollection<ChKeyModel> ModelsNav { get; set; } = new List<ChKeyModel>();
}
