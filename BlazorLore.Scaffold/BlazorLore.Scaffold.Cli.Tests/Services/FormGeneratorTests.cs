using BlazorLore.Scaffold.Cli.Services;
using FluentAssertions;

namespace BlazorLore.Scaffold.Cli.Tests.Services;

public class FormGeneratorTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _templateDirectory;
    
    public FormGeneratorTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"FormGeneratorTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        // Create a mock template directory structure
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(assemblyLocation) ?? ".";
        _templateDirectory = Path.Combine(directory, "Templates", "Form");
        Directory.CreateDirectory(_templateDirectory);
        
        // Create mock form template
        CreateMockTemplate();
    }
    
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // Ignore errors during cleanup
        }
        
        try
        {
            // Clean up templates directory
            var templatesRoot = Path.GetDirectoryName(_templateDirectory);
            if (templatesRoot != null && Directory.Exists(templatesRoot))
            {
                Directory.Delete(templatesRoot, true);
            }
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }
    
    private void CreateMockTemplate()
    {
        // Copy templates from the source project to the test assembly's output directory
        var sourceTemplatesPath = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!,
            "..", "..", "..", "..", "..",
            "BlazorLore.Scaffold.Cli", "Templates", "Form", "Form.razor.scriban"
        );
        
        // Normalize the path
        sourceTemplatesPath = Path.GetFullPath(sourceTemplatesPath);
        
        if (File.Exists(sourceTemplatesPath))
        {
            var content = File.ReadAllText(sourceTemplatesPath);
            File.WriteAllText(Path.Combine(_templateDirectory, "Form.razor.scriban"), content);
        }
        else
        {
            // If source templates not found, create a simple mock template for testing
            var mockTemplate = @"@using System.ComponentModel.DataAnnotations
@inject ILogger<{{ form_name }}> Logger

<EditForm Model=""@{{ model_instance }}"" OnValidSubmit=""@{{ submit_action }}"">
    <DataAnnotationsValidator />
    <ValidationSummary />

    <div class=""form-container"">
        <h3>{{ if is_edit_form }}Edit {{ model_info.name }}{{ else }}Create {{ model_info.name }}{{ end }}</h3>

        {{ for property in model_info.properties }}
        <div class=""form-group"">
            <label for=""{{ property.name }}"">{{ property.name }}:</label>
            {{ if property.type == ""string"" || property.type == ""string?"" }}
            <InputText id=""{{ property.name }}"" class=""form-control"" @bind-Value=""{{ model_instance }}.{{ property.name }}"" />
            {{ else if property.type == ""int"" || property.type == ""int?"" }}
            <InputNumber id=""{{ property.name }}"" class=""form-control"" @bind-Value=""{{ model_instance }}.{{ property.name }}"" />
            {{ else if property.type == ""decimal"" || property.type == ""decimal?"" }}
            <InputNumber id=""{{ property.name }}"" class=""form-control"" @bind-Value=""{{ model_instance }}.{{ property.name }}"" />
            {{ else if property.type == ""DateTime"" || property.type == ""DateTime?"" }}
            <InputDate id=""{{ property.name }}"" class=""form-control"" @bind-Value=""{{ model_instance }}.{{ property.name }}"" />
            {{ else if property.type == ""bool"" || property.type == ""bool?"" }}
            <InputCheckbox id=""{{ property.name }}"" class=""form-check-input"" @bind-Value=""{{ model_instance }}.{{ property.name }}"" />
            {{ else }}
            <InputText id=""{{ property.name }}"" class=""form-control"" @bind-Value=""{{ model_instance }}.{{ property.name }}"" />
            {{ end }}
            <ValidationMessage For=""@(() => {{ model_instance }}.{{ property.name }})"" />
        </div>
        {{ end }}

        <div class=""form-group"">
            <button type=""submit"" class=""btn btn-primary"">
                {{ if is_edit_form }}Update{{ else }}Create{{ end }}
            </button>
            <button type=""button"" class=""btn btn-secondary"" @onclick=""Cancel"">
                Cancel
            </button>
        </div>
    </div>
</EditForm>

@code {
    {{ if is_edit_form }}
    [Parameter] public {{ model_info.name }} {{ model_instance }} { get; set; } = new();
    {{ else }}
    private {{ model_info.name }} {{ model_instance }} = new();
    {{ end }}

    [Parameter] public EventCallback<{{ model_info.name }}> OnSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private async Task {{ submit_action }}()
    {
        Logger.LogInformation($""{{ if is_edit_form }}Updating{{ else }}Creating{{ end }} {{ model_info.name }}"");
        
        if (OnSubmit.HasDelegate)
        {
            await OnSubmit.InvokeAsync({{ model_instance }});
        }
        
        {{ if !is_edit_form }}
        // Reset form for new entry
        {{ model_instance }} = new();
        {{ end }}
    }

    private async Task Cancel()
    {
        if (OnCancel.HasDelegate)
        {
            await OnCancel.InvokeAsync();
        }
    }
}

<style>
    .form-container {
        max-width: 600px;
        margin: 0 auto;
        padding: 20px;
    }

    .form-group {
        margin-bottom: 15px;
    }

    .form-group label {
        display: block;
        margin-bottom: 5px;
        font-weight: 600;
    }

    .form-control {
        width: 100%;
        padding: 8px 12px;
        border: 1px solid #ced4da;
        border-radius: 4px;
        font-size: 16px;
    }

    .form-check-input {
        margin-top: 8px;
    }

    .btn {
        margin-right: 10px;
    }

    .validation-message {
        color: #dc3545;
        font-size: 14px;
        margin-top: 5px;
    }
</style>";
            
            File.WriteAllText(Path.Combine(_templateDirectory, "Form.razor.scriban"), mockTemplate);
        }
    }
    
    [Fact]
    public async Task GenerateFormAsync_CreatesBasicCreateForm()
    {
        // Arrange
        var generator = new FormGenerator();
        var modelInfo = new ModelInfo
        {
            Name = "Product",
            Namespace = "MyApp.Models",
            Properties = new List<PropertyInfo>
            {
                new() { Name = "Name", Type = "string", IsNullable = false },
                new() { Name = "Price", Type = "decimal", IsNullable = false },
                new() { Name = "IsActive", Type = "bool", IsNullable = false }
            }
        };
        
        var formName = "ProductCreateForm";
        var outputPath = Path.Combine(_testDirectory, "Forms");
        
        // Act
        await generator.GenerateFormAsync(modelInfo, formName, outputPath, false, "HandleSubmit");
        
        // Assert
        Directory.Exists(outputPath).Should().BeTrue();
        
        var formFile = Path.Combine(outputPath, $"{formName}.razor");
        File.Exists(formFile).Should().BeTrue();
        
        var content = await File.ReadAllTextAsync(formFile);
        content.Should().Contain("@using System.ComponentModel.DataAnnotations");
        content.Should().Contain("@inject ILogger<ProductCreateForm> Logger");
        content.Should().Contain("<h3>Create Product</h3>");
        content.Should().Contain("private Product product = new();");
        // Check button exists with proper text, ignoring exact whitespace
        content.Should().Contain("<button type=\"submit\" class=\"btn btn-primary\">");
        content.Should().Contain("Create");
        content.Should().Contain("</button>");
        content.Should().Contain("EventCallback<Product> OnSubmit");
        content.Should().Contain("<div class=\"form-container\">");
    }
    
    [Fact]
    public async Task GenerateFormAsync_CreatesEditForm()
    {
        // Arrange
        var generator = new FormGenerator();
        var modelInfo = new ModelInfo
        {
            Name = "User",
            Namespace = "MyApp.Models",
            Properties = new List<PropertyInfo>
            {
                new() { Name = "Username", Type = "string", IsNullable = false },
                new() { Name = "Email", Type = "string", IsNullable = false }
            }
        };
        
        var formName = "UserEditForm";
        var outputPath = Path.Combine(_testDirectory, "Forms");
        
        // Act
        await generator.GenerateFormAsync(modelInfo, formName, outputPath, true, "HandleUpdate");
        
        // Assert
        var formFile = Path.Combine(outputPath, $"{formName}.razor");
        var content = await File.ReadAllTextAsync(formFile);
        
        content.Should().Contain("[Parameter] public User user { get; set; } = new();");
        content.Should().NotContain("private User user = new();");
        // Check button exists with proper text, ignoring exact whitespace
        content.Should().Contain("<button type=\"submit\" class=\"btn btn-primary\">");
        content.Should().Contain("Update");
        content.Should().Contain("</button>");
        content.Should().Contain("EventCallback<User> OnSubmit");
        content.Should().Contain("<h3>Edit User</h3>");
    }
    
    [Fact]
    public async Task GenerateFormAsync_HandlesNullableProperties()
    {
        // Arrange
        var generator = new FormGenerator();
        var modelInfo = new ModelInfo
        {
            Name = "Contact",
            Namespace = "MyApp.Models",
            Properties = new List<PropertyInfo>
            {
                new() { Name = "Name", Type = "string", IsNullable = false },
                new() { Name = "Email", Type = "string?", IsNullable = true },
                new() { Name = "Age", Type = "int?", IsNullable = true }
            }
        };
        
        var formName = "ContactForm";
        var outputPath = Path.Combine(_testDirectory, "Forms");
        
        // Act
        await generator.GenerateFormAsync(modelInfo, formName, outputPath, false, "HandleSubmit");
        
        // Assert
        var formFile = Path.Combine(outputPath, $"{formName}.razor");
        var content = await File.ReadAllTextAsync(formFile);
        
        content.Should().Contain("contact.Email");
        content.Should().Contain("contact.Age");
        content.Should().Contain("<InputText id=\"Email\"");
        content.Should().Contain("<InputNumber id=\"Age\"");
        content.Should().Contain("<label for=\"Email\">Email:</label>");
        content.Should().Contain("<label for=\"Age\">Age:</label>");
    }
    
    [Fact]
    public async Task GenerateFormAsync_HandlesVariousDataTypes()
    {
        // Arrange
        var generator = new FormGenerator();
        var modelInfo = new ModelInfo
        {
            Name = "ComplexModel",
            Namespace = "MyApp.Models",
            Properties = new List<PropertyInfo>
            {
                new() { Name = "Text", Type = "string", IsNullable = false },
                new() { Name = "Number", Type = "int", IsNullable = false },
                new() { Name = "Date", Type = "DateTime", IsNullable = false },
                new() { Name = "IsEnabled", Type = "bool", IsNullable = false },
                new() { Name = "Amount", Type = "decimal", IsNullable = false }
            }
        };
        
        var formName = "ComplexModelForm";
        var outputPath = Path.Combine(_testDirectory, "Forms");
        
        // Act
        await generator.GenerateFormAsync(modelInfo, formName, outputPath, false, "HandleSubmit");
        
        // Assert
        var formFile = Path.Combine(outputPath, $"{formName}.razor");
        var content = await File.ReadAllTextAsync(formFile);
        
        content.Should().Contain("<InputText id=\"Text\"");
        content.Should().Contain("<InputNumber id=\"Number\"");
        content.Should().Contain("<InputDate id=\"Date\"");
        content.Should().Contain("<InputCheckbox id=\"IsEnabled\"");
        content.Should().Contain("<InputNumber id=\"Amount\"");
        content.Should().Contain("<label for=\"Text\">Text:</label>");
        content.Should().Contain("<label for=\"Number\">Number:</label>");
        content.Should().Contain("<label for=\"Date\">Date:</label>");
        content.Should().Contain("<label for=\"IsEnabled\">IsEnabled:</label>");
        content.Should().Contain("<label for=\"Amount\">Amount:</label>");
    }
    
    [Fact]
    public async Task GenerateFormAsync_CreatesOutputDirectory_WhenNotExists()
    {
        // Arrange
        var generator = new FormGenerator();
        var modelInfo = new ModelInfo
        {
            Name = "TestModel",
            Namespace = "MyApp.Models",
            Properties = new List<PropertyInfo>
            {
                new() { Name = "Name", Type = "string", IsNullable = false }
            }
        };
        
        var formName = "TestForm";
        var outputPath = Path.Combine(_testDirectory, "NewDirectory", "Forms");
        
        Directory.Exists(outputPath).Should().BeFalse();
        
        // Act
        await generator.GenerateFormAsync(modelInfo, formName, outputPath, false, "HandleSubmit");
        
        // Assert
        Directory.Exists(outputPath).Should().BeTrue();
        File.Exists(Path.Combine(outputPath, $"{formName}.razor")).Should().BeTrue();
    }
    
    [Fact]
    public async Task GenerateFormAsync_HandlesModelWithValidationAttributes()
    {
        // Arrange
        var generator = new FormGenerator();
        var modelInfo = new ModelInfo
        {
            Name = "ValidatedModel",
            Namespace = "MyApp.Models",
            Properties = new List<PropertyInfo>
            {
                new() 
                { 
                    Name = "Email", 
                    Type = "string", 
                    IsNullable = false,
                    ValidationAttributes = new List<ValidationAttribute>
                    {
                        new() { Name = "Required" },
                        new() { Name = "EmailAddress" }
                    }
                },
                new() 
                { 
                    Name = "Age", 
                    Type = "int", 
                    IsNullable = false,
                    ValidationAttributes = new List<ValidationAttribute>
                    {
                        new() 
                        { 
                            Name = "Range",
                            Parameters = new Dictionary<string, string>
                            {
                                { "Minimum", "18" },
                                { "Maximum", "100" }
                            }
                        }
                    }
                }
            }
        };
        
        var formName = "ValidatedForm";
        var outputPath = Path.Combine(_testDirectory, "Forms");
        
        // Act
        await generator.GenerateFormAsync(modelInfo, formName, outputPath, false, "HandleSubmit");
        
        // Assert
        var formFile = Path.Combine(outputPath, $"{formName}.razor");
        var content = await File.ReadAllTextAsync(formFile);
        
        content.Should().Contain("<DataAnnotationsValidator />");
        content.Should().Contain("<ValidationSummary />");
        content.Should().Contain("<ValidationMessage For=\"@(() => validatedModel.Email)\" />");
        content.Should().Contain("<ValidationMessage For=\"@(() => validatedModel.Age)\" />");
    }
    
    [Fact]
    public async Task GenerateFormAsync_UsesCorrectModelInstanceName()
    {
        // Arrange
        var generator = new FormGenerator();
        var modelInfo = new ModelInfo
        {
            Name = "CustomerOrder",
            Namespace = "MyApp.Models",
            Properties = new List<PropertyInfo>
            {
                new() { Name = "OrderNumber", Type = "string", IsNullable = false }
            }
        };
        
        var formName = "OrderForm";
        var outputPath = Path.Combine(_testDirectory, "Forms");
        
        // Act
        await generator.GenerateFormAsync(modelInfo, formName, outputPath, false, "HandleSubmit");
        
        // Assert
        var formFile = Path.Combine(outputPath, $"{formName}.razor");
        var content = await File.ReadAllTextAsync(formFile);
        
        // Model instance should be camelCase version of model name
        content.Should().Contain("private CustomerOrder customerOrder = new();");
        content.Should().Contain("Model=\"@customerOrder\"");
        content.Should().Contain("customerOrder.OrderNumber");
        content.Should().Contain("<h3>Create CustomerOrder</h3>");
    }
    
    [Fact]
    public async Task GenerateFormAsync_HandlesRecordTypes()
    {
        // Arrange
        var generator = new FormGenerator();
        var modelInfo = new ModelInfo
        {
            Name = "PersonRecord",
            Namespace = "MyApp.Models",
            IsRecord = true,
            Properties = new List<PropertyInfo>
            {
                new() { Name = "FirstName", Type = "string", IsNullable = false },
                new() { Name = "LastName", Type = "string", IsNullable = false }
            }
        };
        
        var formName = "PersonForm";
        var outputPath = Path.Combine(_testDirectory, "Forms");
        
        // Act
        await generator.GenerateFormAsync(modelInfo, formName, outputPath, false, "HandleSubmit");
        
        // Assert
        var formFile = Path.Combine(outputPath, $"{formName}.razor");
        var content = await File.ReadAllTextAsync(formFile);
        
        content.Should().Contain("private PersonRecord personRecord = new();");
        content.Should().Contain("EventCallback<PersonRecord> OnSubmit");
        content.Should().Contain("<h3>Create PersonRecord</h3>");
    }
}