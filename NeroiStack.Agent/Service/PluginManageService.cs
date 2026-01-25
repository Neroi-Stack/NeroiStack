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
					string decryptedApiKey = string.IsNullOrWhiteSpace(mcpHttp.ApiKey) ? string.Empty : _encryption.Decrypt(mcpHttp.ApiKey);
					var tmpVm = (PluginMcpHttpVM)mcpHttp;
					tmpVm.ApiKey = decryptedApiKey;
					vm = tmpVm;
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
					string decryptedApiKey = string.IsNullOrWhiteSpace(openApi.ApiKey) ? string.Empty : _encryption.Decrypt(openApi.ApiKey);
					string decryptedBearer = string.IsNullOrWhiteSpace(openApi.BearerToken) ? string.Empty : _encryption.Decrypt(openApi.BearerToken);
					var tmpVm = (ChPluginOpenApiVM)openApi;
					tmpVm.ApiKey = decryptedApiKey;
					tmpVm.BearerToken = decryptedBearer;
					vm = tmpVm;
				}
				break;
			case PluginType.SqlAgentTool:
				var sql = await _context.PluginSqls.FirstOrDefaultAsync(p => p.PluginId == id);
				if (sql != null)
				{
					string decryptedConn = string.IsNullOrWhiteSpace(sql.ConnectionString) ? string.Empty : _encryption.Decrypt(sql.ConnectionString);
					var tmpVm = (ChPluginSqlVM)sql;
					tmpVm.ConnectionString = decryptedConn;
					vm = tmpVm;
				}
				break;
			case PluginType.GoogleSearch:
				var googleSearch = await _context.PluginGoogleSearches.FirstOrDefaultAsync(p => p.PluginId == id);
				if (googleSearch != null)
				{
					string decryptedApiKey = string.IsNullOrWhiteSpace(googleSearch.ApiKey) ? string.Empty : _encryption.Decrypt(googleSearch.ApiKey);
					var tmpVm = (ChPluginGoogleSearchVM)googleSearch;
					tmpVm.ApiKey = decryptedApiKey;
					vm = tmpVm;
				}
				break;
			case PluginType.BingSearch:
				var bingSearch = await _context.PluginBingSearches.FirstOrDefaultAsync(p => p.PluginId == id);
				if (bingSearch != null)
				{
					string decryptedApiKey = string.IsNullOrWhiteSpace(bingSearch.ApiKey) ? string.Empty : _encryption.Decrypt(bingSearch.ApiKey);
					var tmpVm = (ChPluginBingSearchVM)bingSearch;
					tmpVm.ApiKey = decryptedApiKey;
					vm = tmpVm;
				}
				break;
			case PluginType.VectorDbSearch:
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
			case PluginType.SqlAgentTool:
				if (pluginVm is ChPluginSqlVM sqlVm)
				{
					_context.PluginSqls.Add(new ChPluginSql
					{
						PluginId = plugin.Id,
						Provider = sqlVm.Provider ?? string.Empty,
						ConnectionString = string.IsNullOrWhiteSpace(sqlVm.ConnectionString) ? string.Empty : _encryption.Encrypt(sqlVm.ConnectionString)
					});
				}
				break;
			case PluginType.GoogleSearch:
				if (pluginVm is ChPluginGoogleSearchVM googleSearchVm)
				{
					_context.PluginGoogleSearches.Add(new ChPluginGoogleSearch
					{
						PluginId = plugin.Id,
						ApiKey = string.IsNullOrWhiteSpace(googleSearchVm.ApiKey) ? string.Empty : _encryption.Encrypt(googleSearchVm.ApiKey),
						SearchEngineId = googleSearchVm.SearchEngineId ?? string.Empty
					});
				}
				break;
			case PluginType.BingSearch:
				if (pluginVm is ChPluginBingSearchVM bingSearchVm)
				{
					_context.PluginBingSearches.Add(new ChPluginBingSearch
					{
						PluginId = plugin.Id,
						ApiKey = string.IsNullOrWhiteSpace(bingSearchVm.ApiKey) ? string.Empty : _encryption.Encrypt(bingSearchVm.ApiKey),
					});
				}
				break;
			case PluginType.VectorDbSearch:
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
			case PluginType.SqlAgentTool:
				if (pluginVm is ChPluginSqlVM sqlVm)
				{
					var entity = await _context.PluginSqls.FirstOrDefaultAsync(p => p.PluginId == plugin.Id);
					if (entity != null)
					{
						entity.Provider = sqlVm.Provider ?? string.Empty;
						entity.ConnectionString = string.IsNullOrWhiteSpace(sqlVm.ConnectionString) ? string.Empty : _encryption.Encrypt(sqlVm.ConnectionString);
					}
				}
				break;
			case PluginType.GoogleSearch:
				if (pluginVm is ChPluginGoogleSearchVM googleSearchVm)
				{
					var entity = await _context.PluginGoogleSearches.FirstOrDefaultAsync(p => p.PluginId == plugin.Id);
					if (entity != null)
					{
						entity.ApiKey = string.IsNullOrWhiteSpace(googleSearchVm.ApiKey) ? string.Empty : _encryption.Encrypt(googleSearchVm.ApiKey);
						entity.SearchEngineId = googleSearchVm.SearchEngineId ?? string.Empty;
					}
				}
				break;
			case PluginType.BingSearch:
				if (pluginVm is ChPluginBingSearchVM bingSearchVm)
				{
					var entity = await _context.PluginBingSearches.FirstOrDefaultAsync(p => p.PluginId == plugin.Id);
					entity?.ApiKey = string.IsNullOrWhiteSpace(bingSearchVm.ApiKey) ? string.Empty : _encryption.Encrypt(bingSearchVm.ApiKey);
				}
				break;
			case PluginType.VectorDbSearch:
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
			case PluginType.SqlAgentTool:
				var sql = await _context.PluginSqls.FirstOrDefaultAsync(p => p.PluginId == id);
				if (sql != null) _context.PluginSqls.Remove(sql);
				break;
			case PluginType.GoogleSearch:
				var googleSearch = await _context.PluginGoogleSearches.FirstOrDefaultAsync(p => p.PluginId == id);
				if (googleSearch != null) _context.PluginGoogleSearches.Remove(googleSearch);
				break;
			case PluginType.BingSearch:
				var bingSearch = await _context.PluginBingSearches.FirstOrDefaultAsync(p => p.PluginId == id);
				if (bingSearch != null) _context.PluginBingSearches.Remove(bingSearch);
				break;
			case PluginType.VectorDbSearch:
				break;
		}

		_context.Plugins.Remove(plugin);
		await _context.SaveChangesAsync();
	}

	public async Task<List<PluginBaseDropDownVM>> GetPluginsDropDownAsync()
		=> await _context.Plugins.Where(p => p.IsEnabled).Select(p => (PluginBaseDropDownVM)p).ToListAsync();
}
