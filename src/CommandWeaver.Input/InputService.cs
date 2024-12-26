public class InputService(IInputReader inputReader) : IInputService
{
    public DynamicValue Prompt(InputInformation inputInformation)
    {
        return new DynamicValue(inputReader.Prompt());
    }
}