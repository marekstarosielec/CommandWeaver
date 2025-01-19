using System.Collections.Immutable;

public record ListGroup(IVariableService variableService) : Operation
{
    public override string Name => nameof(ListGroup);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } =
        new Dictionary<string, OperationParameter>
        {
            {"list", new OperationParameter { Description = "List to group", Required = true, List = true}},
            {"property", new OperationParameter { Description = "Property to group by", Required = true, AllowedType = "text"}},
            {"saveTo", new OperationParameter { Description = "Name of variable where grouped values will be saved", Required = true, AllowedType = "text"}}
        }.ToImmutableDictionary();

    public override Task Run(CancellationToken cancellationToken)
    {
        var groups = new List<DynamicValue>();
        //Build groups
        foreach (var listElement in Parameters["list"].Value.ListValue!)
        {
            variableService.WriteVariableValue(VariableScope.Command, "current-grouping-element", listElement);
            var group = variableService.ReadVariableValue(new DynamicValue($"current-grouping-element.{Parameters["property"].Value.TextValue!}"), true);
            if (groups.Any(g => g == group))
                continue;
            groups.Add(group);
        }
        variableService.WriteVariableValue(VariableScope.Command, "current-grouping-element", new DynamicValue());
        
        //Order by TextValue in each group and place null at the end (default behaviour)
        groups = groups.OrderBy(g => g.IsNull()).ThenBy(g => g.TextValue).ToList();
        variableService.WriteVariableValue(VariableScope.Command, Parameters["saveTo"].Value.TextValue!, new DynamicValue(groups));
        return Task.CompletedTask;
    }
}