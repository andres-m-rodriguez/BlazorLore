using Scriban;

namespace BlazorLore.Scaffold.Cli.Services;

public class ComponentGenerator
{
    private readonly string _templateBasePath;

    public ComponentGenerator()
    {
        // Use AppContext.BaseDirectory for AOT and single-file compatibility
        var directory = AppContext.BaseDirectory;
        _templateBasePath = Path.Combine(directory, "Templates", "Component");
    }

    public async Task GenerateComponentAsync(string name, string outputPath, bool generateCodeBehind, bool generateCss)
    {
        // Ensure the output directory exists
        Directory.CreateDirectory(outputPath);

        // Prepare the model for the templates
        var model = new
        {
            Name = name,
            Namespace = "MyApp.Components", // This could be made configurable
            HasCodeBehind = generateCodeBehind,
            HasCss = generateCss
        };

        // Generate the main component file
        await GenerateFileFromTemplateAsync(
            Path.Combine(_templateBasePath, "Component.razor.scriban"),
            Path.Combine(outputPath, $"{name}.razor"),
            model);

        // Generate code-behind if requested
        if (generateCodeBehind)
        {
            await GenerateFileFromTemplateAsync(
                Path.Combine(_templateBasePath, "Component.razor.cs.scriban"),
                Path.Combine(outputPath, $"{name}.razor.cs"),
                model);
        }

        // Generate CSS if requested
        if (generateCss)
        {
            await GenerateFileFromTemplateAsync(
                Path.Combine(_templateBasePath, "Component.razor.css.scriban"),
                Path.Combine(outputPath, $"{name}.razor.css"),
                model);
        }
    }

    private async Task GenerateFileFromTemplateAsync(string templatePath, string outputPath, object model)
    {
        // Read the template
        var templateContent = await File.ReadAllTextAsync(templatePath);

        // Parse and render the template
        var template = Template.Parse(templateContent);
        var result = await template.RenderAsync(model);

        // Write the result
        await File.WriteAllTextAsync(outputPath, result);
    }
}