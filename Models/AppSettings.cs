namespace KernelMemoryRAG.Models;

public class AppSettings
{
    // Ollama settings
    public string? OllamaEndpoint { get; set; } = "http://localhost:11434";
    public string? OllamaModelId { get; set; } = "gemma3:12b";
    public string? OllamaEmbeddingModelId { get; set; } = "nomic-embed-text";
    
    // Flag to determine which AI service to use
    public string AIServiceType { get; set; } = "Ollama"; // Options: "OpenAI", "Ollama"
    public string? DocumentFolderPath { get; set; } = String.Empty;
}
