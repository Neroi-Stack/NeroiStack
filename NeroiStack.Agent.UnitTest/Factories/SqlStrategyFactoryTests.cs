using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Factories;

namespace NeroiStack.Agent.UnitTest.Factories;

public class SqlStrategyFactoryTests
{
	[Fact]
	public void GetSupportedDatabaseTypes_ShouldContainKnownTypes()
	{
		var supported = SqlStrategyFactory.GetSupportedDatabaseTypes().ToList();

		Assert.Contains(SqlAgentToolType.Sqlite, supported);
		Assert.Contains(SqlAgentToolType.Postgres, supported);
		Assert.Contains(SqlAgentToolType.Mysql, supported);
	}

	[Fact]
	public void GetStrategy_InvalidType_ThrowsArgumentOutOfRangeException()
	{
		var invalid = (SqlAgentToolType)999;
		Assert.Throws<ArgumentOutOfRangeException>(() => SqlStrategyFactory.GetStrategy(invalid));
	}
}