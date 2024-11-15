public interface IOutputWriter
{
    void WriteText(string textValue);
    void WriteObject(string json);
        
    void WriteRaw(string text);
}