using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;
using NeroiStack.Agent.Service;
using NeroiStack.Common.Interface;

namespace NeroiStack.Agent.UnitTest.Service;

public class PluginManageServiceTests
{
	private class FakeEncryption : IEncryption
	{
		public string Encrypt(string plainText) => "enc:" + plainText;
		public string Decrypt(string cipherText) => cipherText.StartsWith("enc:") ? cipherText.Substring(4) : cipherText;
	}

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
	public async Task GetPluginsAsync_ReturnsList()
	{
		using var ctx = CreateInMemoryContext();
		ctx.Plugins.Add(new ChPlugin { Id = 1, Name = "P1", Type = PluginType.McpHttp });
		ctx.Plugins.Add(new ChPlugin { Id = 2, Name = "P2", Type = PluginType.OpenApi });
		await ctx.SaveChangesAsync();

		var svc = new PluginManageService(ctx, NullLogger<PluginManageService>.Instance, new FakeEncryption());
		var list = await svc.GetPluginsAsync();
		Assert.Equal(2, list.Count);
	}

	[Fact]
	public async Task GetPluginAsync_McpHttp_DecryptsApiKey()
	{
		using var ctx = CreateInMemoryContext();
		var plugin = new ChPlugin { Id = 10, Name = "Mcp", Type = PluginType.McpHttp };
		await ctx.Plugins.AddAsync(plugin);
		await ctx.PluginMcpHttps.AddAsync(new ChPluginMcpHttp { PluginId = 10, Endpoint = "/", ApiKey = "enc:key" });
		await ctx.SaveChangesAsync();

		var svc = new PluginManageService(ctx, NullLogger<PluginManageService>.Instance, new FakeEncryption());
		var vm = await svc.GetPluginAsync(10);
		Assert.IsType<PluginMcpHttpVM>(vm);
		var http = (PluginMcpHttpVM)vm!;
		Assert.Equal("key", http.ApiKey);
	}

	[Fact]
	public async Task GetPluginAsync_OpenApi_DecryptsSecrets()
	{
		using var ctx = CreateInMemoryContext();
		var plugin = new ChPlugin { Id = 20, Name = "Open", Type = PluginType.OpenApi };
		await ctx.Plugins.AddAsync(plugin);
		await ctx.PluginOpenApis.AddAsync(new ChPluginOpenApi { PluginId = 20, ApiKey = "enc:ak", BearerToken = "enc:bt", Uri = "u" });
		await ctx.SaveChangesAsync();

		var svc = new PluginManageService(ctx, NullLogger<PluginManageService>.Instance, new FakeEncryption());
		var vm = await svc.GetPluginAsync(20);
		Assert.IsType<ChPluginOpenApiVM>(vm);
		var open = (ChPluginOpenApiVM)vm!;
		Assert.Equal("ak", open.ApiKey);
		Assert.Equal("bt", open.BearerToken);
	}

	[Fact]
	public async Task CreatePluginAsync_CreatesSubEntity_AndEncrypts()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new PluginManageService(ctx, NullLogger<PluginManageService>.Instance, new FakeEncryption());

