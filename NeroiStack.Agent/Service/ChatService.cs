using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.EntityFrameworkCore;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Interface;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using NeroiStack.Agent.Strategies.Orchestration;
using NeroiStack.Agent.Strategies;
using NeroiStack.Agent.Factories;
using Microsoft.SemanticKernel;
using NeroiStack.Common.Interface;

namespace NeroiStack.Agent.Service;

public class ChatService(IServiceScopeFactory scopeFactory, IEnumerable<IOrchestrationStrategy> orchestrationStrategies) : IChatService
{
	private readonly ConcurrentDictionary<int, ChatSession> _sessions = new();


	public void ClearSession(int chatInstanceId)
	{
		if (_sessions.TryRemove(chatInstanceId, out var session))
		{
			foreach (var res in session.Resources)
			{
				if (res is IDisposable d) d.Dispose();
			}
		}
	}

	public async Task InitializeAsync(int chatInstanceId, CancellationToken ct = default)
	{
		if (_sessions.ContainsKey(chatInstanceId)) return;

		using var scope = scopeFactory.CreateScope();
		var chatContext = scope.ServiceProvider.GetRequiredService<IChatContext>();

		var instance = await chatContext.ChatInstances.FindAsync([chatInstanceId], ct)
			?? throw new Exception($"Chat Instance {chatInstanceId} not found");

		var chatDef = await chatContext.Chats.FindAsync([instance.ChatId], ct)
			?? throw new Exception("Chat definition not found");

		var chatAgents = await chatContext.ChatAgents
			.Where(ca => ca.ChatId == instance.ChatId)
			.OrderBy(ca => ca.Order)
			.ToListAsync(ct);

		if (chatAgents.Count == 0) throw new Exception("No agents found for this chat");

		// Load history
		var history = new ChatHistory();
		var mems = await chatContext.ChatMemories
			.Where(cm => cm.ChatInstanceId == chatInstanceId)
			.OrderByDescending(cm => cm.CreatedAt)
			.Take(20)
			.ToListAsync(ct);
		mems.Reverse();

		foreach (var m in mems)
		{
			if (m.RoleType == RoleType.User) history.AddUserMessage(m.Content);
			else history.AddAssistantMessage(m.Content);
		}

		var session = new ChatSession
		{
			ChatInstanceId = chatInstanceId,
			History = history,
			OrchestrationType = chatDef.AgentOrchestrationType,
			ChatConfig = chatDef
		};

		_sessions.TryAdd(chatInstanceId, session);
	}

	private async Task EnsureSessionReadyAsync(ChatSession session, InvokeChatRequest request, IChatContext ctx, IKernelFactory kernelFactory, IEnumerable<IKernelProviderStrategy> strategies)
	{
		if (session.Kernel != null && session.CurrentSupplier == request.Supplier && session.CurrentModelName == request.ModelName) return;

		if (session.Kernel != null)
		{
			foreach (var res in session.Resources)
			{
				if (res is IDisposable d) d.Dispose();
			}
			session.Resources.Clear();
		}

		string modelName = request.ModelName;
		SupplierEnum supplier = request.Supplier;

		if (string.IsNullOrEmpty(modelName)) throw new Exception("Model Name is required");

		var (kernel, settings) = await kernelFactory.CreateKernelAsync(ctx, session, supplier, modelName, request.Ct);

		session.Kernel = kernel;
		session.CurrentSupplier = supplier;
		session.CurrentModelName = modelName;
		var instance = await ctx.ChatInstances.FindAsync([session.ChatInstanceId], request.Ct);
		var agentDefs = await ctx.ChatAgents
			.Join(ctx.Agents, ca => ca.AgentId, a => a.Id, (ca, a) => new { ca, a })
			.Where(x => x.ca.ChatId == instance!.ChatId)
			.OrderBy(x => x.ca.Order)
			.ToListAsync(request.Ct);

		var providerStrategy = strategies.FirstOrDefault(s => s.CanHandle(request.Supplier));

		session.Agents = [.. agentDefs.Select(x =>
		{
			dynamic agentSettings = providerStrategy?.CreateExecutionSettings((AgentVM)x.a) ?? settings;
			return new ChatCompletionAgent
			{
				Name = x.a.Name,
				Description = x.a.Description,
				Instructions = x.a.Instructions,
				Kernel = session.Kernel,
				Arguments = new(agentSettings)
			};
		})];
	}

	public async Task<(string text, Func<string, Task>)> ChatAsync(InvokeChatRequest chatRequest)
	{
		try
		{
			if (!_sessions.ContainsKey(chatRequest.ChatInstanceId))
			{
				await InitializeAsync(chatRequest.ChatInstanceId, chatRequest.Ct);
			}

			var session = _sessions[chatRequest.ChatInstanceId];

			using var scope = scopeFactory.CreateScope();
			var chatContext = scope.ServiceProvider.GetRequiredService<IChatContext>();
			var kernelFactory = scope.ServiceProvider.GetRequiredService<IKernelFactory>();
			var strategies = scope.ServiceProvider.GetRequiredService<IEnumerable<IKernelProviderStrategy>>();
			var mime = scope.ServiceProvider.GetRequiredService<IMimeType>();

			await EnsureSessionReadyAsync(session, chatRequest, chatContext, kernelFactory, strategies);

			var messageItems = new ChatMessageContentItemCollection
			{
				new TextContent(chatRequest.Text)
			};

			if (chatRequest.ImagePaths != null)
			{
				foreach (var path in chatRequest.ImagePaths)
				{
					if (File.Exists(path))
					{
						var bytes = await File.ReadAllBytesAsync(path, chatRequest.Ct);
						var imageData = new ReadOnlyMemory<byte>(bytes);
						var mimeType = await mime.Get(bytes);
						messageItems.Add(new ImageContent(imageData, mimeType));
					}
				}
			}

			session.History.AddUserMessage(messageItems);

			var strategy = orchestrationStrategies.FirstOrDefault(s => s.CanHandle(session.OrchestrationType));
			if (strategy == null)
			{
				// Fallback to default if not found, or throw
				strategy = orchestrationStrategies.First(s => s.CanHandle(AgentOrchestrationType.Single));
			}

			string resultText = await strategy.ExecuteAsync(session, chatRequest, chatContext);

			session.History.AddAssistantMessage(resultText);

			await chatContext.ChatMemories.AddRangeAsync(
			[
				new() {
					RoleType = RoleType.User,
					ChatInstanceId = chatRequest.ChatInstanceId,
					Content = chatRequest.Text,
					CreatedAt = DateTime.UtcNow,
				},
				new() {
					RoleType = RoleType.Assistant,
					ChatInstanceId = chatRequest.ChatInstanceId,
					Content = resultText,
					CreatedAt = DateTime.UtcNow,
				}
			], chatRequest.Ct);
			await chatContext.SaveChangesAsync(chatRequest.Ct);

			return (resultText, chatRequest.OnChunk ?? (_ => Task.CompletedTask));
		}
		catch (HttpOperationException ex)
		{
			throw new Exception($"AI Provider Error ({ex.StatusCode}): {ex.ResponseContent}", ex);
		}
		catch (Exception ex)
		{
			throw new Exception("handle error: " + ex.Message, ex);
		}
	}
}
