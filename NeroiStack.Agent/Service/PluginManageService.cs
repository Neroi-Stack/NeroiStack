using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Interface;
using NeroiStack.Agent.Model;
using NeroiStack.Common.Interface;

namespace NeroiStack.Agent.Service;

public class PluginManageService(IChatContext context, ILogger<PluginManageService> logger, IEncryption encryption) : IPluginManageService
{
	private readonly IChatContext _context = context;
	private readonly ILogger<PluginManageService> _logger = logger;
	private readonly IEncryption _encryption = encryption;

	public async Task<List<PluginBaseVM>> GetPluginsAsync()
		=> await _context.Plugins.Select(p => (PluginBaseVM)p).ToListAsync();

	public async Task<PluginBaseVM?> GetPluginAsync(int id)
	{
		var plugin = await _context.Plugins.FindAsync(id);
		if (plugin == null) return null;

		PluginBaseVM? vm = null;

		switch (plugin.Type)
		{
			case PluginType.McpHttp:
				var mcpHttp = await _context.PluginMcpHttps.FirstOrDefaultAsync(p => p.PluginId == id);
				if (mcpHttp != null)
				{
					mcpHttp.ApiKey = mcpHttp.ApiKey == null ? null : _encryption.Decrypt(mcpHttp.ApiKey);
					vm = (PluginMcpHttpVM)mcpHttp;
				}
				break;
			case PluginType.McpStdio:
				var mcpStdio = await _context.PluginMcpStdios.FirstOrDefaultAsync(p => p.PluginId == id);
				if (mcpStdio != null) vm = (ChPluginMcpStdioVM)mcpStdio;
				break;
			case PluginType.OpenApi:
				var openApi = await _context.PluginOpenApis.FirstOrDefaultAsync(p => p.PluginId == id);
				if (openApi != null)
				{
					openApi.ApiKey = openApi.ApiKey == null ? null : _encryption.Decrypt(openApi.ApiKey);
					openApi.BearerToken = openApi.BearerToken == null ? null : _encryption.Decrypt(openApi.BearerToken);
					vm = (ChPluginOpenApiVM)openApi;
				}
				break;
		}

		if (vm == null)
		{
			vm = (PluginBaseVM)plugin;
		}
		else
		{
			vm.Id = plugin.Id;
			vm.Name = plugin.Name;
			vm.Description = plugin.Description;
			vm.Type = plugin.Type;
			vm.IsEnabled = plugin.IsEnabled;
		}

		return vm;
	}

	public async Task<int> CreatePluginAsync(PluginBaseVM pluginVm)
	{
		var plugin = new ChPlugin
		{
			Name = pluginVm.Name,
			Description = pluginVm.Description,
			Type = pluginVm.Type,
			IsEnabled = pluginVm.IsEnabled
		};
		_context.Plugins.Add(plugin);
		await _context.SaveChangesAsync();
		pluginVm.Id = plugin.Id;

		switch (pluginVm.Type)
		{
			case PluginType.McpHttp:
				if (pluginVm is PluginMcpHttpVM mcpHttpVm)
				{
					var pluginMcpHttp = new ChPluginMcpHttp
					{
						PluginId = plugin.Id,
						Endpoint = mcpHttpVm.Endpoint,
						ApiKey = string.IsNullOrWhiteSpace(mcpHttpVm.ApiKey) ? null : _encryption.Encrypt(mcpHttpVm.ApiKey)
					};
					_context.PluginMcpHttps.Add(pluginMcpHttp);
				}
				break;
			case PluginType.McpStdio:
				if (pluginVm is ChPluginMcpStdioVM mcpStdioVm)
				{
					_context.PluginMcpStdios.Add(new ChPluginMcpStdio
					{
						PluginId = plugin.Id,
						Command = mcpStdioVm.Command,
						Arguments = mcpStdioVm.Arguments
					});
				}
				break;
			case PluginType.OpenApi:
				if (pluginVm is ChPluginOpenApiVM openApiVm)
				{
					var pluginOpenApi = new ChPluginOpenApi
					{
						PluginId = plugin.Id,
						AuthType = openApiVm.AuthType,
						ApiKey = string.IsNullOrWhiteSpace(openApiVm.ApiKey) ? null : _encryption.Encrypt(openApiVm.ApiKey),
						BearerToken = string.IsNullOrWhiteSpace(openApiVm.BearerToken) ? null : _encryption.Encrypt(openApiVm.BearerToken),
						Uri = openApiVm.Uri,
						FilePath = openApiVm.FilePath
					};
					_context.PluginOpenApis.Add(pluginOpenApi);
				}
				break;
		}
		await _context.SaveChangesAsync();
		return plugin.Id;
	}

