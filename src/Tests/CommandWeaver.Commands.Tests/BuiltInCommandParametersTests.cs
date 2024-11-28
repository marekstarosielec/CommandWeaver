public class BuiltInCommandParametersTests
{
    [Fact]
    public void BuiltInCommandParameters_ShouldContainLogLevelParameter()
    {
        // Act
        var logLevelParameter = BuiltInCommandParameters.List.FirstOrDefault(p => p.Key == "log-level");

        // Assert
        Assert.NotNull(logLevelParameter);
        Assert.Equal("log-level", logLevelParameter.Key);
        Assert.Equal("Controls the detail of logs output by the application.", logLevelParameter.Description);
        Assert.NotNull(logLevelParameter.AllowedEnumValues);
        Assert.Equal(typeof(LogLevel), logLevelParameter.AllowedEnumValues);
    }

    [Fact]
    public void BuiltInCommandParameters_ShouldAllHaveUniqueKeys()
    {
        // Act
        var keys = BuiltInCommandParameters.List.Select(p => p.Key).ToList();

        // Assert
        Assert.Equal(keys.Count, keys.Distinct().Count());
    }
}
