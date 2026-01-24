using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Model;
using NeroiStack.Agent.Service;

namespace NeroiStack.Agent.UnitTest.Service;

public class ChatManageServiceTests
{
	private static ChatContext CreateInMemoryContext()
	{
		var connection = new SqliteConnection("DataSource=:memory:");
		connection.Open();
		var options = new DbContextOptionsBuilder<ChatContext>()
			.UseSqlite(connection)
			.Options;
		var ctx = new ChatContext(options);
		ctx.Database.EnsureCreated();
		return ctx;
	}

	[Fact]
	public async Task GetAsync_ReturnsChatsWithAgentsOrdered()
	{
		using var ctx = CreateInMemoryContext();
		ctx.Chats.Add(new ChChat { Id = 1, Name = "C1" });
		ctx.Chats.Add(new ChChat { Id = 2, Name = "C2" });
		ctx.ChatAgents.Add(new ChChatAgent { ChatId = 1, AgentId = 5, Order = 2 });
		ctx.ChatAgents.Add(new ChChatAgent { ChatId = 1, AgentId = 4, Order = 1 });
		ctx.ChatAgents.Add(new ChChatAgent { ChatId = 2, AgentId = 7, Order = 1 });
		await ctx.SaveChangesAsync();

		var svc = new ChatManageService(ctx);
		var list = await svc.GetAsync();

		Assert.Equal(2, list.Count);
		var c1 = list.First(c => c.Id == 1);
		Assert.Equal(2, c1.Agents.Count);
		Assert.Equal(4, c1.Agents[0].AgentId);
		Assert.Equal(5, c1.Agents[1].AgentId);
	}

	[Fact]
	public async Task CreateAsync_CreatesChatAndAgents()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new ChatManageService(ctx);
		var req = new CreateChatRequest { Name = "New", Agents = new System.Collections.Generic.List<ChatAgentVM> { new ChatAgentVM { AgentId = 10, Order = 1, IsPrimary = true } }, IsEnabled = true };
		var vm = await svc.CreateAsync(req);

		Assert.True(vm.Id > 0);
		Assert.Single(vm.Agents);
		var agents = await ctx.ChatAgents.Where(a => a.ChatId == vm.Id).ToListAsync();
		Assert.Single(agents);
		Assert.Equal(10, agents[0].AgentId);
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsChatWithAgentsAndHandoffs()
	{
		using var ctx = CreateInMemoryContext();
		ctx.Chats.Add(new ChChat { Id = 50, Name = "MyChat" });
		// Ensure referenced agents exist to satisfy FK constraints
		ctx.Agents.Add(new ChAgent { Id = 1, Name = "A1" });
		ctx.Agents.Add(new ChAgent { Id = 2, Name = "A2" });
		ctx.ChatAgents.Add(new ChChatAgent { ChatId = 50, AgentId = 1, Order = 1, IsPrimary = true });
		ctx.ChatHandoffs.Add(new ChChatHandoff { ChatId = 50, FromAgentId = 1, ToAgentId = 2, Description = "desc" });
		await ctx.SaveChangesAsync();

		var svc = new ChatManageService(ctx);
		var vm = await svc.GetByIdAsync(50);

		Assert.Equal(50, vm.Id);
		Assert.Single(vm.Agents);
		Assert.Single(vm.Handoffs);
	}

