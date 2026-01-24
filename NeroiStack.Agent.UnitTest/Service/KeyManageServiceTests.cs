using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;
using NeroiStack.Agent.Service;
using NeroiStack.Common.Interface;

namespace NeroiStack.Agent.UnitTest.Service;

public class KeyManageServiceTests
{
	private class FakeEncryption : IEncryption
	{
		public string Encrypt(string plainText)
		{
			return "enc:" + plainText;
		}

		public string Decrypt(string cipherText)
		{
			if (cipherText == "bad-enc") throw new Exception("decrypt failed");
			return cipherText.StartsWith("enc:") ? cipherText.Substring(4) : cipherText;
		}
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
	public async Task SaveKeyAsync_CreatesNewKeyAndModels()
	{
		using var ctx = CreateInMemoryContext();
		var enc = new FakeEncryption();
		var svc = new KeyManageService(ctx, enc);

		var vm = new KeyVM { Supplier = SupplierEnum.OpenAI, KeyType = KeyType.Chat, Endpoint = "ep", Key = "secret", Models = new List<string> { "m1", "m2" } };
		await svc.SaveKeyAsync(vm);

		var keys = await ctx.Keys.Include(k => k.ModelsNav).ToListAsync();
		Assert.Single(keys);
		var k = keys[0];
		Assert.Equal(SupplierEnum.OpenAI, k.Supplier);
		Assert.Equal("enc:secret", k.EncryptedKey);
		Assert.Equal(2, k.ModelsNav.Count);
		Assert.Contains(k.ModelsNav, m => m.ModelId == "m1");
	}

	[Fact]
	public async Task SaveKeyAsync_UpdateExisting_ReplacesModelsAndKey()
	{
		using var ctx = CreateInMemoryContext();
		var enc = new FakeEncryption();

		var existing = new ChKey { Id = Guid.NewGuid(), Supplier = SupplierEnum.OpenAI, KeyType = KeyType.Chat, EncryptedKey = enc.Encrypt("old"), Endpoint = "e" };
		existing.ModelsNav.Add(new ChKeyModel { ModelId = "old1" });
		existing.ModelsNav.Add(new ChKeyModel { ModelId = "old2" });
		await ctx.Keys.AddAsync(existing);
		await ctx.SaveChangesAsync();

		var svc = new KeyManageService(ctx, enc);
		var vm = new KeyVM { Id = existing.Id, Supplier = SupplierEnum.AzureOpenAI, KeyType = KeyType.Embedding, Endpoint = "e2", Key = "newk", Models = new List<string> { "new1" } };
		await svc.SaveKeyAsync(vm);

		var updated = await ctx.Keys.Include(k => k.ModelsNav).FirstOrDefaultAsync(k => k.Id == existing.Id);
		Assert.NotNull(updated);
		Assert.Equal(SupplierEnum.AzureOpenAI, updated.Supplier);
		Assert.Equal("enc:newk", updated.EncryptedKey);
		Assert.Single(updated.ModelsNav);
		Assert.Equal("new1", updated.ModelsNav.First().ModelId);
	}

	[Fact]
	public async Task GetAllKeysAsync_IgnoresFailedDecrypt()
	{
		using var ctx = CreateInMemoryContext();
		var enc = new FakeEncryption();

		var good = new ChKey { Id = Guid.NewGuid(), Supplier = SupplierEnum.OpenAI, KeyType = KeyType.Chat, EncryptedKey = enc.Encrypt("g"), Endpoint = "e" };
		good.ModelsNav.Add(new ChKeyModel { ModelId = "m" });
		var bad = new ChKey { Id = Guid.NewGuid(), Supplier = SupplierEnum.Google, KeyType = KeyType.Chat, EncryptedKey = "bad-enc", Endpoint = "e2" };
		await ctx.Keys.AddRangeAsync(good, bad);
		await ctx.SaveChangesAsync();

		var svc = new KeyManageService(ctx, enc);
		var all = await svc.GetAllKeysAsync();
		Assert.Single(all);
		Assert.Equal("g", all[0].Key);
		Assert.Single(all[0].Models);
	}

