using System.Data.Common;
using System.Text.Json;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using NeroiStack.Agent.Model;
using NeroiStack.Agent.Enum;

namespace NeroiStack.Agent.Strategies.Plugin.SqlAgentTool;

public abstract class BaseSqlStrategy : ISqlStrategy
{
	public abstract SqlAgentToolType DbType { get; }

	protected abstract DbConnection CreateConnection(string? connectionString);
	protected abstract Compiler CreateCompiler();

	public async Task<string> ExecuteQueryAsync(
		string? connectionString = null,
		string? tableName = null,
		IEnumerable<string>? selectColumns = null,
		IEnumerable<WhereCondition>? whereConditions = null,
		IEnumerable<string>? orderByColumns = null,
		int? limit = null,
		IEnumerable<JoinCondition>? joins = null,
		CancellationToken cancellationToken = default)
	{
		using var connection = CreateConnection(connectionString);
		await connection.OpenAsync(cancellationToken);

		var compiler = CreateCompiler();
		var db = new QueryFactory(connection, compiler);
		var query = new Query(tableName);
		if (selectColumns != null && selectColumns.Any())
			query = query.Select([.. selectColumns]);
		else
			query = query.Select("*");
		if (joins != null)
		{
			foreach (var join in joins)
			{
				if (!string.IsNullOrEmpty(join.Table) && join.On != null)
				{
					var joinType = join.Type?.ToLowerInvariant() ?? "inner";
					var left = join.On.Left;
					var op = join.On.Operator ?? "=";
					var right = join.On.Right;

					query = joinType switch
					{
						"left" => query.LeftJoin(join.Table, left, op, right),
						"right" => query.RightJoin(join.Table, left, op, right),
						"cross" => query.CrossJoin(join.Table),
						_ => query.Join(join.Table, left, op, right),
					};
				}
			}
		}
		if (whereConditions != null)
		{
			foreach (var cond in whereConditions)
			{
				query = query.Where(cond.Field, cond.Operator, cond.Value);
			}
		}
		if (orderByColumns != null && orderByColumns.Any())
		{
			foreach (var col in orderByColumns)
				query = query.OrderBy(col);
		}
		if (limit != null && limit > 0)
			query = query.Limit(limit.Value);

		var result = await db.GetAsync(query, cancellationToken: cancellationToken);

		var resultList = result.Select(r => (IDictionary<string, object>)r).ToList();
		return JsonSerializer.Serialize(resultList);
	}

	public abstract Task<List<string>> GetSchemasAsync(string connectionString, CancellationToken cancellationToken = default);
	public abstract Task<List<string>> GetTablesAsync(string connectionString, CancellationToken cancellationToken = default);
	public abstract Task<List<string>> GetColumnsAsync(string connectionString, string tableName, CancellationToken cancellationToken = default);
}
