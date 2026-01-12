using Microsoft.EntityFrameworkCore;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Interface;
using NeroiStack.Agent.Model;
using NeroiStack.Common.Interface;

namespace NeroiStack.Agent.Service;

public class KeyManageService(IChatContext context, IEncryption encryption) : IKeyManageService
{
	private readonly IChatContext _context = context;
	private readonly IEncryption _encryption = encryption;

	public async Task SaveKeyAsync(KeyVM keyVM)
	{
		var encrypted = _encryption.Encrypt(keyVM.Key ?? string.Empty);

		var existing = await _context.Keys
			.Include(k => k.ModelsNav)
			.FirstOrDefaultAsync(k => k.Id == keyVM.Id);

		if (existing != null)
		{
			existing.Supplier = keyVM.Supplier;
			existing.EncryptedKey = encrypted;
			existing.Endpoint = keyVM.Endpoint ?? string.Empty;

			// Update models
			_context.KeyModels.RemoveRange(existing.ModelsNav);
			existing.ModelsNav.Clear();
			foreach (var model in keyVM.Models)
			{
				existing.ModelsNav.Add(new ChKeyModel { ModelId = model });
			}

			_context.Keys.Update(existing);
		}
		else
		{
			var newKey = new ChKey
			{
				Supplier = keyVM.Supplier,
				EncryptedKey = encrypted,
				Endpoint = keyVM.Endpoint ?? string.Empty
			};
			foreach (var model in keyVM.Models)
			{
				newKey.ModelsNav.Add(new ChKeyModel { ModelId = model });
			}
			await _context.Keys.AddAsync(newKey);
		}
		await _context.SaveChangesAsync();
	}

	public async Task<List<KeyVM>> GetAllKeysAsync()
	{
		var entities = await _context.Keys.Include(k => k.ModelsNav).ToListAsync();
		var result = new List<KeyVM>();

		foreach (var entity in entities)
		{
			try
			{
				result.Add(new KeyVM
				{
					Id = entity.Id,
					Supplier = entity.Supplier,
					Endpoint = entity.Endpoint,
					Key = _encryption.Decrypt(entity.EncryptedKey),
					Models = entity.ModelsNav.Select(m => m.ModelId).ToList()
				});
			}
			catch
			{
				// Failed to decrypt, ignore
			}
		}
		return result;
	}

	public async Task DeleteKeyAsync(Guid id)
	{
		var entity = await _context.Keys.FirstOrDefaultAsync(k => k.Id == id);
		if (entity != null)
		{
			_context.Keys.Remove(entity);
			await _context.SaveChangesAsync();
		}
	}
}