using Microsoft.SemanticKernel;
using System.ComponentModel;
using NeroiStack.Agent.Enum;
using System.Text;
using Dapper;
using NeroiStack.Agent.Factories;

namespace NeroiStack.Agent.KernelFunc;

public class SqlAgentPlugin(string connectionString, string provider)
{
	private readonly string _connectionString = connectionString;
	private readonly string _provider = provider;

	[KernelFunction, Description("Execute a query (supports join, where, order by, limit).")]
	public async Task<string> ExecuteQuerySafe(
		string tableName,
		IEnumerable<string> selectColumns,
		Dictionary<string, object>? whereColumnsAndValues = null,
		IEnumerable<string>? orderByColumns = null,
		int limit = 0,
		[Description("List of joins. Each join is a dictionary with keys: 'Table', 'On', and optional 'Type' (default 'INNER').")]
		IEnumerable<Dictionary<string, string>>? joins = null)
	{
		if (!System.Enum.TryParse<SqlAgentToolType>(_provider, true, out var dbType))
		{
			return $"Invalid provider: {_provider}";
		}
		var strategy = SqlStrategyFactory.GetStrategy(dbType);
		var sql = new StringBuilder();
		sql.Append("SELECT ");
		sql.Append(selectColumns.Any() ? string.Join(", ", selectColumns) : "*");
		sql.Append(" FROM ");
		sql.Append(tableName);

		if (joins != null)
		{
			foreach (var join in joins)
			{
				if (join.TryGetValue("Table", out var joinTable) && join.TryGetValue("On", out var joinOn))
				{
					var joinType = join.ContainsKey("Type") ? join["Type"] : "INNER";
					sql.Append($" {joinType} JOIN {joinTable} ON {joinOn}");
				}
			}
		}

		var parameters = new DynamicParameters();

		if (whereColumnsAndValues != null && whereColumnsAndValues.Count != 0)
		{
			var whereClause = new List<string>();
			foreach (var kv in whereColumnsAndValues)
			{
				whereClause.Add($"{kv.Key} = @{kv.Key}");
				parameters.Add($"@{kv.Key}", kv.Value);
			}
			sql.Append(" WHERE " + string.Join(" AND ", whereClause));
		}
		if (orderByColumns != null && orderByColumns.Any())
		{
			sql.Append(" ORDER BY " + string.Join(", ", orderByColumns));
		}
		if (limit > 0) sql.Append(" LIMIT @limit");
		if (limit > 0) parameters.Add("@limit", limit);

		var result = await strategy.ExecuteQueryAsync(_connectionString, sql.ToString(), parameters);
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
	public async Task<string> GetTableSchema(
		[Description("The table name")] string tableName)
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
