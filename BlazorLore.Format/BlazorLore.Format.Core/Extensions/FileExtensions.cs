namespace BlazorLore.Format.Core.Extensions;

public static class FileExtensions
{
    private static readonly IBlazorFormatter _formatter = new BlazorFormatter();

    public static async Task FormatBlazorFileAsync(string filePath, BlazorFormatterOptions? options = null)
    {
        var content = await File.ReadAllTextAsync(filePath);
        var formatted = await _formatter.FormatAsync(content, options);
        await File.WriteAllTextAsync(filePath, formatted);
    }

    public static void FormatBlazorFile(string filePath, BlazorFormatterOptions? options = null)
    {
        var content = File.ReadAllText(filePath);
        var formatted = _formatter.Format(content, options);
        File.WriteAllText(filePath, formatted);
    }

    public static async Task FormatBlazorFilesAsync(string directory, string searchPattern = "*.razor", BlazorFormatterOptions? options = null)
    {
        var files = Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);
        
        var tasks = files.Select(file => FormatBlazorFileAsync(file, options));
        await Task.WhenAll(tasks);
    }

    public static void FormatBlazorFiles(string directory, string searchPattern = "*.razor", BlazorFormatterOptions? options = null)
    {
        var files = Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);
        
        foreach (var file in files)
        {
            FormatBlazorFile(file, options);
        }
    }
}