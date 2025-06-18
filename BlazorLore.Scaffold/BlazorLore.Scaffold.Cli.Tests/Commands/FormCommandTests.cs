using System.CommandLine;
using System.CommandLine.Parsing;
using BlazorLore.Scaffold.Cli.Commands;
using BlazorLore.Scaffold.Cli.Tests.Utilities;
using FluentAssertions;

namespace BlazorLore.Scaffold.Cli.Tests.Commands;

public class FormCommandTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _templateDirectory;
    public FormCommandTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"FormCommandTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        // Setup templates for FormGenerator
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(assemblyLocation) ?? ".";
        _templateDirectory = Path.Combine(directory, "Templates", "Form");
        Directory.CreateDirectory(_templateDirectory);
        CreateMockTemplate();
    }
    
    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
        
        var templatesRoot = Path.GetDirectoryName(_templateDirectory);
        if (templatesRoot != null && Directory.Exists(templatesRoot))
        {
            Directory.Delete(templatesRoot, true);
        }
    }
    
    private void CreateMockTemplate()
    {
        var formTemplate = @"<EditForm Model=""@{{ model_instance }}"" OnValidSubmit=""@{{ submit_action }}"">
    <h3>{{ form_name }}</h3>
    {{ for prop in model_info.properties }}
    <div class=""form-group"">
        <label>{{ prop.name }}:</label>
        <InputText @bind-Value=""{{ model_instance }}.{{ prop.name }}"" />
    </div>
    {{ end }}
    <button type=""submit"">{{ if is_edit_form }}Update{{ else }}Create{{ end }}</button>
</EditForm>

@code {
    {{ if is_edit_form }}
    [Parameter] public {{ model_info.name }} {{ model_instance }} { get; set; } = new();
    {{ else }}
    private {{ model_info.name }} {{ model_instance }} = new();
    {{ end }}
}";
        
        File.WriteAllText(Path.Combine(_templateDirectory, "Form.razor.scriban"), formTemplate);
    }
    
    [Fact]
    public async Task GenerateCommand_CreatesForm_FromSimpleModel()
    {
        // Arrange
        var formCommand = new FormCommand();
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(formCommand.GetCommand());
        
        var modelPath = Path.Combine(_testDirectory, "Product.cs");
        var modelContent = @"
namespace MyApp.Models
{
    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}";
        await File.WriteAllTextAsync(modelPath, modelContent);
        
        var outputPath = Path.Combine(_testDirectory, "Forms");
        var args = new[] { "form", "generate", modelPath, "--path", outputPath };
        
        // Act
        var result = await rootCommand.InvokeAsync(args);
        
        // Assert
        result.Should().Be(0);
        
        var formFile = Path.Combine(outputPath, "ProductForm.razor");
        File.Exists(formFile).Should().BeTrue();
        
        var formContent = await File.ReadAllTextAsync(formFile);
        formContent.Should().Contain("<h3>ProductForm</h3>");
        formContent.Should().Contain("private Product product = new();");
        formContent.Should().Contain("<button type=\"submit\">Create</button>");
    }
    
    [Fact]
    public async Task GenerateCommand_CreatesEditForm_WithCustomName()
    {
        // Arrange
        var formCommand = new FormCommand();
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(formCommand.GetCommand());
        
        var modelPath = Path.Combine(_testDirectory, "User.cs");
        var modelContent = @"
namespace MyApp.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Email { get; set; }
    }
}";
        await File.WriteAllTextAsync(modelPath, modelContent);
        
        var outputPath = Path.Combine(_testDirectory, "Forms");
        var args = new[] { "form", "generate", modelPath, "--name", "UserEditForm", "--path", outputPath, "--edit" };
        
        // Act
        var result = await rootCommand.InvokeAsync(args);
        
        // Assert
        result.Should().Be(0);
        
        var formFile = Path.Combine(outputPath, "UserEditForm.razor");
        var formContent = await File.ReadAllTextAsync(formFile);
        formContent.Should().Contain("<h3>UserEditForm</h3>");
        formContent.Should().Contain("[Parameter] public User user { get; set; } = new();");
        formContent.Should().Contain("<button type=\"submit\">Update</button>");
    }
    
    [Fact]
    public async Task GenerateCommand_DetectsValidationAttributes()
    {
        // Arrange
        var formCommand = new FormCommand();
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(formCommand.GetCommand());
        
        var modelPath = Path.Combine(_testDirectory, "ValidatedModel.cs");
        var modelContent = @"
using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
    public class ValidatedModel
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Range(18, 100)]
        public int Age { get; set; }
    }
}";
        await File.WriteAllTextAsync(modelPath, modelContent);
        
        var outputPath = Path.Combine(_testDirectory, "Forms");
        var args = new[] { "form", "generate", modelPath, "--path", outputPath };
        
        // Act
        var result = await rootCommand.InvokeAsync(args);
        
        // Assert
        result.Should().Be(0);
        
        var formFile = Path.Combine(outputPath, "ValidatedModelForm.razor");
        File.Exists(formFile).Should().BeTrue();
    }
    
    [Fact]
    public async Task GenerateCommand_UsesCustomSubmitAction()
    {
        // Arrange
        var formCommand = new FormCommand();
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(formCommand.GetCommand());
        
        var modelPath = Path.Combine(_testDirectory, "Order.cs");
        var modelContent = @"
namespace MyApp.Models
{
    public class Order
    {
        public string OrderNumber { get; set; }
        public decimal Total { get; set; }
    }
}";
        await File.WriteAllTextAsync(modelPath, modelContent);
        
        var outputPath = Path.Combine(_testDirectory, "Forms");
        var args = new[] { "form", "generate", modelPath, "--path", outputPath, "--submit-action", "HandleOrderSubmit" };
        
        // Act
        var result = await rootCommand.InvokeAsync(args);
        
        // Assert
        result.Should().Be(0);
        
        var formFile = Path.Combine(outputPath, "OrderForm.razor");
        var formContent = await File.ReadAllTextAsync(formFile);
        formContent.Should().Contain("OnValidSubmit=\"@HandleOrderSubmit\"");
    }
    
    [Fact]
    public async Task GenerateCommand_HandlesModelNotFound()
    {
        // Arrange
        var formCommand = new FormCommand();
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(formCommand.GetCommand());
        
        var nonExistentModel = Path.Combine(_testDirectory, "NonExistent.cs");
        var args = new[] { "form", "generate", nonExistentModel };
        
        // Act
        var result = await rootCommand.InvokeAsync(args);
        
        // Assert
        result.Should().NotBe(0); // Should fail for non-existent file
    }
    
    [Fact]
    public async Task GenerateCommand_HandlesInvalidModel()
    {
        // Arrange
        var formCommand = new FormCommand();
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(formCommand.GetCommand());
        
        var invalidModelPath = Path.Combine(_testDirectory, "InvalidModel.cs");
        var modelContent = @"
namespace MyApp.Models
{
    // No class declaration
}";
        await File.WriteAllTextAsync(invalidModelPath, modelContent);
        
        var args = new[] { "form", "generate", invalidModelPath };
        
        // Act
        var result = await rootCommand.InvokeAsync(args);
        
        // Assert
        result.Should().NotBe(0); // Should fail for invalid model
    }
    
    [Fact]
    public async Task GenerateCommand_WorksWithRecordTypes()
    {
        // Arrange
        var formCommand = new FormCommand();
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(formCommand.GetCommand());
        
        var modelPath = Path.Combine(_testDirectory, "PersonRecord.cs");
        var modelContent = @"
namespace MyApp.Models
{
    public record Person(string FirstName, string LastName, int Age);
}";
        await File.WriteAllTextAsync(modelPath, modelContent);
        
        var outputPath = Path.Combine(_testDirectory, "Forms");
        var args = new[] { "form", "generate", modelPath, "--path", outputPath };
        
        // Act
        var result = await rootCommand.InvokeAsync(args);
        
        // Assert
        result.Should().Be(0);
        
        var formFile = Path.Combine(outputPath, "PersonForm.razor");
        File.Exists(formFile).Should().BeTrue();
    }
    
    [Fact]
    public void FormCommand_HasCorrectMetadata()
    {
        // Arrange
        var formCommand = new FormCommand();
        
        // Assert
        formCommand.EntityName.Should().Be("form");
        formCommand.Description.Should().Be("Generate forms from models");
    }
    
    [Fact]
    public void GetCommand_ReturnsValidCommandStructure()
    {
        // Arrange
        var formCommand = new FormCommand();
        
        // Act
        var command = formCommand.GetCommand();
        
        // Assert
        command.Name.Should().Be("form");
        command.Description.Should().Be("Generate forms from models");
        command.Subcommands.Should().HaveCount(1);
        command.Subcommands.First().Name.Should().Be("generate");
        
        var generateCommand = command.Subcommands.First();
        generateCommand.Arguments.Should().HaveCount(1);
        generateCommand.Options.Should().HaveCount(4);
        
        var optionNames = generateCommand.Options.Select(o => o.Name).ToList();
        optionNames.Should().Contain("name");
        optionNames.Should().Contain("path");
        optionNames.Should().Contain("edit");
        optionNames.Should().Contain("submit-action");
    }
}