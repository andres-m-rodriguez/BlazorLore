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
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
        
        // Clean up templates directory
        var templatesRoot = Path.GetDirectoryName(_templateDirectory);
        if (templatesRoot != null && Directory.Exists(templatesRoot))
        {
            Directory.Delete(templatesRoot, true);
        }
    }
    
    private void CreateMockTemplate()
    {
        // Form.razor.scriban
        var formTemplate = @"@using {{ model_info.namespace }}

<EditForm Model=""@{{ model_instance }}"" OnValidSubmit=""@{{ submit_action }}"">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    <h3>{{ form_name }}</h3>
    
    {{ for prop in model_info.properties }}
    <div class=""form-group"">
        <label for=""{{ prop.name }}"">{{ prop.name }}:</label>
        {{ if prop.type == ""string"" || prop.type == ""string?"" }}
        <InputText id=""{{ prop.name }}"" @bind-Value=""{{ model_instance }}.{{ prop.name }}"" class=""form-control"" />
        {{ else if prop.type == ""int"" || prop.type == ""int?"" }}
        <InputNumber id=""{{ prop.name }}"" @bind-Value=""{{ model_instance }}.{{ prop.name }}"" class=""form-control"" />
        {{ else if prop.type == ""DateTime"" || prop.type == ""DateTime?"" }}
        <InputDate id=""{{ prop.name }}"" @bind-Value=""{{ model_instance }}.{{ prop.name }}"" class=""form-control"" />
        {{ else if prop.type == ""bool"" || prop.type == ""bool?"" }}
        <InputCheckbox id=""{{ prop.name }}"" @bind-Value=""{{ model_instance }}.{{ prop.name }}"" class=""form-check-input"" />
        {{ else }}
        <InputText id=""{{ prop.name }}"" @bind-Value=""{{ model_instance }}.{{ prop.name }}"" class=""form-control"" />
        {{ end }}
        <ValidationMessage For=""@(() => {{ model_instance }}.{{ prop.name }})"" />
    </div>
    {{ end }}
    
    <button type=""submit"" class=""btn btn-primary"">{{ if is_edit_form }}Update{{ else }}Create{{ end }}</button>
</EditForm>

@code {
    {{ if is_edit_form }}
    [Parameter] public {{ model_info.name }} {{ model_instance }} { get; set; } = new();
    {{ else }}
    private {{ model_info.name }} {{ model_instance }} = new();
    {{ end }}
    
    [Parameter] public EventCallback<{{ model_info.name }}> {{ submit_action }} { get; set; }
}";
        
        File.WriteAllText(Path.Combine(_templateDirectory, "Form.razor.scriban"), formTemplate);
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
        content.Should().Contain("@using MyApp.Models");
        content.Should().Contain("<h3>ProductCreateForm</h3>");
        content.Should().Contain("private Product product = new();");
        content.Should().Contain("<button type=\"submit\" class=\"btn btn-primary\">Create</button>");
        content.Should().Contain("EventCallback<Product> HandleSubmit");
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
        content.Should().Contain("<button type=\"submit\" class=\"btn btn-primary\">Update</button>");
        content.Should().Contain("EventCallback<User> HandleUpdate");
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
        content.Should().Contain("InputText");
        content.Should().Contain("InputNumber");
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
        
        content.Should().Contain("InputText");
        content.Should().Contain("InputNumber");
        content.Should().Contain("InputDate");
        content.Should().Contain("InputCheckbox");
        content.Should().Contain("<label for=\"Text\">Text:</label>");
        content.Should().Contain("<label for=\"Number\">Number:</label>");
        content.Should().Contain("<label for=\"Date\">Date:</label>");
        content.Should().Contain("<label for=\"IsEnabled\">IsEnabled:</label>");
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
        content.Should().Contain("ValidationMessage For=\"@(() => validatedModel.Email)\"");
        content.Should().Contain("ValidationMessage For=\"@(() => validatedModel.Age)\"");
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
        content.Should().Contain("EventCallback<PersonRecord> HandleSubmit");
    }
}