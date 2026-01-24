using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using NeroiStack.Agent.Enum;
using SqlKata.Compilers;
using System.Data.Common;

namespace NeroiStack.Agent.Strategies.Plugin.SqlAgentTool;

public class SqliteStrategy : BaseSqlStrategy
{
	public override SqlAgentToolType DbType => SqlAgentToolType.Sqlite;

	protected override DbConnection CreateConnection(string? connectionString) => new SqliteConnection(connectionString);
	protected override Compiler CreateCompiler() => new SqliteCompiler();

	public override async Task<List<string>> GetSchemasAsync(string connectionString, CancellationToken cancellationToken = default)
	{
		// SQLite does not support multiple schemas in the same way as other RDBMS.
		// Returning an empty list or a list with a single default schema.
		return ["main"];
	}

	public override async Task<List<string>> GetTablesAsync(string connectionString, CancellationToken cancellationToken = default)
	{
		using var connection = new SqliteConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return [.. await connection.QueryAsync<string>("SELECT name FROM sqlite_master WHERE type='table';")];
	}

	public override async Task<List<string>> GetColumnsAsync(string connectionString, string tableName, CancellationToken cancellationToken = default)
	{
		using var connection = new SqliteConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		var result = await connection.QueryAsync($"PRAGMA table_info([{tableName}])");
		return [.. result.Select(r => (string)r.name)];
	}
}
