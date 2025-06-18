using System.Text;

namespace BlazorLore.Scaffold.Cli.Tests.Utilities;

public static class TestHelpers
{
    /// <summary>
    /// Creates a temporary directory with automatic cleanup
    /// </summary>
    public static TemporaryDirectory CreateTempDirectory(string prefix = "Test")
    {
        return new TemporaryDirectory(prefix);
    }
    
    /// <summary>
    /// Creates a mock C# model file with the specified properties
    /// </summary>
    public static async Task<string> CreateModelFileAsync(string directory, string modelName, params (string Type, string Name, string[]? Attributes)[] properties)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.ComponentModel.DataAnnotations;");
        sb.AppendLine();
        sb.AppendLine("namespace TestModels");
        sb.AppendLine("{");
        sb.AppendLine($"    public class {modelName}");
        sb.AppendLine("    {");
        
        foreach (var (type, name, attributes) in properties)
        {
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    sb.AppendLine($"        [{attribute}]");
                }
            }
            sb.AppendLine($"        public {type} {name} {{ get; set; }}");
            sb.AppendLine();
        }
        
        sb.AppendLine("    }");
        sb.AppendLine("}");
        
        var filePath = Path.Combine(directory, $"{modelName}.cs");
        await File.WriteAllTextAsync(filePath, sb.ToString());
        return filePath;
    }
    
    /// <summary>
    /// Creates a mock Blazor component file
    /// </summary>
    public static async Task<string> CreateComponentFileAsync(string directory, string componentName, string? codeBlock = null, params string[] injectDirectives)
    {
        var sb = new StringBuilder();
        
        foreach (var inject in injectDirectives)
        {
            sb.AppendLine(inject);
        }
        
        if (injectDirectives.Any())
        {
            sb.AppendLine();
        }
        
        sb.AppendLine($"<h3>{componentName}</h3>");
        sb.AppendLine();
        
        if (!string.IsNullOrEmpty(codeBlock))
        {
            sb.AppendLine("@code {");
            sb.AppendLine(codeBlock);
            sb.AppendLine("}");
        }
        
        var filePath = Path.Combine(directory, $"{componentName}.razor");
        await File.WriteAllTextAsync(filePath, sb.ToString());
        return filePath;
    }
    
    /// <summary>
    /// Creates template files for testing generators
    /// </summary>
    public static void SetupTemplates(string assemblyDirectory, Dictionary<string, string> templates)
    {
        foreach (var (relativePath, content) in templates)
        {
            var fullPath = Path.Combine(assemblyDirectory, relativePath);
            var directory = Path.GetDirectoryName(fullPath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(fullPath, content);
        }
    }
    
    /// <summary>
    /// Compares two files ignoring line endings and extra whitespace
    /// </summary>
    public static bool FilesAreEquivalent(string file1Path, string file2Path)
    {
        var content1 = NormalizeContent(File.ReadAllText(file1Path));
        var content2 = NormalizeContent(File.ReadAllText(file2Path));
        return content1 == content2;
    }
    
    private static string NormalizeContent(string content)
    {
        return content
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Trim();
    }
}

/// <summary>
/// Represents a temporary directory that is automatically cleaned up when disposed
/// </summary>
public class TemporaryDirectory : IDisposable
{
    public string Path { get; }
    
    public TemporaryDirectory(string prefix = "Test")
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{prefix}_{Guid.NewGuid()}");
        Directory.CreateDirectory(Path);
    }
    
    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            try
            {
                Directory.Delete(Path, true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }
}