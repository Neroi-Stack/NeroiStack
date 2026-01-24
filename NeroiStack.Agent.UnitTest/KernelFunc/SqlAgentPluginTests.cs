using NeroiStack.Agent.KernelFunc;

namespace NeroiStack.Agent.UnitTest.KernelFunc;

public class SqlAgentPluginTests
{
	[Fact]
	public async Task Methods_WithInvalidProvider_ReturnInvalidProviderMessage()
	{
		var plugin = new SqlAgentPlugin("fake-conn", "invalidProvider");

		var tables = await plugin.GetTables();
		Assert.Contains("Invalid provider", tables);

		var schemas = await plugin.GetSchemas();
		Assert.Contains("Invalid provider", schemas);

		var columns = await plugin.GetColumns("any");
		Assert.Contains("Invalid provider", columns);

		var exec = await plugin.ExecuteQuerySafe("any", new[] { "*" });
		Assert.Contains("Invalid provider", exec);
	}
}