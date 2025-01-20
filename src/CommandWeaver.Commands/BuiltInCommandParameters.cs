public static class BuiltInCommandParameters
{
    public static List<CommandParameter> List = [
        new CommandParameter
        { 
            Key = "log-level", 
            Description = "Controls the detail of logs output by the application.",
            Validation = new Validation { AllowedEnumValues = typeof(LogLevel) },
        },
        new CommandParameter
        { 
            Key = "session",
            Description = "Allows to select session in which command is executed."
        }
    ];

}
