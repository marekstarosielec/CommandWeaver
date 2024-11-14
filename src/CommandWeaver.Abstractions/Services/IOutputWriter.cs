public interface IOutputWriter
{
    void Write(string text);

    void WriteRaw(string text);
}