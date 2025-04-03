using System;

namespace KernelMemoryRAG.Utilities;

public static class ConsoleHelper
{
    public static void WriteLineInColor(string message, ConsoleColor color)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
    }

    public static void WriteSuccess(string message)
    {
        WriteLineInColor(message, ConsoleColor.Green);
    }

    public static void WriteError(string message)
    {
        WriteLineInColor(message, ConsoleColor.Red);
    }

    public static void WriteWarning(string message)
    {
        WriteLineInColor(message, ConsoleColor.Yellow);
    }

    public static void WriteInfo(string message)
    {
        WriteLineInColor(message, ConsoleColor.Cyan);
    }
}
