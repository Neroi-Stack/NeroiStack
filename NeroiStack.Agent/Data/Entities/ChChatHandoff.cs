namespace NeroiStack.Agent.Data.Entities;

public class ChChatHandoff
{
	public int Id { get; set; }
	public int ChatId { get; set; }
	public int FromAgentId { get; set; }
	public int ToAgentId { get; set; }
	public string? Description { get; set; }

	public ChChat Chat { get; set; } = null!;
	public ChAgent FromAgent { get; set; } = null!;
	public ChAgent ToAgent { get; set; } = null!;
}