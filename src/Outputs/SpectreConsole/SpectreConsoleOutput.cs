﻿using Spectre.Console;
using Spectre.Console.Json;

namespace SpectreConsole;

public class SpectreConsoleOutput : IOutputWriter
{
    public void WriteRaw(string textValue)
    {
        AnsiConsole.Console.Write(textValue);
    }

    public void WriteMarkup(string textValue)
    {
        AnsiConsole.Write(new CustomRenderable(MarkupConverter.ConvertToSegments(textValue)));
    }

    public void WriteJson(string json)
    {
        try
        {
            AnsiConsole.Write(new JsonText(json));
        }
        catch (Exception e)
        {
            AnsiConsole.Console.Write(json);
        }
    }
    
}