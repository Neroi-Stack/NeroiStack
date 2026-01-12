namespace NeroiStack.Agent.Data.Entities;

public class ChChatInstance
{
	public int Id { get; set; }
	public int ChatId { get; set; }
	public DateTime CreatedAt { get; set; }
	public string? SelectedModel { get; set; }
}