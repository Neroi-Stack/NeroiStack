using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Service;

namespace NeroiStack.Agent.UnitTest.Service;

public class ChatInstanceServiceTests
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
	public async Task GetInstancesAsync_ReturnsOrderedAndWithNames()
	{
		using var ctx = CreateInMemoryContext();
		ctx.Chats.Add(new ChChat { Id = 1, Name = "TheChat" });
		ctx.ChatInstances.Add(new ChChatInstance { Id = 10, ChatId = 1, CreatedAt = DateTime.Now.AddHours(-1) });
		ctx.ChatInstances.Add(new ChChatInstance { Id = 11, ChatId = 1, CreatedAt = DateTime.Now, ChatInstanceName = "InstanceA" });
		await ctx.SaveChangesAsync();

		var svc = new ChatInstanceService(ctx);
		var list = await svc.GetInstancesAsync();

		Assert.Equal(2, list.Count);
		Assert.Equal(11, list[0].Id);
		Assert.Equal("InstanceA", list[0].Name);
		Assert.Equal("TheChat", list[1].Name);
	}

	[Fact]
	public async Task CreateInstanceAsync_SetsNameFromChat()
	{
		using var ctx = CreateInMemoryContext();
		ctx.Chats.Add(new ChChat { Id = 5, Name = "MyChat" });
		await ctx.SaveChangesAsync();

		var svc = new ChatInstanceService(ctx);
		var vm = await svc.CreateInstanceAsync(5);

		Assert.Equal(5, vm.ChatId);
		Assert.Equal("MyChat", vm.Name);
		Assert.True(vm.Id > 0);
	}

	[Fact]
	public async Task CreateInstanceAsync_ChatMissing_NoName()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new ChatInstanceService(ctx);
		var vm = await svc.CreateInstanceAsync(99);
		Assert.Equal(99, vm.ChatId);
		Assert.True(string.IsNullOrEmpty(vm.Name));
	}

	[Fact]
	public async Task DeleteInstanceAsync_RemovesIfExists()
	{
		using var ctx = CreateInMemoryContext();
		ctx.ChatInstances.Add(new ChChatInstance { Id = 200, ChatId = 1 });
		await ctx.SaveChangesAsync();

		var svc = new ChatInstanceService(ctx);
		await svc.DeleteInstanceAsync(200);
		var e = await ctx.ChatInstances.FindAsync(200);
		Assert.Null(e);
	}

	[Fact]
	public async Task DeleteInstanceAsync_NoopIfMissing()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new ChatInstanceService(ctx);
		await svc.DeleteInstanceAsync(9999);
	}

	[Fact]
	public async Task UpdateSelectedModelAsync_UpdatesIfExists()
	{
		using var ctx = CreateInMemoryContext();
		ctx.ChatInstances.Add(new ChChatInstance { Id = 300, ChatId = 1, SelectedModel = "old" });
		await ctx.SaveChangesAsync();

		var svc = new ChatInstanceService(ctx);
		await svc.UpdateSelectedModelAsync(300, "new-model");
		var e = await ctx.ChatInstances.FindAsync(300);
		Assert.NotNull(e);
		Assert.Equal("new-model", e!.SelectedModel);
	}

	[Fact]
	public async Task UpdateSelectedModelAsync_NoopIfMissing()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new ChatInstanceService(ctx);
		await svc.UpdateSelectedModelAsync(9999, "x");
	}

	[Fact]
	public async Task UpdateChatInstanceNameAsync_UpdatesIfExists()
	{
		using var ctx = CreateInMemoryContext();
		ctx.ChatInstances.Add(new ChChatInstance { Id = 400, ChatId = 1, ChatInstanceName = "old" });
		await ctx.SaveChangesAsync();

		var svc = new ChatInstanceService(ctx);
		await svc.UpdateChatInstanceNameAsync(400, "new-name");
		var e = await ctx.ChatInstances.FindAsync(400);
		Assert.Equal("new-name", e!.ChatInstanceName);
	}

	[Fact]
	public async Task UpdateChatInstanceNameAsync_NoopIfMissing()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new ChatInstanceService(ctx);
		await svc.UpdateChatInstanceNameAsync(9999, "x");
	}
}
