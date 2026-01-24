using Dapper;
using NeroiStack.Agent.Enum;

namespace NeroiStack.Agent.Strategies.Plugin.SqlAgentTool;

public interface ISqlStrategy
{
	SqlAgentToolType DbType { get; }
	Task<string> ExecuteQueryAsync(string connectionString, string query, DynamicParameters parameters, CancellationToken cancellationToken = default);
	Task<int> ExecuteNonQueryAsync(string connectionString, string query, DynamicParameters parameters, CancellationToken cancellationToken = default);
	Task<List<string>> GetTablesAsync(string connectionString, CancellationToken cancellationToken = default);
	Task<List<string>> GetColumnsAsync(string connectionString, string tableName, CancellationToken cancellationToken = default);
}
