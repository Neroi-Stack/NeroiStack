namespace NeroiStack.Agent.Enum;

public enum PluginType
{
	OpenApi = 1,
	McpHttp = 2,
	McpStdio = 3,

	// SQL Agent Tool plugin
	SqlAgentTool = 4,

	// RAG related plugins
	BingSearch = 5,
	GoogleSearch = 6,
	VectorDbSearch = 7
}
