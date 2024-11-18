public interface IOutputWriter
{
    void WriteRaw(string textValue);
    
    void WriteMarkup(string textValue);
    
    void WriteJson(string json);

}