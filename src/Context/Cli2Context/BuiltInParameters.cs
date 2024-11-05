using Models;

namespace Cli2Context;

internal static class BuiltInParameters
{
    public static List<Variable> List = [
        new Variable 
        { 
            Key = "log-level", 
            //Description = "Controls the detail of logs output by the application.",
            //AllowedValues = ["trace", "debug", "information", "warning", "error"]
        }
    ];

}
