using Dapper;
using NeroiStack.Agent.Enum;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using System.Data.Common;

namespace NeroiStack.Agent.Strategies.Plugin.SqlAgentTool;

public class MySqlStrategy : BaseSqlStrategy
{
	public override SqlAgentToolType DbType => SqlAgentToolType.Mysql;

	protected override DbConnection CreateConnection(string? connectionString) => new MySqlConnection(connectionString);
	protected override Compiler CreateCompiler() => new MySqlCompiler();

	public override async Task<List<string>> GetSchemasAsync(string connectionString, CancellationToken cancellationToken = default)
	{
		using var connection = new MySqlConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return [.. await connection.QueryAsync<string>("SHOW DATABASES;")];
	}

	public override async Task<List<string>> GetTablesAsync(string connectionString, CancellationToken cancellationToken = default)
	{
		using var connection = new MySqlConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return [.. await connection.QueryAsync<string>("SHOW TABLES;")];
	}

	public override async Task<List<string>> GetColumnsAsync(string connectionString, string tableName, CancellationToken cancellationToken = default)
	{
		using var connection = new MySqlConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return (await connection.QueryAsync<string>("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName", new { tableName })).ToList();
	}
}
