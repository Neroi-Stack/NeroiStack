using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;

namespace NeroiStack.Agent.Model;

public class PluginBaseVM
{
	public int Id { get; set; }
	public PluginType Type { get; set; }
	public string? Name { get; set; }
	public string? Description { get; set; }
	public bool IsEnabled { get; set; }
	public static explicit operator PluginBaseVM(ChPlugin plugin)
	{
		return new PluginBaseVM
		{
			Id = plugin.Id,
			Name = plugin.Name,
			Description = plugin.Description,
			Type = plugin.Type,
			IsEnabled = plugin.IsEnabled,
		};
	}
}

public class PluginMcpHttpVM : PluginBaseVM
{
	public int PluginMcpHttpId { get; set; }
	public int PluginId { get; set; }
	public string? Endpoint { get; set; }
	public string? ApiKey { get; set; }
	public static explicit operator PluginMcpHttpVM(ChPluginMcpHttp plugin)
	{
		return new PluginMcpHttpVM
		{
			PluginMcpHttpId = plugin.Id,
			PluginId = plugin.PluginId,
			Endpoint = plugin.Endpoint
		};
	}
}

public class ChPluginMcpHttpStreamableVM : PluginBaseVM
{
	public int ChPluginMcpHttpStreamableId { get; set; }
	public int PluginId { get; set; }
	public string? Endpoint { get; set; }
	public string? ApiKey { get; set; }
	public static explicit operator ChPluginMcpHttpStreamableVM(ChPluginMcpHttpStreamable plugin)
	{
		return new ChPluginMcpHttpStreamableVM
		{
			ChPluginMcpHttpStreamableId = plugin.Id,
			PluginId = plugin.PluginId,
			Endpoint = plugin.Endpoint,
		};
	}
}

public class ChPluginMcpStdioVM : PluginBaseVM
{
	public int ChPluginMcpStdioId { get; set; }
	public int PluginId { get; set; }
	public string? Command { get; set; }
	public List<string>? Arguments { get; set; }
	public static explicit operator ChPluginMcpStdioVM(ChPluginMcpStdio plugin)
	{
		return new ChPluginMcpStdioVM
		{
			ChPluginMcpStdioId = plugin.Id,
			PluginId = plugin.PluginId,
			Command = plugin.Command,
			Arguments = plugin.Arguments
		};
	}
}

public class ChPluginOpenApiVM : PluginBaseVM
{
	public int ChPluginOpenApiId { get; set; }
	public int PluginId { get; set; }
	public int AuthType { get; set; }
	public string? ApiKey { get; set; }
	public string? BearerToken { get; set; }
	public string? Uri { get; set; }
	public string? FilePath { get; set; }
	public static explicit operator ChPluginOpenApiVM(ChPluginOpenApi plugin)
	{
		return new ChPluginOpenApiVM
		{
			ChPluginOpenApiId = plugin.Id,
			PluginId = plugin.PluginId,
			AuthType = plugin.AuthType,
			Uri = plugin.Uri,
			FilePath = plugin.FilePath
		};
	}
}


public class ChPluginSqlVM : PluginBaseVM
{
	public int PluginSqlId { get; set; }
	public int PluginId { get; set; }
	public string? Provider { get; set; }
	public string? ConnectionString { get; set; }
	public static List<string> AvailableProviders { get; } =
		[.. System.Enum.GetValues<SqlAgentToolType>().Select(e => e.ToString())];

	public static explicit operator ChPluginSqlVM(ChPluginSql plugin)
	{
		return new ChPluginSqlVM
		{
			PluginSqlId = plugin.Id,
			PluginId = plugin.Id,
			Provider = plugin.Provider,
			ConnectionString = plugin.ConnectionString
		};
	}
}

public class PluginBaseDropDownVM
{
	public int Id { get; set; }
	public string? Name { get; set; }

	public static explicit operator PluginBaseDropDownVM(ChPlugin v)
	{
		return new PluginBaseDropDownVM
		{
			Id = v.Id,
			Name = v.Name
		};
	}
}