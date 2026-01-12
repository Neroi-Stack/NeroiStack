using NeroiStack.Agent.Enum;

namespace NeroiStack.Agent.Data.Entities;

public class ChChatMemory
{
	public int Id { get; set; }
	public int ChatInstanceId { get; set; }
	public RoleType RoleType { get; set; } = RoleType.User;
	public string Content { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
}