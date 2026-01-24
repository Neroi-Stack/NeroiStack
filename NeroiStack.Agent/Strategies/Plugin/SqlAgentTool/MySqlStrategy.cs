using Dapper;
using NeroiStack.Agent.Enum;
using MySql.Data.MySqlClient;
using System.Text.Json;

namespace NeroiStack.Agent.Strategies.Plugin.SqlAgentTool;

public class MySqlStrategy : ISqlStrategy
{
	public SqlAgentToolType DbType => SqlAgentToolType.Mysql;

	public async Task<string> ExecuteQueryAsync(string connectionString, string query, DynamicParameters parameters, CancellationToken cancellationToken = default)
	{
		using var connection = new MySqlConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		var result = await connection.QueryAsync(query, parameters);
		return JsonSerializer.Serialize(result);
	}

	public async Task<int> ExecuteNonQueryAsync(string connectionString, string query, DynamicParameters parameters, CancellationToken cancellationToken = default)
	{
		using var connection = new MySqlConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return await connection.ExecuteAsync(query, parameters);
	}

	public async Task<List<string>> GetTablesAsync(string connectionString, CancellationToken cancellationToken = default)
	{
		using var connection = new MySqlConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return [.. await connection.QueryAsync<string>("SHOW TABLES;")];
	}

	public async Task<List<string>> GetColumnsAsync(string connectionString, string tableName, CancellationToken cancellationToken = default)
	{
		using var connection = new MySqlConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return (await connection.QueryAsync<string>("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName", new { tableName })).ToList();
	}
}
