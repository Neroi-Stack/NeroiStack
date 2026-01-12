using NeroiStack.Agent.Data.Entities;

namespace NeroiStack.Agent.Model;

public class AgentDropdownVM
{
	public int Id { get; set; }
	public string? Name { get; set; }

	public static explicit operator AgentDropdownVM(ChAgent agent) => new()
	{
		Id = agent.Id,
		Name = agent.Name,
	};
}