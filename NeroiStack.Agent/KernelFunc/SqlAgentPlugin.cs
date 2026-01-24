using Microsoft.SemanticKernel;
using System.ComponentModel;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Factories;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.KernelFunc;

public class SqlAgentPlugin(string connectionString, string provider)
{
	private readonly string _connectionString = connectionString;
	private readonly string _provider = provider;

	[KernelFunction, Description("Execute a query (supports join, where, order by, limit).")]
	public async Task<string> ExecuteQuerySafe(
		[Description("The table name")]
		string tableName,
		[Description("List of columns to select")]
		IEnumerable<string> selectColumns,
		[Description("Dictionary of where columns and their values")]
		IEnumerable<WhereCondition>? whereColumnsAndValues = null,
		[Description("List of columns to order by")]
		IEnumerable<string>? orderByColumns = null,
		[Description("Limit the number of results returned")]
		int limit = 0,
		[Description("List of joins. Each join is a dictionary with keys: 'Table', 'On', and optional 'Type' (default 'INNER').")]
		IEnumerable<JoinCondition>? joins = null)
	{
		if (!System.Enum.TryParse<SqlAgentToolType>(_provider, true, out var dbType))
		{
			return $"Invalid provider: {_provider}";
		}
		var strategy = SqlStrategyFactory.GetStrategy(dbType);
		var result = await strategy.ExecuteQueryAsync(
			_connectionString,
			tableName,
			selectColumns,
			whereColumnsAndValues,
			orderByColumns,
			limit,
			joins
		);
		return result;
	}

	[KernelFunction, Description("Get column names of a table.")]
	public async Task<string> GetColumns(string tableName)
	{
		try
		{
			if (!System.Enum.TryParse<SqlAgentToolType>(_provider, true, out var dbType))
			{
				return $"Invalid provider: {_provider}";
			}
			var strategy = SqlStrategyFactory.GetStrategy(dbType);
			var columns = await strategy.GetColumnsAsync(_connectionString, tableName);
			return string.Join(", ", columns);
		}
		catch (Exception ex)
		{
			return $"Error getting columns: {ex.Message}";
		}
	}

	[KernelFunction, Description("Get list of schemas in the database.")]
	public async Task<string> GetSchemas()
	{
		try
		{
			if (!System.Enum.TryParse<SqlAgentToolType>(_provider, true, out var dbType))
			{
				return $"Invalid provider: {_provider}";
			}
			var strategy = SqlStrategyFactory.GetStrategy(dbType);
			var schemas = await strategy.GetSchemasAsync(_connectionString);
			return string.Join(", ", schemas);
		}
		catch (Exception ex)
		{
			return $"Error getting schemas: {ex.Message}";
		}
	}

	[KernelFunction, Description("Get list of tables in the database.")]
	public async Task<string> GetTables()
	{
		try
		{
			if (!System.Enum.TryParse<SqlAgentToolType>(_provider, true, out var dbType))
			{
				return $"Invalid provider: {_provider}";
			}
			var strategy = SqlStrategyFactory.GetStrategy(dbType);
			var tables = await strategy.GetTablesAsync(_connectionString);
			return string.Join(", ", tables);
		}
		catch (Exception ex)
		{
			return $"Error getting tables: {ex.Message}";
		}
	}

	[KernelFunction, Description("Get schema of a table.")]
	public async Task<string> GetTableSchema([Description("The table name")] string tableName)
	{
		try
		{
			if (!System.Enum.TryParse<SqlAgentToolType>(_provider, true, out var dbType))
			{
				return $"Invalid provider: {_provider}";
			}
			var strategy = SqlStrategyFactory.GetStrategy(dbType);
			var columns = await strategy.GetColumnsAsync(_connectionString, tableName);
			return string.Join(", ", columns);
		}
		catch (Exception ex)
		{
			return $"Error getting schema: {ex.Message}";
		}
	}
}
