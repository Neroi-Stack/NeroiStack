using CommunityToolkit.Mvvm.Messaging.Messages;

namespace NeroiStack.Messages;

public class AddChatInstanceMessage : ValueChangedMessage<ChatInstanceInfo>
{
	public AddChatInstanceMessage(ChatInstanceInfo value) : base(value)
	{
	}
}

public class ChatInstanceInfo
{
	public int ChatId { get; set; } // The ID of the generic Chat Class/Config
	public string Name { get; set; } = string.Empty;
	public string? ChatInstanceName { get; set; }
	public int InstanceId { get; set; } // Unique ID for this specific running instance
}
