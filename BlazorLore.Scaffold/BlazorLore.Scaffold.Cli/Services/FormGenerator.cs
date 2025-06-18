using Scriban;

namespace BlazorLore.Scaffold.Cli.Services;

public class FormGenerator
{
    private readonly string _templateBasePath;

    public FormGenerator()
    {
        // Get the directory where the executable is located
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(assemblyLocation) ?? ".";
        _templateBasePath = Path.Combine(directory, "Templates", "Form");
    }

    public async Task GenerateFormAsync(ModelInfo modelInfo, string formName, string outputPath, bool isEditForm, string submitAction)
    {
        // Ensure the output directory exists
        Directory.CreateDirectory(outputPath);

        // Prepare the model for the template
        var model = new
        {
            FormName = formName,
            ModelInfo = modelInfo,
            IsEditForm = isEditForm,
            SubmitAction = submitAction,
            Namespace = "MyApp.Components", // This could be made configurable
            ModelInstance = char.ToLower(modelInfo.Name[0]) + modelInfo.Name.Substring(1)
        };

        // Generate the form component
        var templatePath = Path.Combine(_templateBasePath, "Form.razor.scriban");
        var outputFilePath = Path.Combine(outputPath, $"{formName}.razor");

        await GenerateFileFromTemplateAsync(templatePath, outputFilePath, model);
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

public class ModelInfo
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public bool IsRecord { get; set; }
    public List<PropertyInfo> Properties { get; set; } = new();
}

public class PropertyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public List<ValidationAttribute> ValidationAttributes { get; set; } = new();
}

public class ValidationAttribute
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
}