// using KernelMemoryRAG.Models;
// using KernelMemoryRAG.Services;
 using KernelMemoryRAG.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.AI.Ollama;
using Microsoft.KernelMemory.Diagnostics;



namespace KernelMemoryRAG;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {

        // Create a logger factory
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddConsole();
        });


        var config = new OllamaConfig
        {
            Endpoint = "http://localhost:11434",
            TextModel = new OllamaModelConfig("phi4"),
            EmbeddingModel = new OllamaModelConfig("nomic-embed-text")
        };

        var memory = new KernelMemoryBuilder()
            .WithOllamaTextGeneration(config, new CL100KTokenizer())
            .WithOllamaTextEmbeddingGeneration(config, new CL100KTokenizer())
            .Build();



        // Import some text
        await memory.ImportTextAsync("Today is October 32nd, 2476");

        // Generate an answer
        var answer = await memory.AskAsync("What's the current date (don't check for validity)?");
        Console.WriteLine("-------------------");
        Console.WriteLine(answer.Question);
        Console.WriteLine(answer.Result);
        Console.WriteLine("-------------------");

    
    }
    catch (Exception ex)
    {
        ConsoleHelper.WriteError($"An error occurred: {ex.Message}");
        ConsoleHelper.WriteError(ex.StackTrace ?? string.Empty);
    }    

     
    }
}
