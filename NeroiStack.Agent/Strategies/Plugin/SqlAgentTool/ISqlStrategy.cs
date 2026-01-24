using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies.Plugin.SqlAgentTool;

public interface ISqlStrategy
{
	SqlAgentToolType DbType { get; }
	Task<string> ExecuteQueryAsync(
		string? connectionString = null,
		string? tableName = null,
		IEnumerable<string>? selectColumns = null,
		IEnumerable<WhereCondition>? whereConditions = null,
		IEnumerable<string>? orderByColumns = null,
		int? limit = null,
		IEnumerable<JoinCondition>? joins = null,
		CancellationToken cancellationToken = default
	);
	Task<List<string>> GetSchemasAsync(string connectionString, CancellationToken cancellationToken = default);
	Task<List<string>> GetTablesAsync(string connectionString, CancellationToken cancellationToken = default);
	Task<List<string>> GetColumnsAsync(string connectionString, string tableName, CancellationToken cancellationToken = default);
}