	public async Task UpdatePluginAsync(PluginBaseVM pluginVm)
	{
		var plugin = await _context.Plugins.FindAsync(pluginVm.Id);
		if (plugin == null) return;

		plugin.Name = pluginVm.Name;
		plugin.Description = pluginVm.Description;
		plugin.IsEnabled = pluginVm.IsEnabled;

		switch (plugin.Type)
		{
			case PluginType.McpHttp:
				if (pluginVm is PluginMcpHttpVM mcpHttpVm)
				{
					var entity = await _context.PluginMcpHttps.FirstOrDefaultAsync(p => p.PluginId == plugin.Id);
					if (entity != null)
					{
						entity.Endpoint = mcpHttpVm.Endpoint;
						entity.ApiKey = string.IsNullOrWhiteSpace(mcpHttpVm.ApiKey) ? null : _encryption.Encrypt(mcpHttpVm.ApiKey);
					}
				}
				break;
			case PluginType.McpStdio:
				if (pluginVm is ChPluginMcpStdioVM mcpStdioVm)
				{
					var entity = await _context.PluginMcpStdios.FirstOrDefaultAsync(p => p.PluginId == plugin.Id);
					if (entity != null)
					{
						entity.Command = mcpStdioVm.Command;
						entity.Arguments = mcpStdioVm.Arguments;
					}
				}
				break;
			case PluginType.OpenApi:
				if (pluginVm is ChPluginOpenApiVM openApiVm)
				{
					var entity = await _context.PluginOpenApis.FirstOrDefaultAsync(p => p.PluginId == plugin.Id);
					if (entity != null)
					{
						entity.AuthType = openApiVm.AuthType;
						entity.ApiKey = string.IsNullOrWhiteSpace(openApiVm.ApiKey) ? null : _encryption.Encrypt(openApiVm.ApiKey);
						entity.BearerToken = string.IsNullOrWhiteSpace(openApiVm.BearerToken) ? null : _encryption.Encrypt(openApiVm.BearerToken);
						entity.Uri = openApiVm.Uri;
						entity.FilePath = openApiVm.FilePath;
					}
				}
				break;
		}
		await _context.SaveChangesAsync();
	}

	public async Task DeletePluginAsync(int id)
	{
		var plugin = await _context.Plugins.FindAsync(id);
		if (plugin == null) return;

		switch (plugin.Type)
		{
			case PluginType.McpHttp:
				var mcpHttp = await _context.PluginMcpHttps.FirstOrDefaultAsync(p => p.PluginId == id);
				if (mcpHttp != null) _context.PluginMcpHttps.Remove(mcpHttp);
				break;
			case PluginType.McpStdio:
				var mcpStdio = await _context.PluginMcpStdios.FirstOrDefaultAsync(p => p.PluginId == id);
				if (mcpStdio != null) _context.PluginMcpStdios.Remove(mcpStdio);
				break;
			case PluginType.OpenApi:
				var openApi = await _context.PluginOpenApis.FirstOrDefaultAsync(p => p.PluginId == id);
				if (openApi != null) _context.PluginOpenApis.Remove(openApi);
				break;
		}

		_context.Plugins.Remove(plugin);
		await _context.SaveChangesAsync();
	}

	public async Task<List<PluginBaseDropDownVM>> GetPluginsDropDownAsync()
		=> await _context.Plugins.Where(p => p.IsEnabled).Select(p => (PluginBaseDropDownVM)p).ToListAsync();
}
