namespace KernelMemoryRAG.Models;

public class AppSettings
{
    public string? OpenAIApiKey { get; set; }
    public string? OpenAIEndpoint { get; set; }
    public string? OpenAIModelId { get; set; }
    
    // Ollama settings
    public string? OllamaEndpoint { get; set; } = "http://localhost:11434";
    public string? OllamaModelId { get; set; } = "gemma3";
    public string? OllamaEmbeddingModelId { get; set; } = "nomic-embed-text";
    
    // Flag to determine which AI service to use
    public string AIServiceType { get; set; } = "Ollama"; // Options: "OpenAI", "Ollama"
}
