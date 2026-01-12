using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Interface;

public interface IChatService
{
	Task InitializeAsync(int chatInstanceId, CancellationToken ct = default);
	Task<(string text, Func<string, Task>)> ChatAsync(InvokeChatRequest chatRequest);
	void ClearSession(int chatInstanceId);
}