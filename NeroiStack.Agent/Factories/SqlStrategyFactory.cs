using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Strategies.Plugin.SqlAgentTool;
using System.Reflection;

namespace NeroiStack.Agent.Factories;

public static class SqlStrategyFactory
{
	private static readonly Dictionary<SqlAgentToolType, Type> _strategyTypes = [];

	static SqlStrategyFactory()
	{
		var strategyType = typeof(ISqlStrategy);
		var assembly = Assembly.GetExecutingAssembly();

		var strategies = assembly.GetTypes()
			.Where(t => t.IsClass && !t.IsAbstract && strategyType.IsAssignableFrom(t))
			.ToList();

		foreach (var type in strategies)
		{
			if (Activator.CreateInstance(type) is ISqlStrategy instance)
			{
				_strategyTypes[instance.DbType] = type;
			}
		}
	}

	public static ISqlStrategy GetStrategy(SqlAgentToolType dbType)
	{
		if (_strategyTypes.TryGetValue(dbType, out var strategyType))
		{
			return (ISqlStrategy)Activator.CreateInstance(strategyType)!;
		}

		throw new ArgumentOutOfRangeException(nameof(dbType), dbType, $"No strategy found for database type: {dbType}");
	}

	public static IEnumerable<SqlAgentToolType> GetSupportedDatabaseTypes()
	{
		return _strategyTypes.Keys;
	}
}
