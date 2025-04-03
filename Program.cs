using KernelMemoryRAG.Models;
using KernelMemoryRAG.Services;
using KernelMemoryRAG.Utilities;
using Microsoft.Extensions.Configuration;
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


            var ollamaConfig = new OllamaConfig
            {
                Endpoint = "http://localhost:11434",
                TextModel = new OllamaModelConfig("phi4"),
                EmbeddingModel = new OllamaModelConfig("nomic-embed-text")
            };

            var memory = new KernelMemoryBuilder()
                .WithOllamaTextGeneration(ollamaConfig, new CL100KTokenizer())
                .WithOllamaTextEmbeddingGeneration(ollamaConfig, new CL100KTokenizer())
                .Build();


            string directoryPath = @"C:\SynologyDrive\Drive\Documents\AI Training\";//args[0];

            // Validate directory path
            if (!Directory.Exists(directoryPath))
            {
                ConsoleHelper.WriteError($"Directory not found: {directoryPath}");
                return;
            }

            // Load configuration
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var appSettings = new AppSettings();
            config.GetSection("AppSettings").Bind(appSettings);


            // Process documents
            ConsoleHelper.WriteInfo($"Processing documents from: {directoryPath}");
            var supportedFiles = DocumentProcessor.GetSupportedFiles(directoryPath);

            if (supportedFiles.Count == 0)
            {
                ConsoleHelper.WriteWarning("No supported documents found in the specified directory.");
                return;
            }

            // Display document information
            Console.WriteLine("\nDocuments to process:");
            ConsoleHelper.WriteInfo("Starting document import...");
            foreach (var file in supportedFiles)
            {
                try
                {
                    Console.WriteLine($"- {Path.GetFileName(file)} ({DocumentProcessor.GetFileType(file)}, {DocumentProcessor.GetFileSizeFormatted(file)})");
                    ConsoleHelper.WriteInfo($"Importing: {Path.GetFileName(file)}");
                    await memory.ImportDocumentAsync(file, documentId: Path.GetFileNameWithoutExtension(file.Replace(" ", "_")));
                    ConsoleHelper.WriteSuccess($"Successfully imported: {Path.GetFileName(file)}");
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteError($"Error importing {Path.GetFileName(file)}: {ex.Message}");
                }

            }
            ConsoleHelper.WriteSuccess("Document import completed.");
            Console.WriteLine();

            var answer = await memory.AskAsync("What games did I most often mention as 'Itching to Play'?");
            Console.WriteLine("-------------------");
            Console.WriteLine(answer.Question);
            Console.WriteLine(answer.Result);
            Console.WriteLine("-------------------");
            Console.WriteLine("Press enter to exit. Otherwise, type a new question.");


            string? nextQuestion;

            do
            {
                Console.Write("> ");
                nextQuestion = Console.ReadLine();

                if(string.IsNullOrEmpty(nextQuestion))
                    break;

                answer = await memory.AskAsync(nextQuestion);
                Console.WriteLine("-------------------");
                Console.WriteLine(answer.Question);
                Console.WriteLine(answer.Result);
                Console.WriteLine("-------------------");
            }
            while(!string.IsNullOrEmpty(nextQuestion));
            

        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"An error occurred: {ex.Message}");
            ConsoleHelper.WriteError(ex.StackTrace ?? string.Empty);
        }


    }
}
