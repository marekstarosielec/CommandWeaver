public class InputService(IInputReader inputReader, IVariableService variableService) : IInputService
{
    public DynamicValue Prompt(InputInformation inputInformation)
    {
        return new DynamicValue(
            inputReader.PromptText(
                inputInformation.Message, 
                inputInformation.Required,
                inputInformation.PromptStyle,
                GetSecretChar(inputInformation)));
    }

    private char? GetSecretChar(InputInformation inputInformation)
    {
        if (!inputInformation.IsSecret) return null;
         
        var secretCharVariable = variableService.ReadVariableValue(new DynamicValue("{{secret-char}}"));
        return !string.IsNullOrEmpty(secretCharVariable.TextValue) ? secretCharVariable.TextValue[0] : '*';
    }
}