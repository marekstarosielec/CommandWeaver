public static class BuiltInCommandParameters
{
    public static List<CommandParameter> List = [
        new CommandParameter
        { 
            Key = "log-level", //TODO: Add test that this exists, since it is handled in CommandWeaver.
            Description = "Controls the detail of logs output by the application.",
            AllowedEnumValues = typeof(LogLevel),
        }
    ];

}
