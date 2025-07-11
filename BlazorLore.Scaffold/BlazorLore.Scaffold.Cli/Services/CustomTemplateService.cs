using System.Text.Json;
using System.Text.Json.Serialization;
using Scriban;

namespace BlazorLore.Scaffold.Cli.Services;

public class CustomTemplateService
{
    private const string DefaultTemplatesFolder = ".blazor-templates";
    private readonly string _globalTemplatesPath;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public CustomTemplateService()
    {
        _globalTemplatesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".blazor-templates"
        );
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = TemplateJsonContext.Default
        };
    }

    public async Task InitializeTemplatesAsync(string path = DefaultTemplatesFolder)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(fullPath);

        // Create example custom component template
        var componentDir = Path.Combine(fullPath, "my-component");
        Directory.CreateDirectory(componentDir);

        // Create config file
        var config = new TemplateConfig
        {
            Name = "My Custom Component",
            Description = "A custom component template example",
            Category = "component",
            Files = new List<TemplateFile>
            {
                new TemplateFile 
                { 
                    Source = "component.razor.scriban", 
                    Output = "{{ name }}.razor" 
                },
                new TemplateFile
                { 
                    Source = "component.razor.cs.scriban", 
                    Output = "{{ name }}.razor.cs",
                    Condition = "{{ has_code_behind }}"
                }
            },
            Parameters = new List<TemplateParameter>
            {
                new TemplateParameter
                { 
                    Name = "includeHeader", 
                    Type = "bool", 
                    DefaultValue = true,
                    Description = "Include a header section"
                },
                new TemplateParameter
                {
                    Name = "author",
                    Type = "string",
                    DefaultValue = "",
                    Description = "Component author name"
                }
            }
        };

        await File.WriteAllTextAsync(
            Path.Combine(componentDir, "template.config.json"),
            JsonSerializer.Serialize(config, _jsonOptions)
        );

        // Create component template
        await File.WriteAllTextAsync(
            Path.Combine(componentDir, "component.razor.scriban"),
            @"@* 
   Custom Component Template
   Generated by BlazorLore Scaffold
   {{ if custom.author }}Author: {{ custom.author }}{{ end }}
*@
@namespace {{ namespace }}

<div class=""{{ name | string.kebabcase }}-component"">
    {{ if custom.include_header }}
    <h2>{{ name | string.humanize }}</h2>
    {{ end }}
    
    <p>This is a custom component generated from a user template.</p>
    
    @if (ShowContent)
    {
        <div class=""content"">
            @ChildContent
        </div>
    }
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public bool ShowContent { get; set; } = true;
    
    protected override void OnInitialized()
    {
        // Custom initialization logic
        base.OnInitialized();
    }
}"
        );

        // Create code-behind template
        await File.WriteAllTextAsync(
            Path.Combine(componentDir, "component.razor.cs.scriban"),
            @"using Microsoft.AspNetCore.Components;

namespace {{ namespace }};

/// <summary>
/// {{ name }} component
/// {{ if custom.author }}Created by: {{ custom.author }}{{ end }}
/// </summary>
public partial class {{ name }} : ComponentBase
{
    protected override async Task OnInitializedAsync()
    {
        // Async initialization
        await base.OnInitializedAsync();
    }
    
    protected override void OnParametersSet()
    {
        // Parameter validation
        base.OnParametersSet();
    }
}"
        );

        Console.WriteLine($"✅ Custom templates initialized at: {fullPath}");
        Console.WriteLine($"   - Example template: my-component");
        Console.WriteLine($"\n💡 To use custom templates:");
        Console.WriteLine($"   blazor-scaffold component MyComponent --template my-component");
    }

    public async Task<List<TemplateInfo>> DiscoverTemplatesAsync(string? category = null)
    {
        var templates = new List<TemplateInfo>();

        // Search in multiple locations
        var searchPaths = new List<string>();

        // 1. Current directory templates
        var localPath = Path.Combine(Directory.GetCurrentDirectory(), DefaultTemplatesFolder);
        if (Directory.Exists(localPath))
        {
            searchPaths.Add(localPath);
        }

        // 2. Global user templates
        if (Directory.Exists(_globalTemplatesPath))
        {
            searchPaths.Add(_globalTemplatesPath);
        }

        // 3. Built-in templates
        var builtInPath = Path.Combine(AppContext.BaseDirectory, "Templates");
        if (Directory.Exists(builtInPath))
        {
            // Add built-in templates as special entries
            if (category == null || category == "component")
            {
                templates.Add(new TemplateInfo
                {
                    Name = "component",
                    DisplayName = "Standard Component (built-in)",
                    Description = "Standard Blazor component with optional code-behind and CSS",
                    Category = "component",
                    Path = Path.Combine(builtInPath, "Component"),
                    IsBuiltIn = true
                });
            }
            
            if (category == null || category == "form")
            {
                templates.Add(new TemplateInfo
                {
                    Name = "form",
                    DisplayName = "Standard Form (built-in)",
                    Description = "Form component with validation",
                    Category = "form",
                    Path = Path.Combine(builtInPath, "Form"),
                    IsBuiltIn = true
                });
            }

            if (category == null || category == "service")
            {
                templates.Add(new TemplateInfo
                {
                    Name = "service",
                    DisplayName = "Standard Service (built-in)",
                    Description = "Service class with optional interface",
                    Category = "service",
                    Path = Path.Combine(builtInPath, "Service"),
                    IsBuiltIn = true
                });
            }
        }

        // Search custom templates
        foreach (var searchPath in searchPaths)
        {
            var dirs = Directory.GetDirectories(searchPath);
            
            foreach (var dir in dirs)
            {
                var configPath = Path.Combine(dir, "template.config.json");
                if (File.Exists(configPath))
                {
                    try
                    {
                        var configJson = await File.ReadAllTextAsync(configPath);
                        var config = JsonSerializer.Deserialize<TemplateConfig>(configJson, _jsonOptions);
                        
                        if (config != null && (category == null || config.Category == category))
                        {
                            templates.Add(new TemplateInfo
                            {
                                Name = Path.GetFileName(dir),
                                DisplayName = config.Name,
                                Description = config.Description,
                                Category = config.Category,
                                Path = dir,
                                IsBuiltIn = false,
                                Config = config
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Failed to load template config from {dir}: {ex.Message}");
                    }
                }
            }
        }

        return templates;
    }

    public async Task<bool> GenerateFromCustomTemplateAsync(
        string templateName,
        string outputPath,
        Dictionary<string, object> variables)
    {
        var templates = await DiscoverTemplatesAsync();
        var template = templates.FirstOrDefault(t => t.Name == templateName);
        
        if (template == null)
        {
            throw new InvalidOperationException($"Template '{templateName}' not found");
        }

        if (template.IsBuiltIn)
        {
            // Built-in templates are handled by their respective generators
            return false;
        }

        Directory.CreateDirectory(outputPath);

        foreach (var file in template.Config!.Files)
        {
            // Check condition
            if (!string.IsNullOrEmpty(file.Condition))
            {
                var conditionTemplate = Template.Parse(file.Condition);
                var conditionResult = await conditionTemplate.RenderAsync(variables);
                
                if (conditionResult.Trim().ToLower() != "true")
                {
                    continue;
                }
            }

            // Process file
            var sourcePath = Path.Combine(template.Path, file.Source);
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"Warning: Template file not found: {file.Source}");
                continue;
            }

            var templateContent = await File.ReadAllTextAsync(sourcePath);
            var contentTemplate = Template.Parse(templateContent);
            var renderedContent = await contentTemplate.RenderAsync(variables);

            // Render output filename
            var outputNameTemplate = Template.Parse(file.Output);
            var outputFileName = await outputNameTemplate.RenderAsync(variables);
            
            var fullOutputPath = Path.Combine(outputPath, outputFileName);
            await File.WriteAllTextAsync(fullOutputPath, renderedContent);
            
            Console.WriteLine($"   - Generated: {outputFileName}");
        }

        return true;
    }
}

public class TemplateInfo
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public string Path { get; set; } = "";
    public bool IsBuiltIn { get; set; }
    public TemplateConfig? Config { get; set; }
}

public class TemplateConfig
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public List<TemplateFile> Files { get; set; } = new();
    public List<TemplateParameter> Parameters { get; set; } = new();
}

public class TemplateFile
{
    public string Source { get; set; } = "";
    public string Output { get; set; } = "";
    public string? Condition { get; set; }
}

public class TemplateParameter
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "string";
    public object? DefaultValue { get; set; }
    public string Description { get; set; } = "";
}