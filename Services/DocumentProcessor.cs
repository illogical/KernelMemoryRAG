using KernelMemoryRAG.Utilities;

namespace KernelMemoryRAG.Services;

public class DocumentProcessor
{
    // Supported file extensions
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".pdf", ".docx", ".doc", ".md", ".html", ".htm", ".csv", ".json", ".xml"
    };

    public static bool IsFileSupported(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return SupportedExtensions.Contains(extension);
    }

    public static List<string> GetSupportedFiles(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            ConsoleHelper.WriteError($"Directory not found: {directoryPath}");
            return new List<string>();
        }

        try
        {
            var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                .Where(IsFileSupported)
                .ToList();

            ConsoleHelper.WriteInfo($"Found {files.Count} supported documents in {directoryPath}");
            return files;
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Error scanning directory: {ex.Message}");
            return new List<string>();
        }
    }

    public static string GetFileType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".txt" => "Text",
            ".pdf" => "PDF",
            ".docx" or ".doc" => "Word Document",
            ".md" => "Markdown",
            ".html" or ".htm" => "HTML",
            ".csv" => "CSV",
            ".json" => "JSON",
            ".xml" => "XML",
            _ => "Unknown"
        };
    }

    public static long GetFileSize(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }
        catch
        {
            return 0;
        }
    }

    public static string GetFileSizeFormatted(string filePath)
    {
        var bytes = GetFileSize(filePath);
        
        if (bytes < 1024)
            return $"{bytes} bytes";
        
        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:F2} KB";
        
        if (bytes < 1024 * 1024 * 1024)
            return $"{bytes / (1024.0 * 1024.0):F2} MB";
        
        return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
    }
}
