namespace NeroiStack.Agent.Data.Entities;

public class ChChatAgent
{
	public int ChatId { get; set; }
	public int AgentId { get; set; }

	/// <summary>
	/// Sequential: Execution order
	/// GroupChat/Magentic: Priority or irrelevant
	/// </summary>
	public int Order { get; set; }

	/// <summary>
	/// GroupChat: Moderator/Admin
	/// Magentic: Manager
	/// Handoff: Initial Agent
	/// </summary>
	public bool IsPrimary { get; set; }
}