	[Fact]
	public async Task GetByIdAsync_NotFound_Throws()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new ChatManageService(ctx);
		await Assert.ThrowsAsync<Exception>(() => svc.GetByIdAsync(99999));
	}

	[Fact]
	public async Task UpdateAsync_ReplacesAgentsAndHandoffs()
	{
		using var ctx = CreateInMemoryContext();
		ctx.Chats.Add(new ChChat { Id = 70, Name = "Before" });
		// Ensure agents exist for FK constraints
		ctx.Agents.Add(new ChAgent { Id = 1, Name = "A1" });
		ctx.Agents.Add(new ChAgent { Id = 2, Name = "A2" });
		ctx.Agents.Add(new ChAgent { Id = 3, Name = "A3" });
		ctx.ChatAgents.Add(new ChChatAgent { ChatId = 70, AgentId = 1, Order = 1 });
		ctx.ChatHandoffs.Add(new ChChatHandoff { ChatId = 70, FromAgentId = 1, ToAgentId = 2, Description = "old" });
		await ctx.SaveChangesAsync();

		var svc = new ChatManageService(ctx);
		var vm = new ChatVM { Id = 70, Name = "After", Agents = new System.Collections.Generic.List<ChatAgentVM> { new ChatAgentVM { AgentId = 2, Order = 1, IsPrimary = true } }, Handoffs = new System.Collections.Generic.List<ChatHandoffVM> { new ChatHandoffVM { FromAgentId = 2, ToAgentId = 3, Description = "new" } } };
		var result = await svc.UpdateAsync(vm);

		Assert.Equal("After", result.Name);
		var agents = await ctx.ChatAgents.Where(a => a.ChatId == 70).ToListAsync();
		Assert.Single(agents);
		Assert.Equal(2, agents[0].AgentId);
		var handoffs = await ctx.ChatHandoffs.Where(h => h.ChatId == 70).ToListAsync();
		Assert.Single(handoffs);
		Assert.Equal(3, handoffs[0].ToAgentId);
	}

	[Fact]
	public async Task UpdateAsync_NotFound_Throws()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new ChatManageService(ctx);
		var vm = new ChatVM { Id = 9999 };
		await Assert.ThrowsAsync<Exception>(() => svc.UpdateAsync(vm));
	}

	[Fact]
	public async Task DeleteAsync_RemovesChat()
	{
		using var ctx = CreateInMemoryContext();
		ctx.Chats.Add(new ChChat { Id = 80, Name = "ToDelete" });
		await ctx.SaveChangesAsync();

		var svc = new ChatManageService(ctx);
		await svc.DeleteAsync(80);
		var chat = await ctx.Chats.FindAsync(80);
		Assert.Null(chat);
	}

	[Fact]
	public async Task DeleteAsync_NotFound_Throws()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new ChatManageService(ctx);
		await Assert.ThrowsAsync<Exception>(() => svc.DeleteAsync(424242));
	}

	private class FailSaveContext : ChatContext
	{
		public FailSaveContext(DbContextOptions<ChatContext> options) : base(options) { }
		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			throw new Exception("Simulated DB failure");
		}
	}

	private static FailSaveContext CreateFailSaveContext()
	{
		var connection = new SqliteConnection("DataSource=:memory:");
		connection.Open();
		var options = new DbContextOptionsBuilder<ChatContext>()
			.UseSqlite(connection)
			.Options;
		var ctx = new FailSaveContext(options);
		ctx.Database.EnsureCreated();
		return ctx;
	}

	[Fact]
	public async Task CreateAsync_SaveChangesThrows_Throws()
	{
		using var ctx = CreateFailSaveContext();
		var svc = new ChatManageService(ctx);
		var req = new CreateChatRequest { Name = "N", Agents = [new ChatAgentVM { AgentId = 1 }] };
		await Assert.ThrowsAsync<Exception>(() => svc.CreateAsync(req));
	}

	[Fact]
	public async Task UpdateAsync_SaveChangesThrows_Throws()
	{
		using var ctx = CreateFailSaveContext();
		ctx.Chats.Add(new ChChat { Id = 90, Name = "Existing" });
		var svc = new ChatManageService(ctx);
		var vm = new ChatVM { Id = 90, Name = "X", Agents = [new ChatAgentVM { AgentId = 2 }] };
		await Assert.ThrowsAsync<Exception>(() => svc.UpdateAsync(vm));
	}

	[Fact]
	public async Task CreateAsync_NullAgents_DoesNotThrowAndAgentsEmpty()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new ChatManageService(ctx);
		var req = new CreateChatRequest { Name = "NoAgents", IsEnabled = true };
		var vm = await svc.CreateAsync(req);
		Assert.Empty(vm.Agents ?? []);
		var agents = await ctx.ChatAgents.Where(a => a.ChatId == vm.Id).ToListAsync();
		Assert.Empty(agents);
	}

	[Fact]
	public async Task UpdateAsync_NullAgentsAndHandoffs_RemovesExisting()
	{
		using var ctx = CreateInMemoryContext();
		ctx.Chats.Add(new ChChat { Id = 2000, Name = "C" });
		ctx.Agents.Add(new ChAgent { Id = 10, Name = "A10" });
		ctx.ChatAgents.Add(new ChChatAgent { ChatId = 2000, AgentId = 10, Order = 1 });
		ctx.ChatHandoffs.Add(new ChChatHandoff { ChatId = 2000, FromAgentId = 10, ToAgentId = 10, Description = "d" });
		await ctx.SaveChangesAsync();

		var svc = new ChatManageService(ctx);
		var vm = new ChatVM { Id = 2000, Name = "C2", Agents = [], Handoffs = [] };
		var res = await svc.UpdateAsync(vm);
		var agents = await ctx.ChatAgents.Where(a => a.ChatId == 2000).ToListAsync();
		var handoffs = await ctx.ChatHandoffs.Where(h => h.ChatId == 2000).ToListAsync();
		Assert.Empty(agents);
		Assert.Empty(handoffs);
	}
}
