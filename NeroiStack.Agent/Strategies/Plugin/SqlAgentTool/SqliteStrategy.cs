using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using NeroiStack.Agent.Enum;
using System.Text.Json;

namespace NeroiStack.Agent.Strategies.Plugin.SqlAgentTool;

public class SqliteStrategy : ISqlStrategy
{
	public SqlAgentToolType DbType => SqlAgentToolType.Sqlite;

	public async Task<string> ExecuteQueryAsync(string connectionString, string query, DynamicParameters parameters, CancellationToken cancellationToken = default)
	{
		using var connection = new SqliteConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		var result = await connection.QueryAsync(query, parameters);
		return JsonSerializer.Serialize(result);
	}

	public async Task<int> ExecuteNonQueryAsync(string connectionString, string query, DynamicParameters parameters, CancellationToken cancellationToken = default)
	{
		using var connection = new SqliteConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return await connection.ExecuteAsync(query, parameters);
	}

	public async Task<List<string>> GetTablesAsync(string connectionString, CancellationToken cancellationToken = default)
	{
		using var connection = new SqliteConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return [.. await connection.QueryAsync<string>("SELECT name FROM sqlite_master WHERE type='table';")];
	}

	public async Task<List<string>> GetColumnsAsync(string connectionString, string tableName, CancellationToken cancellationToken = default)
	{
		using var connection = new SqliteConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		var result = await connection.QueryAsync($"PRAGMA table_info({tableName})");
		return [.. result.Select(r => (string)r.name)];
	}
}
