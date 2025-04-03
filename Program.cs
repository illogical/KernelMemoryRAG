using KernelMemoryRAG.Models;
using KernelMemoryRAG.Services;
using KernelMemoryRAG.Utilities;
using Microsoft.Extensions.Configuration;

namespace KernelMemoryRAG;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            // Display welcome message
            Console.WriteLine("=======================================================");
            Console.WriteLine("  Kernel Memory RAG Application");
            Console.WriteLine("=======================================================");
            Console.WriteLine();

            // Check if a directory path was provided
            // if (args.Length == 0)
            // {
            //     ConsoleHelper.WriteError("Please provide a directory path containing documents to process.");
            //     Console.WriteLine("Usage: KernelMemoryRagApp <directory_path>");
            //     return;
            // }

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

            // Check for API keys in environment variables if not in config
            appSettings.OpenAIApiKey ??= Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            appSettings.OllamaEndpoint ??= Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? "http://localhost:11434";
            
            // Validate settings based on the selected AI service type
            if (appSettings.AIServiceType == "OpenAI" && string.IsNullOrEmpty(appSettings.OpenAIApiKey))
            {
                ConsoleHelper.WriteError("OpenAI API key is required when using OpenAI service type.");
                ConsoleHelper.WriteInfo("You can set it in appsettings.json or as environment variable OPENAI_API_KEY");
                return;
            }
            else if (appSettings.AIServiceType == "Ollama" && string.IsNullOrEmpty(appSettings.OllamaEndpoint))
            {
                ConsoleHelper.WriteError("Ollama endpoint is required when using Ollama service type.");
                ConsoleHelper.WriteInfo("You can set it in appsettings.json or as environment variable OLLAMA_ENDPOINT");
                ConsoleHelper.WriteInfo("Default endpoint is http://localhost:11434");
                return;
            }

            // Initialize memory service
            ConsoleHelper.WriteInfo("Initializing Kernel Memory...");
            var memoryService = new MemoryService(appSettings);
            ConsoleHelper.WriteSuccess("Kernel Memory initialized successfully.");

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
            foreach (var file in supportedFiles)
            {
                Console.WriteLine($"- {Path.GetFileName(file)} ({DocumentProcessor.GetFileType(file)}, {DocumentProcessor.GetFileSizeFormatted(file)})");
            }
            Console.WriteLine();

            // Confirm import
            ConsoleHelper.WriteInfo("Starting document import...");
            await memoryService.ImportDocumentsFromDirectoryAsync(directoryPath);
            ConsoleHelper.WriteSuccess("Document import completed.");

            // Interactive Q&A mode
            Console.WriteLine("\n=======================================================");
            Console.WriteLine("  RAG Q&A Mode - Ask questions about your documents");
            Console.WriteLine("  (Type 'exit' to quit)");
            Console.WriteLine("=======================================================");

            while (true)
            {
                Console.WriteLine();
                Console.Write("Your question: ");
                string question = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(question))
                    continue;

                if (question.Trim().ToLower() == "exit")
                    break;

                string answer = await memoryService.AskQuestionAsync(question);
                Console.WriteLine();
                ConsoleHelper.WriteInfo("Answer:");
                Console.WriteLine(answer);
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"An error occurred: {ex.Message}");
            ConsoleHelper.WriteError(ex.StackTrace ?? string.Empty);
        }
    }
}
