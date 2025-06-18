using BlazorLore.Format.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorLore.Format.Cli.Configuration;

public static class ConfigurationLoader
{
    private const string ConfigFileName = ".blazorfmt.json";
    private static readonly BlazorFormatterOptionsContext JsonContext = new();

    public static BlazorFormatterOptions LoadConfiguration(string? configPath = null)
    {
        var searchPath = configPath ?? FindConfigFile();
        
        if (searchPath != null && File.Exists(searchPath))
        {
            try
            {
                var json = File.ReadAllText(searchPath);
                return JsonSerializer.Deserialize(json, JsonContext.BlazorFormatterOptions) 
                    ?? new BlazorFormatterOptions();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Warning: Failed to load config from {searchPath}: {ex.Message}");
            }
        }
        
        return new BlazorFormatterOptions();
    }

    public static void SaveConfiguration(BlazorFormatterOptions options, string? configPath = null)
    {
        var path = configPath ?? Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);
        var json = JsonSerializer.Serialize(options, JsonContext.BlazorFormatterOptions);
        File.WriteAllText(path, json);
    }

    private static string? FindConfigFile()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        while (!string.IsNullOrEmpty(currentDir))
        {
            var configPath = Path.Combine(currentDir, ConfigFileName);
            if (File.Exists(configPath))
                return configPath;
            
            var parent = Directory.GetParent(currentDir);
            if (parent == null)
                break;
                
            currentDir = parent.FullName;
        }
        
        return null;
    }
}