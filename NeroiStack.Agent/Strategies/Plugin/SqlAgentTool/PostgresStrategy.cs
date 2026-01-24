using Dapper;
using NeroiStack.Agent.Enum;
using Npgsql;
using SqlKata.Compilers;
using System.Data.Common;

namespace NeroiStack.Agent.Strategies.Plugin.SqlAgentTool;

public class PostgresStrategy : BaseSqlStrategy
{
	public override SqlAgentToolType DbType => SqlAgentToolType.Postgres;

	protected override DbConnection CreateConnection(string? connectionString) => new NpgsqlConnection(connectionString);
	protected override Compiler CreateCompiler() => new PostgresCompiler();

	public override async Task<List<string>> GetSchemasAsync(string connectionString, CancellationToken cancellationToken = default)
	{
		using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return [.. await connection.QueryAsync<string>("SELECT schema_name FROM information_schema.schemata;")];
	}

	public override async Task<List<string>> GetTablesAsync(string connectionString, CancellationToken cancellationToken = default)
	{
		using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return [.. await connection.QueryAsync<string>("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';")];
	}

	public override async Task<List<string>> GetColumnsAsync(string connectionString, string tableName, CancellationToken cancellationToken = default)
	{
		using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync(cancellationToken);
		return [.. await connection.QueryAsync<string>("SELECT column_name FROM information_schema.columns WHERE table_name = @tableName;", new { tableName })];
	}
}
