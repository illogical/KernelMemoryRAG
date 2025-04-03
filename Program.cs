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
            // Load configuration
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var appSettings = config.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();

            // Initialize memory service
            ConsoleHelper.WriteInfo("Initializing Kernel Memory...");
            var memoryService = new MemoryService(appSettings);
            ConsoleHelper.WriteSuccess("Kernel Memory initialized successfully.");
            string directoryPath = appSettings.DocumentFolderPath;

            // Validate directory path
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            {
                ConsoleHelper.WriteError($"Directory not found: {directoryPath}");
                return;
            }

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

            // Import documents
            ConsoleHelper.WriteInfo("Starting document import...");
            await memoryService.ImportDocumentsFromDirectoryAsync(directoryPath, Constants.Contexts.GameHistory);
            ConsoleHelper.WriteSuccess("Document import completed.");
            Console.WriteLine();

            // Initial question
            string initialQuestion = "What games did I most often mention as 'Itching to Play'?";
            string answer = await memoryService.AskQuestionAsync(initialQuestion, Constants.Contexts.GameHistory);
            Console.WriteLine("-------------------");
            Console.WriteLine(initialQuestion);
            Console.WriteLine(answer);
            Console.WriteLine("-------------------");
            Console.WriteLine("Press enter to exit. Otherwise, type a new question.");

            // Interactive Q&A loop
            string? nextQuestion;
            do
            {
                Console.Write("> ");
                nextQuestion = Console.ReadLine();

                if (string.IsNullOrEmpty(nextQuestion))
                    break;

                answer = await memoryService.AskQuestionAsync(nextQuestion, Constants.Contexts.GameHistory);
                Console.WriteLine("-------------------");
                Console.WriteLine(nextQuestion);
                Console.WriteLine(answer);
                Console.WriteLine("-------------------");
            }
            while (!string.IsNullOrEmpty(nextQuestion));
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"An error occurred: {ex.Message}");
            ConsoleHelper.WriteError(ex.StackTrace ?? string.Empty);
        }
    }
}

