using System.CommandLine;
using System.CommandLine.Parsing;
using BlazorLore.Scaffold.Cli.Commands;
using BlazorLore.Scaffold.Cli.Tests.Utilities;
using FluentAssertions;

namespace BlazorLore.Scaffold.Cli.Tests.Commands;

public class ComponentCommandTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _templateDirectory;
    public ComponentCommandTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ComponentCommandTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        // Templates are now copied from the main project via csproj configuration
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(assemblyLocation) ?? ".";
        _templateDirectory = Path.Combine(directory, "Templates", "Component");
    }
    
    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
    
    [Fact]
    public async Task GenerateCommand_CreatesComponent_WithDefaultOptions()
    {
        // Arrange
        var componentCommand = new ComponentCommand();
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(componentCommand.GetCommand());
        
        var outputPath = Path.Combine(_testDirectory, "Components");
        var args = new[] { "component", "generate", "TestComponent", "--path", outputPath };
        
        // Act
        var result = await rootCommand.InvokeAsync(args);
        
        // Assert
        result.Should().Be(0);
        
        var componentFile = Path.Combine(outputPath, "TestComponent.razor");
        File.Exists(componentFile).Should().BeTrue();
        File.Exists(Path.Combine(outputPath, "TestComponent.razor.cs")).Should().BeFalse();
        File.Exists(Path.Combine(outputPath, "TestComponent.razor.css")).Should().BeFalse();
    }
    
    [Fact]
    public async Task GenerateCommand_CreatesComponent_WithAllOptions()
    {
        // Arrange
        var componentCommand = new ComponentCommand();
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(componentCommand.GetCommand());
        
        var outputPath = Path.Combine(_testDirectory, "Components");
        var args = new[] { "component", "generate", "FullComponent", "--path", outputPath, "--code-behind", "--css" };
        
        // Act
        var result = await rootCommand.InvokeAsync(args);
        
        // Assert
        result.Should().Be(0);
        
        File.Exists(Path.Combine(outputPath, "FullComponent.razor")).Should().BeTrue();
        File.Exists(Path.Combine(outputPath, "FullComponent.razor.cs")).Should().BeTrue();
        File.Exists(Path.Combine(outputPath, "FullComponent.razor.css")).Should().BeTrue();
    }
    
    [Fact]
    public async Task RefactorCommand_ExtractsCodeBehind()
    {
        // Arrange
        var componentCommand = new ComponentCommand();
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(componentCommand.GetCommand());
        
        var componentPath = Path.Combine(_testDirectory, "RefactorTest.razor");
        var componentContent = @"<h3>Test</h3>

@code {
    private string message = ""Hello"";
}";
        await File.WriteAllTextAsync(componentPath, componentContent);
        
        var args = new[] { "component", "refactor", componentPath, "--extract-code" };
        
        // Act
        var result = await rootCommand.InvokeAsync(args);
        
        // Assert
        result.Should().Be(0);
        
        var updatedContent = await File.ReadAllTextAsync(componentPath);
        updatedContent.Should().Contain("@inherits RefactorTestBase");
        updatedContent.Should().NotContain("@code");
        
        var codeBehindPath = Path.Combine(_testDirectory, "RefactorTest.razor.cs");
        File.Exists(codeBehindPath).Should().BeTrue();
    }
    
    [Fact]
    public async Task RefactorCommand_ShowsMessage_WhenNoOptionProvided()
    {
        // Arrange
        var componentCommand = new ComponentCommand();
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(componentCommand.GetCommand());
        
        var componentPath = Path.Combine(_testDirectory, "Test.razor");
        await File.WriteAllTextAsync(componentPath, "<h3>Test</h3>");
        
        var args = new[] { "component", "refactor", componentPath };
        
        // Act
        var result = await rootCommand.InvokeAsync(args);
        
        // Assert
        result.Should().Be(0);
    }
    
    [Fact]
    public async Task ListCommand_ListsAllRazorFiles()
    {
        // Arrange
        var componentCommand = new ComponentCommand();
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(componentCommand.GetCommand());
        
        // Create test structure
        var componentsDir = Path.Combine(_testDirectory, "Components");
        var sharedDir = Path.Combine(componentsDir, "Shared");
        Directory.CreateDirectory(sharedDir);
        
        await File.WriteAllTextAsync(Path.Combine(componentsDir, "App.razor"), "<h3>App</h3>");
        await File.WriteAllTextAsync(Path.Combine(componentsDir, "Index.razor"), "<h3>Index</h3>");
        await File.WriteAllTextAsync(Path.Combine(sharedDir, "Header.razor"), "<h3>Header</h3>");
        await File.WriteAllTextAsync(Path.Combine(componentsDir, "NotAComponent.cs"), "// C# file");
        
        var args = new[] { "component", "list", "--dir", componentsDir };
        
        // Act
        var result = await rootCommand.InvokeAsync(args);
        
        // Assert
        result.Should().Be(0);
        // Verify files were created in the expected structure
        File.Exists(Path.Combine(componentsDir, "App.razor")).Should().BeTrue();
        File.Exists(Path.Combine(componentsDir, "Index.razor")).Should().BeTrue();
        File.Exists(Path.Combine(sharedDir, "Header.razor")).Should().BeTrue();
    }
    
    [Fact]
    public async Task ModernizeCommand_ConvertsToConstructorInjection()
    {
        // Arrange
        var componentCommand = new ComponentCommand();
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(componentCommand.GetCommand());
        
        var codeBehindPath = Path.Combine(_testDirectory, "OldComponent.razor.cs");
        var content = @"using Microsoft.AspNetCore.Components;

namespace MyApp.Components;

public partial class OldComponentBase : ComponentBase
{
    [Inject]
    public ILogger<OldComponent> Logger { get; set; } = default!;
    
    protected override void OnInitialized()
    {
        Logger.LogInformation(""Initialized"");
    }
}";
        await File.WriteAllTextAsync(codeBehindPath, content);
        
        var args = new[] { "component", "modernize", codeBehindPath };
        
        // Act
        var result = await rootCommand.InvokeAsync(args);
        
        // Assert
        result.Should().Be(0);
        
        var modernizedContent = await File.ReadAllTextAsync(codeBehindPath);
        modernizedContent.Should().Contain("public partial class OldComponentBase(ILogger<OldComponent> logger) : ComponentBase");
        modernizedContent.Should().Contain("private readonly ILogger<OldComponent> Logger = logger;");
        modernizedContent.Should().NotContain("[Inject]");
    }
    
    [Fact]
    public void ComponentCommand_HasCorrectMetadata()
    {
        // Arrange
        var componentCommand = new ComponentCommand();
        
        // Assert
        componentCommand.EntityName.Should().Be("component");
        componentCommand.Description.Should().Be("Generate and manage Blazor components");
    }
    
    [Fact]
    public void GetCommand_ReturnsValidCommandStructure()
    {
        // Arrange
        var componentCommand = new ComponentCommand();
        
        // Act
        var command = componentCommand.GetCommand();
        
        // Assert
        command.Name.Should().Be("component");
        command.Description.Should().Be("Generate and manage Blazor components");
        command.Subcommands.Should().HaveCount(4);
        
        var subcommandNames = command.Subcommands.Select(c => c.Name).ToList();
        subcommandNames.Should().Contain("generate");
        subcommandNames.Should().Contain("refactor");
        subcommandNames.Should().Contain("list");
        subcommandNames.Should().Contain("modernize");
    }
}