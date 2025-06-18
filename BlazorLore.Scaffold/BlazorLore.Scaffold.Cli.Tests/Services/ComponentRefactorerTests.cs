using BlazorLore.Scaffold.Cli.Services;
using FluentAssertions;

namespace BlazorLore.Scaffold.Cli.Tests.Services;

public class ComponentRefactorerTests : IDisposable
{
    private readonly string _testDirectory;
    
    public ComponentRefactorerTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ComponentRefactorerTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }
    
    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
    
    [Fact]
    public async Task ExtractCodeBehindAsync_ThrowsFileNotFoundException_WhenComponentNotFound()
    {
        // Arrange
        var refactorer = new ComponentRefactorer();
        var nonExistentPath = Path.Combine(_testDirectory, "NonExistent.razor");
        
        // Act & Assert
        await refactorer.Invoking(r => r.ExtractCodeBehindAsync(nonExistentPath))
            .Should().ThrowAsync<FileNotFoundException>()
            .WithMessage($"Component file not found: {nonExistentPath}");
    }
    
    [Fact]
    public async Task ExtractCodeBehindAsync_ExtractsSimpleCodeBlock()
    {
        // Arrange
        var refactorer = new ComponentRefactorer();
        var componentPath = Path.Combine(_testDirectory, "SimpleComponent.razor");
        var content = @"<h3>Simple Component</h3>

@code {
    private string message = ""Hello"";
    
    protected override void OnInitialized()
    {
        message = ""Initialized"";
    }
}";
        await File.WriteAllTextAsync(componentPath, content);
        
        // Act
        await refactorer.ExtractCodeBehindAsync(componentPath);
        
        // Assert
        var updatedContent = await File.ReadAllTextAsync(componentPath);
        updatedContent.Should().Contain("@inherits SimpleComponentBase");
        updatedContent.Should().NotContain("@code");
        updatedContent.Should().Contain("<h3>Simple Component</h3>");
        
        var codeBehindPath = Path.Combine(_testDirectory, "SimpleComponent.razor.cs");
        File.Exists(codeBehindPath).Should().BeTrue();
        
        var codeBehindContent = await File.ReadAllTextAsync(codeBehindPath);
        codeBehindContent.Should().Contain("public partial class SimpleComponentBase : ComponentBase");
        codeBehindContent.Should().Contain("private string message = \"Hello\";");
        codeBehindContent.Should().Contain("protected override void OnInitialized()");
    }
    
    [Fact]
    public async Task ExtractCodeBehindAsync_ExtractsInjectDirectives()
    {
        // Arrange
        var refactorer = new ComponentRefactorer();
        var componentPath = Path.Combine(_testDirectory, "ServiceComponent.razor");
        var content = @"@inject ILogger<ServiceComponent> Logger
@inject NavigationManager Navigation

<h3>Service Component</h3>

@code {
    protected override void OnInitialized()
    {
        Logger.LogInformation(""Component initialized"");
    }
}";
        await File.WriteAllTextAsync(componentPath, content);
        
        // Act
        await refactorer.ExtractCodeBehindAsync(componentPath);
        
        // Assert
        var updatedContent = await File.ReadAllTextAsync(componentPath);
        updatedContent.Should().NotContain("@inject");
        updatedContent.Should().Contain("@inherits ServiceComponentBase");
        
        var codeBehindPath = Path.Combine(_testDirectory, "ServiceComponent.razor.cs");
        var codeBehindContent = await File.ReadAllTextAsync(codeBehindPath);
        codeBehindContent.Should().Contain("[Inject]");
        codeBehindContent.Should().Contain("public ILogger<ServiceComponent> Logger { get; set; } = default!");
        codeBehindContent.Should().Contain("public NavigationManager Navigation { get; set; } = default!");
    }
    
    [Fact]
    public async Task ExtractCodeBehindAsync_HandlesComponentWithoutCodeBlock()
    {
        // Arrange
        var refactorer = new ComponentRefactorer();
        var componentPath = Path.Combine(_testDirectory, "NoCodeComponent.razor");
        var content = @"<h3>No Code Component</h3>
<p>This component has no code block.</p>";
        await File.WriteAllTextAsync(componentPath, content);
        
        // Act
        await refactorer.ExtractCodeBehindAsync(componentPath);
        
        // Assert
        var updatedContent = await File.ReadAllTextAsync(componentPath);
        updatedContent.Should().NotContain("@inherits");
        updatedContent.Should().Be(content);
        
        var codeBehindPath = Path.Combine(_testDirectory, "NoCodeComponent.razor.cs");
        File.Exists(codeBehindPath).Should().BeFalse();
    }
    
    [Fact]
    public async Task ExtractCodeBehindAsync_HandlesComplexCodeBlock()
    {
        // Arrange
        var refactorer = new ComponentRefactorer();
        var componentPath = Path.Combine(_testDirectory, "ComplexComponent.razor");
        var content = @"@inject IDataService DataService

<h3>Complex Component</h3>

@code {
    private List<Item> items = new();
    public string Title { get; set; } = ""Default"";
    
    protected override async Task OnInitializedAsync()
    {
        items = await DataService.GetItemsAsync();
        
        if (items.Any())
        {
            var firstItem = items.First();
            ProcessItem(firstItem);
        }
    }
    
    private void ProcessItem(Item item)
    {
        // Complex logic with nested braces
        if (item != null)
        {
            item.Process(() => 
            {
                Console.WriteLine(""Processing"");
            });
        }
    }
}";
        await File.WriteAllTextAsync(componentPath, content);
        
        // Act
        await refactorer.ExtractCodeBehindAsync(componentPath);
        
        // Assert
        var codeBehindPath = Path.Combine(_testDirectory, "ComplexComponent.razor.cs");
        var codeBehindContent = await File.ReadAllTextAsync(codeBehindPath);
        codeBehindContent.Should().Contain("private List<Item> items = new();");
        codeBehindContent.Should().Contain("public string Title { get; set; } = \"Default\";");
        codeBehindContent.Should().Contain("private void ProcessItem(Item item)");
        codeBehindContent.Should().Contain("Console.WriteLine(\"Processing\");");
    }
    
    [Fact]
    public async Task ConvertToConstructorInjectionAsync_ThrowsFileNotFoundException_WhenFileNotFound()
    {
        // Arrange
        var refactorer = new ComponentRefactorer();
        var nonExistentPath = Path.Combine(_testDirectory, "NonExistent.razor.cs");
        
        // Act & Assert
        await refactorer.Invoking(r => r.ConvertToConstructorInjectionAsync(nonExistentPath))
            .Should().ThrowAsync<FileNotFoundException>()
            .WithMessage($"Code-behind file not found: {nonExistentPath}");
    }
    
    [Fact]
    public async Task ConvertToConstructorInjectionAsync_ConvertsInjectProperties()
    {
        // Arrange
        var refactorer = new ComponentRefactorer();
        var codeBehindPath = Path.Combine(_testDirectory, "TestComponent.razor.cs");
        var content = @"using Microsoft.AspNetCore.Components;

namespace MyApp.Components;

public partial class TestComponentBase : ComponentBase
{
    [Inject]
    public ILogger<TestComponent> Logger { get; set; } = default!;
    
    [Inject]
    public NavigationManager Navigation { get; set; } = default!;
    
    protected override void OnInitialized()
    {
        Logger.LogInformation(""Component initialized"");
    }
}";
        await File.WriteAllTextAsync(codeBehindPath, content);
        
        // Act
        await refactorer.ConvertToConstructorInjectionAsync(codeBehindPath);
        
        // Assert
        var modernizedContent = await File.ReadAllTextAsync(codeBehindPath);
        modernizedContent.Should().Contain("public partial class TestComponentBase(ILogger<TestComponent> logger, NavigationManager navigation) : ComponentBase");
        modernizedContent.Should().Contain("private readonly ILogger<TestComponent> Logger = logger;");
        modernizedContent.Should().Contain("private readonly NavigationManager Navigation = navigation;");
        modernizedContent.Should().NotContain("[Inject]");
        modernizedContent.Should().Contain("Logger.LogInformation(\"Component initialized\");");
    }
    
    [Fact]
    public async Task ConvertToConstructorInjectionAsync_HandlesNoInjectProperties()
    {
        // Arrange
        var refactorer = new ComponentRefactorer();
        var codeBehindPath = Path.Combine(_testDirectory, "NoInjectComponent.razor.cs");
        var content = @"using Microsoft.AspNetCore.Components;

namespace MyApp.Components;

public partial class NoInjectComponentBase : ComponentBase
{
    private string message = ""Hello"";
    
    protected override void OnInitialized()
    {
        message = ""Initialized"";
    }
}";
        await File.WriteAllTextAsync(codeBehindPath, content);
        
        // Act
        await refactorer.ConvertToConstructorInjectionAsync(codeBehindPath);
        
        // Assert
        var modernizedContent = await File.ReadAllTextAsync(codeBehindPath);
        modernizedContent.Should().Contain("public partial class NoInjectComponentBase : ComponentBase");
        modernizedContent.Should().NotContain("(");
        modernizedContent.Should().Contain("message = \"Initialized\";");
    }
    
    [Fact]
    public async Task ConvertToConstructorInjectionAsync_PreservesOtherPropertiesAndMethods()
    {
        // Arrange
        var refactorer = new ComponentRefactorer();
        var codeBehindPath = Path.Combine(_testDirectory, "MixedComponent.razor.cs");
        var content = @"using Microsoft.AspNetCore.Components;

namespace MyApp.Components;

public partial class MixedComponentBase : ComponentBase
{
    [Inject]
    public IDataService DataService { get; set; } = default!;
    
    public string Title { get; set; } = ""Default"";
    
    private List<Item> items = new();
    
    protected override async Task OnInitializedAsync()
    {
        items = await DataService.GetItemsAsync();
    }
    
    public void UpdateTitle(string newTitle)
    {
        Title = newTitle;
        StateHasChanged();
    }
}";
        await File.WriteAllTextAsync(codeBehindPath, content);
        
        // Act
        await refactorer.ConvertToConstructorInjectionAsync(codeBehindPath);
        
        // Assert
        var modernizedContent = await File.ReadAllTextAsync(codeBehindPath);
        modernizedContent.Should().Contain("public partial class MixedComponentBase(IDataService dataService) : ComponentBase");
        modernizedContent.Should().Contain("private readonly IDataService DataService = dataService;");
        modernizedContent.Should().Contain("public string Title { get; set; } = \"Default\";");
        modernizedContent.Should().Contain("public void UpdateTitle(string newTitle)");
        modernizedContent.Should().Contain("protected override async Task OnInitializedAsync()");
    }
    
    [Fact]
    public async Task ConvertToConstructorInjectionAsync_ThrowsException_WhenNoClassFound()
    {
        // Arrange
        var refactorer = new ComponentRefactorer();
        var codeBehindPath = Path.Combine(_testDirectory, "InvalidFile.cs");
        var content = @"// This file has no class declaration
namespace MyApp.Components;";
        await File.WriteAllTextAsync(codeBehindPath, content);
        
        // Act & Assert
        await refactorer.Invoking(r => r.ConvertToConstructorInjectionAsync(codeBehindPath))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Could not find class declaration.");
    }
}