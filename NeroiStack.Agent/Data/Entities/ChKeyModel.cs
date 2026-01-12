namespace NeroiStack.Agent.Data.Entities;

public class ChKeyModel
{
	public Guid Id { get; set; }
	public Guid KeyId { get; set; }
	public string ModelId { get; set; } = string.Empty;
	public virtual ChKey Key { get; set; } = null!;
}
