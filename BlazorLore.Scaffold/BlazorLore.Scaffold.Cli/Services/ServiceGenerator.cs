using Scriban;

namespace BlazorLore.Scaffold.Cli.Services;

public class ServiceGenerator
{
    private readonly string _templateBasePath;

    public ServiceGenerator()
    {
        var directory = AppContext.BaseDirectory;
        _templateBasePath = Path.Combine(directory, "Templates", "Service");
    }

    public async Task GenerateServiceAsync(
        string name,
        string outputPath,
        bool generateInterface = true,
        bool isRepository = false,
        string? entityName = null,
        string? entityIdType = null,
        List<(string type, string name)>? dependencies = null)
    {
        // Ensure output directory exists
        Directory.CreateDirectory(outputPath);

        // Detect namespace from project structure
        var namespaceName = await DetectNamespaceAsync(outputPath);

        // Prepare dependencies for template
        var templateDependencies = (dependencies?.Select(d => new
        {
            type = d.type,
            parameter_name = char.ToLower(d.name[0]) + d.name.Substring(1),
            field_name = "_" + char.ToLower(d.name[0]) + d.name.Substring(1)
        }) ?? Enumerable.Empty<object>()).ToList();

        // Prepare the model for the templates
        var model = new
        {
            service_name = name,
            @namespace = namespaceName,  // Changed to match template
            generate_interface = generateInterface,
            include_logger = true,
            is_repository = isRepository,
            entity_name = entityName ?? "Entity",
            entity_id_type = entityIdType ?? "int",
            dependencies = templateDependencies,
            add_usings = true,
            interface_methods = new List<string>()
        };

        // Generate the service implementation
        if (generateInterface)
        {
            // Generate service with interface implementation
            await GenerateFileFromTemplateAsync(
                Path.Combine(_templateBasePath, "Service.cs.scriban"),
                Path.Combine(outputPath, $"{name}.cs"),
                model);

            // Generate separate interface file
            await GenerateFileFromTemplateAsync(
                Path.Combine(_templateBasePath, "IService.cs.scriban"),
                Path.Combine(outputPath, $"I{name}.cs"),
                model);
        }
        else
        {
            // Generate standalone service
            await GenerateFileFromTemplateAsync(
                Path.Combine(_templateBasePath, "Service.cs.scriban"),
                Path.Combine(outputPath, $"{name}.cs"),
                model);
        }
    }

    private async Task GenerateFileFromTemplateAsync(string templatePath, string outputPath, object model)
    {
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template not found: {templatePath}");
        }

        // Read the template
        var templateContent = await File.ReadAllTextAsync(templatePath);

        // Parse and render the template
        var template = Template.Parse(templateContent);
        var result = await template.RenderAsync(model);

        // Write the result
        await File.WriteAllTextAsync(outputPath, result);
    }

    private async Task<string> DetectNamespaceAsync(string outputPath)
    {
        // Look for .csproj file to determine project root and namespace
        var directory = new DirectoryInfo(outputPath);
        
        while (directory != null)
        {
            var csprojFiles = directory.GetFiles("*.csproj");
            if (csprojFiles.Length > 0)
            {
                var projectName = Path.GetFileNameWithoutExtension(csprojFiles[0].Name);
                var relativePath = Path.GetRelativePath(directory.FullName, outputPath);
                
                if (relativePath != ".")
                {
                    var namespaceParts = relativePath.Replace(Path.DirectorySeparatorChar, '.');
                    return $"{projectName}.{namespaceParts}";
                }
                
                return projectName;
            }
            
            directory = directory.Parent;
        }

        // Check for _Imports.razor for namespace hints
        directory = new DirectoryInfo(outputPath);
        while (directory != null)
        {
            var importsFile = Path.Combine(directory.FullName, "_Imports.razor");
            if (File.Exists(importsFile))
            {
                var content = await File.ReadAllTextAsync(importsFile);
                var namespaceMatch = System.Text.RegularExpressions.Regex.Match(
                    content, @"@namespace\s+(\S+)");
                
                if (namespaceMatch.Success)
                {
                    return namespaceMatch.Groups[1].Value;
                }
            }
            
            directory = directory.Parent;
        }

        return "MyApp.Services";
    }
}