	[Fact]
	public async Task DeleteKeyAsync_RemovesAndNoopIfMissing()
	{
		using var ctx = CreateInMemoryContext();
		var enc = new FakeEncryption();
		var svc = new KeyManageService(ctx, enc);

		var k = new ChKey { Id = Guid.NewGuid(), Supplier = SupplierEnum.OpenAI, KeyType = KeyType.Chat, EncryptedKey = enc.Encrypt("x"), Endpoint = "e" };
		await ctx.Keys.AddAsync(k);
		await ctx.SaveChangesAsync();

		await svc.DeleteKeyAsync(k.Id);
		var found = await ctx.Keys.FindAsync(k.Id);
		Assert.Null(found);

		// No exception for missing
		await svc.DeleteKeyAsync(Guid.NewGuid());
	}

	[Fact]
	public async Task SaveKeyAsync_EmptyModels_DoesNotFailAndNoModelsSaved()
	{
		using var ctx = CreateInMemoryContext();
		var enc = new FakeEncryption();
		var svc = new KeyManageService(ctx, enc);

		var vm = new KeyVM { Supplier = SupplierEnum.OpenAI, KeyType = KeyType.Embedding, Endpoint = "ep", Key = "secret", Models = new List<string>() };
		await svc.SaveKeyAsync(vm);

		var keys = await ctx.Keys.Include(k => k.ModelsNav).ToListAsync();
		Assert.Single(keys);
		Assert.Empty(keys[0].ModelsNav);
	}

	[Fact]
	public async Task SaveKeyAsync_UpdateExisting_WithEmptyModels_ClearsPreviousModels()
	{
		using var ctx = CreateInMemoryContext();
		var enc = new FakeEncryption();

		var existing = new ChKey { Id = Guid.NewGuid(), Supplier = SupplierEnum.OpenAI, KeyType = KeyType.TTS, EncryptedKey = enc.Encrypt("old"), Endpoint = "e" };
		existing.ModelsNav.Add(new ChKeyModel { ModelId = "old1" });
		await ctx.Keys.AddAsync(existing);
		await ctx.SaveChangesAsync();

		var svc = new KeyManageService(ctx, enc);
		var vm = new KeyVM { Id = existing.Id, Supplier = SupplierEnum.OpenAI, KeyType = KeyType.TTS, Endpoint = "e", Key = "newk", Models = new List<string>() };
		await svc.SaveKeyAsync(vm);

		var updated = await ctx.Keys.Include(k => k.ModelsNav).FirstOrDefaultAsync(k => k.Id == existing.Id);
		Assert.NotNull(updated);
		Assert.Empty(updated.ModelsNav);
	}

	private class ThrowOnEncrypt : IEncryption
	{
		public string Encrypt(string plainText) => throw new Exception("encrypt failure");
		public string Decrypt(string cipherText) => cipherText;
	}

	[Fact]
	public async Task SaveKeyAsync_EncryptionThrows_Propagates()
	{
		using var ctx = CreateInMemoryContext();
		var enc = new ThrowOnEncrypt();
		var svc = new KeyManageService(ctx, enc);
		var vm = new KeyVM { Supplier = SupplierEnum.OpenAI, KeyType = KeyType.Chat, Endpoint = "ep", Key = "secret", Models = new List<string> { "m" } };
		await Assert.ThrowsAsync<Exception>(() => svc.SaveKeyAsync(vm));
	}

	[Fact]
	public async Task GetAllKeysAsync_NoKeys_ReturnsEmpty()
	{
		using var ctx = CreateInMemoryContext();
		var enc = new FakeEncryption();
		var svc = new KeyManageService(ctx, enc);
		var all = await svc.GetAllKeysAsync();
		Assert.Empty(all);
	}
}