		var vm = new PluginMcpHttpVM { Name = "New", Type = PluginType.McpHttp, Endpoint = "/", ApiKey = "secret" };
		var id = await svc.CreatePluginAsync(vm);
		var plugin = await ctx.Plugins.FindAsync(id);
		Assert.NotNull(plugin);
		var mcp = await ctx.PluginMcpHttps.FirstOrDefaultAsync(p => p.PluginId == id);
		Assert.NotNull(mcp);
		Assert.Equal("enc:secret", mcp.ApiKey);
	}

	[Fact]
	public async Task UpdatePluginAsync_UpdatesNestedEntities()
	{
		using var ctx = CreateInMemoryContext();
		var plugin = new ChPlugin { Id = 30, Name = "P", Type = PluginType.SqlAgentTool };
		await ctx.Plugins.AddAsync(plugin);
		await ctx.PluginSqls.AddAsync(new ChPluginSql { PluginId = 30, Provider = "Sqlite", ConnectionString = "enc:cs" });
		await ctx.SaveChangesAsync();

		var svc = new PluginManageService(ctx, NullLogger<PluginManageService>.Instance, new FakeEncryption());
		var vm = new ChPluginSqlVM { Id = 30, Type = PluginType.SqlAgentTool, ConnectionString = "newcs", Provider = "My" };
		await svc.UpdatePluginAsync(vm);

		var sql = await ctx.PluginSqls.FirstOrDefaultAsync(p => p.PluginId == 30);
		Assert.NotNull(sql);
		Assert.Equal("enc:newcs", sql.ConnectionString);
		Assert.Equal("My", sql.Provider);
	}

	[Fact]
	public async Task DeletePluginAsync_RemovesPluginAndSubtype()
	{
		using var ctx = CreateInMemoryContext();
		var plugin = new ChPlugin { Id = 40, Name = "P", Type = PluginType.McpStdio };
		await ctx.Plugins.AddAsync(plugin);
		await ctx.PluginMcpStdios.AddAsync(new ChPluginMcpStdio { PluginId = 40, Command = "cmd" });
		await ctx.SaveChangesAsync();

		var svc = new PluginManageService(ctx, NullLogger<PluginManageService>.Instance, new FakeEncryption());
		await svc.DeletePluginAsync(40);
		var p = await ctx.Plugins.FindAsync(40);
		var s = await ctx.PluginMcpStdios.FirstOrDefaultAsync(x => x.PluginId == 40);
		Assert.Null(p);
		Assert.Null(s);
	}

	[Fact]
	public async Task GetPluginsDropDownAsync_ReturnsOnlyEnabled()
	{
		using var ctx = CreateInMemoryContext();
		ctx.Plugins.Add(new ChPlugin { Id = 1, Name = "A", IsEnabled = true });
		ctx.Plugins.Add(new ChPlugin { Id = 2, Name = "B", IsEnabled = false });
		await ctx.SaveChangesAsync();

		var svc = new PluginManageService(ctx, NullLogger<PluginManageService>.Instance, new FakeEncryption());
		var list = await svc.GetPluginsDropDownAsync();
		Assert.Single(list);
		Assert.Equal("A", list[0].Name);
	}

	[Fact]
	public async Task GetPluginAsync_MissingPlugin_ReturnsNull()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new PluginManageService(ctx, NullLogger<PluginManageService>.Instance, new FakeEncryption());
		var vm = await svc.GetPluginAsync(9999);
		Assert.Null(vm);
	}

	private class ThrowOnDecrypt : IEncryption
	{
		public string Encrypt(string plainText) => "enc:" + plainText;
		public string Decrypt(string cipherText) => throw new Exception("decrypt fail");
	}

	[Fact]
	public async Task GetPluginAsync_DecryptionThrows_Propagates()
	{
		using var ctx = CreateInMemoryContext();
		var plugin = new ChPlugin { Id = 60, Name = "Mcp", Type = PluginType.McpHttp };
		await ctx.Plugins.AddAsync(plugin);
		await ctx.PluginMcpHttps.AddAsync(new ChPluginMcpHttp { PluginId = 60, Endpoint = "/", ApiKey = "enc:bad" });
		await ctx.SaveChangesAsync();

		var svc = new PluginManageService(ctx, NullLogger<PluginManageService>.Instance, new ThrowOnDecrypt());
		await Assert.ThrowsAsync<Exception>(() => svc.GetPluginAsync(60));
	}

	private class ThrowOnEncrypt : IEncryption
	{
		public string Encrypt(string plainText) => throw new Exception("encrypt fail");
		public string Decrypt(string cipherText) => cipherText;
	}

	[Fact]
	public async Task CreatePluginAsync_EncryptionThrows_Propagates()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new PluginManageService(ctx, NullLogger<PluginManageService>.Instance, new ThrowOnEncrypt());
		var vm = new PluginMcpHttpVM { Name = "New", Type = PluginType.McpHttp, Endpoint = "/", ApiKey = "secret" };
		await Assert.ThrowsAsync<Exception>(() => svc.CreatePluginAsync(vm));
	}

	[Fact]
	public async Task UpdatePluginAsync_PluginMissing_NoThrow()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new PluginManageService(ctx, NullLogger<PluginManageService>.Instance, new FakeEncryption());
		var vm = new PluginMcpHttpVM { Id = 9999, Name = "X", Type = PluginType.McpHttp, Endpoint = "/" };
		await svc.UpdatePluginAsync(vm);
	}

	[Fact]
	public async Task UpdatePluginAsync_EncryptionThrows_Propagates()
	{
		using var ctx = CreateInMemoryContext();
		var plugin = new ChPlugin { Id = 70, Name = "P", Type = PluginType.OpenApi };
		await ctx.Plugins.AddAsync(plugin);
		await ctx.PluginOpenApis.AddAsync(new ChPluginOpenApi { PluginId = 70, ApiKey = "enc:a", BearerToken = "enc:b", Uri = "u" });
		await ctx.SaveChangesAsync();

		var svc = new PluginManageService(ctx, NullLogger<PluginManageService>.Instance, new ThrowOnEncrypt());
		var vm = new ChPluginOpenApiVM { Id = 70, Type = PluginType.OpenApi, ApiKey = "new", BearerToken = "new" };
		await Assert.ThrowsAsync<Exception>(() => svc.UpdatePluginAsync(vm));
	}

	[Fact]
	public async Task CreatePluginAsync_MissingSubtype_NoSubtypeCreated()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new PluginManageService(ctx, NullLogger<PluginManageService>.Instance, new FakeEncryption());
		var vm = new PluginBaseVM { Name = "Base", Type = PluginType.OpenApi };
		var id = await svc.CreatePluginAsync(vm);
		var open = await ctx.PluginOpenApis.FirstOrDefaultAsync(p => p.PluginId == id);
		Assert.Null(open);
	}

	[Fact]
	public async Task DeletePluginAsync_NotFound_NoThrow()
	{
		using var ctx = CreateInMemoryContext();
		var svc = new PluginManageService(ctx, NullLogger<PluginManageService>.Instance, new FakeEncryption());
		await svc.DeletePluginAsync(123456);
	}
}
