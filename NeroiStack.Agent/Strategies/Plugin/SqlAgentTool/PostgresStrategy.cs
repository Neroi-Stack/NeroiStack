using Dapper;
using NeroiStack.Agent.Enum;
using Npgsql;
using System.Text.Json;

namespace NeroiStack.Agent.Strategies.Plugin.SqlAgentTool;

public class PostgresStrategy : ISqlStrategy
{
	public SqlAgentToolType DbType => SqlAgentToolType.Postgres;

	public async Task<string> ExecuteQueryAsync(string connectionString, string query, DynamicParameters parameters, CancellationToken cancellationToken = default)
	{
		using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		var result = await connection.QueryAsync(query, parameters);
		return JsonSerializer.Serialize(result);
	}

	public async Task<int> ExecuteNonQueryAsync(string connectionString, string query, DynamicParameters parameters, CancellationToken cancellationToken = default)
	{
		using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return await connection.ExecuteAsync(query, parameters);
	}

	public async Task<List<string>> GetTablesAsync(string connectionString, CancellationToken cancellationToken = default)
	{
		using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return (await connection.QueryAsync<string>("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';")).ToList();
	}

	public async Task<List<string>> GetColumnsAsync(string connectionString, string tableName, CancellationToken cancellationToken = default)
	{
		using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return (await connection.QueryAsync<string>("SELECT column_name FROM information_schema.columns WHERE table_name = @tableName;", new { tableName })).ToList();
	}
}
