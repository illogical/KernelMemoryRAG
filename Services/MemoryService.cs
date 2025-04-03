using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using KernelMemoryRAG.Models;
using KernelMemoryRAG.Utilities;

namespace KernelMemoryRAG.Services;

public class MemoryService
{
    private readonly IKernelMemory _memory;
    private readonly Kernel _kernel;

    public MemoryService(AppSettings settings)
    {
        ConsoleHelper.WriteInfo($"Using Ollama service at {settings.OllamaEndpoint}");
        
        // Configure Kernel Memory with default settings
        // This will use in-memory storage and simple text processing
        _memory = new KernelMemoryBuilder().Build();

        // Create Semantic Kernel instance
        var kernelBuilder = Kernel.CreateBuilder();

        // Add Ollama to the kernel
        var endpoint = settings.OllamaEndpoint ?? "http://localhost:11434";
        var modelId = settings.OllamaModelId ?? "llama2";
        
        try
        {
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            kernelBuilder.AddOllamaChatCompletion(modelId, new Uri(endpoint));
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            ConsoleHelper.WriteSuccess($"Successfully configured Ollama with model: {modelId}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Error configuring Ollama: {ex.Message}");
            ConsoleHelper.WriteWarning("Continuing with limited functionality. RAG may not work properly.");
        }

        // Build the kernel
        _kernel = kernelBuilder.Build();

        // Import the memory plugin
        _kernel.ImportPluginFromObject(new MemoryPlugin(_memory));
    }

    public async Task ImportDocumentsFromDirectoryAsync(string directoryPath, string indexName = "default")
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }

        var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
        ConsoleHelper.WriteInfo($"Found {files.Length} files in {directoryPath}");

        foreach (var file in files)
        {
            try
            {
                ConsoleHelper.WriteInfo($"Importing: {Path.GetFileName(file)}");
                await _memory.ImportDocumentAsync(file, documentId: Path.GetFileNameWithoutExtension(file), index: indexName);
                ConsoleHelper.WriteSuccess($"Successfully imported: {Path.GetFileName(file)}");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Error importing {Path.GetFileName(file)}: {ex.Message}");
            }
        }
    }

    public async Task<string> AskQuestionAsync(string question, string indexName = "default")
    {
        try
        {
            ConsoleHelper.WriteInfo($"Asking: {question}");
            var answer = await _memory.AskAsync(question, index: indexName);
            return answer.Result;
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Error asking question: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }

    public Kernel GetKernel()
    {
        return _kernel;
    }

    public IKernelMemory GetMemory()
    {
        return _memory;
    }
}
