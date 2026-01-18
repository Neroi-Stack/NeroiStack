namespace NeroiStack.Agent.Enum;

public enum AgentOrchestrationType
{
	/// <summary>
	/// Single agent handling the request.
	/// </summary>
	Single = 1,

	/// <summary>
	/// Multiple agents generate different solutions to a problem, and their responses are collected for further analysis or selection.
	/// </summary>
	Concurrent = 2,

	/// <summary>
	/// A document passes through a summarization agent, then a translation agent, and finally a quality assurance agent, each building on the previous output.
	/// </summary>
	Sequential = 3,

	/// <summary>
	/// A customer support agent handles a general inquiry, then hands off to a technical expert agent for troubleshooting, or to a billing agent if needed.
	/// </summary>
	Handoff = 4,

	/// <summary>
	/// Agents representing different departments discuss a business proposal, with a manager agent moderating the conversation and involving a human when needed.
	/// </summary>
	GroupChat = 5,

	/// <summary>
	/// A user requests a comprehensive report... The Magentic manager first assigns a research agent... then delegates analysis... coordinates multiple rounds...
	/// </summary>
	Magentic = 6
}
