using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Model;
using NeroiStack.Agent.Service;

namespace NeroiStack.Agent.UnitTest.Service;

public class AgentManageServiceTests
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
	public async Task GetAsync_ReturnsAgentsWithPluginIds()
	{
		using var ctx = CreateInMemoryContext();
		ctx.Agents.Add(new ChAgent { Id = 1, Name = "A1" });
		ctx.Agents.Add(new ChAgent { Id = 2, Name = "A2" });
		ctx.AgentPlugins.Add(new ChAgentPlugin { AgentId = 1, PluginId = 10 });
		ctx.AgentPlugins.Add(new ChAgentPlugin { AgentId = 1, PluginId = 11 });
		ctx.AgentPlugins.Add(new ChAgentPlugin { AgentId = 2, PluginId = 20 });
		await ctx.SaveChangesAsync();

		var svc = new AgentManageService(ctx);
		var list = await svc.GetAsync();

		Assert.Equal(2, list.Count);
		var a1 = list.First(a => a.Id == 1);
		Assert.Contains(10, a1.PluginIds);
		Assert.Contains(11, a1.PluginIds);
	}

	[Fact]
	public async Task GetByIdAsync_NotFound_Throws()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new AgentManageService(ctx);
		await Assert.ThrowsAsync<Exception>(() => svc.GetByIdAsync(999));
	}

	[Fact]
	public async Task CreateAsync_AddsAgentAndPlugins()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new AgentManageService(ctx);
		var req = new CreateAgentRequest { Name = "New", PluginIds = new[] { 1, 2 } };
		var vm = await svc.CreateAsync(req);
		Assert.True(vm.Id > 0);
		var plugins = await ctx.AgentPlugins.Where(p => p.AgentId == vm.Id).ToListAsync();
		Assert.Equal(2, plugins.Count);
	}

	[Fact]
	public async Task UpdateAsync_UpdatesAgentAndPlugins()
	{
		using var ctx = CreateInMemoryContext();
		ctx.Agents.Add(new ChAgent { Id = 5, Name = "Old" });
		ctx.AgentPlugins.Add(new ChAgentPlugin { AgentId = 5, PluginId = 1 });
		ctx.AgentPlugins.Add(new ChAgentPlugin { AgentId = 5, PluginId = 2 });
		await ctx.SaveChangesAsync();

		var svc = new AgentManageService(ctx);
		var vm = new AgentVM { Id = 5, Name = "New", PluginIds = new[] { 2, 3 } };
		await svc.UpdateAsync(vm);

		var agent = await ctx.Agents.FindAsync(5);
		Assert.Equal("New", agent?.Name);
		var pluginIds = await ctx.AgentPlugins.Where(p => p.AgentId == 5).Select(p => p.PluginId).ToListAsync();
		Assert.Contains(2, pluginIds);
		Assert.Contains(3, pluginIds);
	}

	[Fact]
	public async Task DeleteAsync_RemovesAgent()
	{
		using var ctx = CreateInMemoryContext();
		ctx.Agents.Add(new ChAgent { Id = 7, Name = "ToDelete" });
		await ctx.SaveChangesAsync();

		var svc = new AgentManageService(ctx);
		await svc.DeleteAsync(7);
		var agent = await ctx.Agents.FindAsync(7);
		Assert.Null(agent);
	}

	[Fact]
	public async Task GetDropdownAsync_ReturnsOnlyEnabled()
	{
		using var ctx = CreateInMemoryContext();
		ctx.Agents.Add(new ChAgent { Id = 100, Name = "On", IsEnabled = true });
		ctx.Agents.Add(new ChAgent { Id = 101, Name = "Off", IsEnabled = false });
		await ctx.SaveChangesAsync();

		var svc = new AgentManageService(ctx);
		var dropdown = await svc.GetDropdownAsync();
		Assert.Single(dropdown);
		Assert.Equal(100, dropdown[0].Id);
	}

	[Fact]
	public async Task UpdateAsync_NotFound_Throws()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new AgentManageService(ctx);
		var vm = new AgentVM { Id = 999, Name = "NonExistent", PluginIds = new int[0] };
		await Assert.ThrowsAsync<Exception>(() => svc.UpdateAsync(vm));
	}

	[Fact]
	public async Task DeleteAsync_NotFound_Throws()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new AgentManageService(ctx);
		await Assert.ThrowsAsync<Exception>(() => svc.DeleteAsync(12345));
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
		var svc = new AgentManageService(ctx);
		var req = new CreateAgentRequest { Name = "New", PluginIds = new[] { 1 } };
		await Assert.ThrowsAsync<Exception>(() => svc.CreateAsync(req));
	}

	[Fact]
	public async Task UpdateAsync_SaveChangesThrows_Throws()
	{
		using var ctx = CreateFailSaveContext();
		ctx.Agents.Add(new ChAgent { Id = 50, Name = "Existing" });
		var svc = new AgentManageService(ctx);
		var vm = new AgentVM { Id = 50, Name = "Updated", PluginIds = new int[0] };
		await Assert.ThrowsAsync<Exception>(() => svc.UpdateAsync(vm));
	}
}
