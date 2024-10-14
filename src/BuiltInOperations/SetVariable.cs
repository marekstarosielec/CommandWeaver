using Models;
using Models.Interfaces.Context;

namespace BuiltInOperations;

public class SetVariable: Operation
{
    //Add test - every Variable should be public, initialized, have getter and setter.
    public Variable Key { get; set; } = new Variable { Key = "key", Description = "Key of variable to set value to." };
    public Variable Value { get; set; } = new Variable { Key = "value", Description = "Value which should be set." };
    public override Task Run(IContext context, CancellationToken cancellationToken)
    {
    //    var key = Key.GetValueAsString(context);
     //   var currentValue = context.Variables.GetVariableValue2(Key.Value as string);
    //    var value = Value.GetValueAsString(context);
        //var t = context.Variables.GetVariable(key);
        
        return Task.CompletedTask;
    }
}