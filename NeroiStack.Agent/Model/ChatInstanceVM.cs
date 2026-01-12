using NeroiStack.Agent.Data.Entities;

namespace NeroiStack.Agent.Model;

public class ChatInstanceVM
{
	public int Id { get; set; }
	public int ChatId { get; set; }
	public string Name { get; set; } = string.Empty; // Name from the Chat Class
	public DateTime CreatedAt { get; set; }

	public static explicit operator ChatInstanceVM(ChChatInstance entity) => new()
	{
		Id = entity.Id,
		ChatId = entity.ChatId,
		CreatedAt = entity.CreatedAt
	};
}
