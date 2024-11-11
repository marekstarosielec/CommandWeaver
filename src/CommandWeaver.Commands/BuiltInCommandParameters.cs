internal static class BuiltInCommandParameters
{
    public static List<CommandParameter> List = [
        new CommandParameter
        { 
            Key = "log-level", 
            Description = "Controls the detail of logs output by the application.",
            AllowedValues = ["trace", "debug", "information", "warning", "error"]
        }
    ];

}
