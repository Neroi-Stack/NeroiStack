using System.ComponentModel;

namespace NeroiStack.Agent.Model;

public class WhereCondition
{
	[Description("The field name to apply the condition on.")]
	public string Field { get; set; } = string.Empty;
	[Description("The operator to use in the condition.")]
	public string Operator { get; set; } = "=";
	[Description("The value to compare the field against.")]
	public object Value { get; set; } = string.Empty;
}

public class JoinCondition
{
	[Description("The type of join (e.g., INNER, LEFT, RIGHT, CROSS).")]
	public string Type { get; set; } = "INNER";
	[Description("The table to join with.")]
	public string Table { get; set; } = string.Empty;
	[Description("The ON clause for the join.")]
	public JoinOnCondition On { get; set; } = new JoinOnCondition();
}

public class JoinOnCondition
{
	[Description("The left side of the ON condition.")]
	public string Left { get; set; } = string.Empty;
	[Description("The operator for the ON condition.")]
	public string Operator { get; set; } = "=";
	[Description("The right side of the ON condition.")]
	public string Right { get; set; } = string.Empty;
}