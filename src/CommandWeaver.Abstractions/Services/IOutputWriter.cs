public interface IOutputWriter
{
    void WriteText(string textValue);
    void WriteObject(DynamicValueObject objectValue);
        
    void WriteRaw(string text);
}