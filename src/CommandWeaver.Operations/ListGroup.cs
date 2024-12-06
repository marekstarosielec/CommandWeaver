using System.Collections.Immutable;

public record ListGroup(IVariableService variableService) : Operation
{
    public override string Name => nameof(ListGroup);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } =
        new Dictionary<string, OperationParameter>
        {
            {"list", new OperationParameter { Description = "List to group", RequiredList = true}},
            {"property", new OperationParameter { Description = "Property to group by", RequiredText = true}},
            {"nullValueReplacement", new OperationParameter { Description = "Text to use instead of empty value"}},
            {"saveTo", new OperationParameter { Description = "Name of variable where grouped values will be saved", RequiredText = true}},
            {"sort", new OperationParameter { Description = "Sorting operation"}}
        }.ToImmutableDictionary();

    public override Task Run(CancellationToken cancellationToken)
    {
        var groups = new List<DynamicValue>();
        foreach (var listElement in Parameters["list"].Value.ListValue!)
        {
            variableService.WriteVariableValue(VariableScope.Command, "current-grouping-element", listElement);
            var group = variableService.ReadVariableValue(new DynamicValue($"current-grouping-element.{Parameters["property"].Value.TextValue!}"), true);
            // if (group.IsNull())
            //     group = Parameters["nullValueReplacement"].Value;
            if (groups.All(g => g != group))
                groups.Add(group);
        }
        variableService.WriteVariableValue(VariableScope.Command, "current-grouping-element", new DynamicValue());
        throw new NotImplementedException();
    }
}