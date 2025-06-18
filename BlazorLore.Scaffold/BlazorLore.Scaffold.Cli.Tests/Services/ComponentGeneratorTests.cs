using BlazorLore.Scaffold.Cli.Services;
using FluentAssertions;

namespace BlazorLore.Scaffold.Cli.Tests.Services;

public class ComponentGeneratorTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _templateDirectory;
    
    public ComponentGeneratorTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ComponentGeneratorTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        // Create a mock template directory structure
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(assemblyLocation) ?? ".";
        _templateDirectory = Path.Combine(directory, "Templates", "Component");
        Directory.CreateDirectory(_templateDirectory);
        
        // Create mock templates
        CreateMockTemplates();
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
    
    private void CreateMockTemplates()
    {
        // Create mock templates if the source templates are not available
        var razorTemplate = Path.Combine(_templateDirectory, "Component.razor.scriban");
        if (!File.Exists(razorTemplate))
        {
            var razorContent = @"{{ if has_code_behind }}@inherits {{ name }}Base{{ else }}@inject ILogger<{{ name }}> Logger
@inject NavigationManager Navigation{{ end }}

<div class=""{{ name | string.downcase }}"">
    <h3>{{ name }}</h3>
    
    <p>This is the {{ name }} component.</p>
    
    <button class=""btn btn-primary"" @onclick=""HandleClick"">
        Click me
    </button>
    
    <p>Counter: @_counter</p>
</div>

{{ if !has_code_behind }}
@code {
    private int _counter = 0;

    protected override void OnInitialized()
    {
        Logger.LogInformation($""{{ name }} initialized"");
    }

    private void HandleClick()
    {
        _counter++;
        Logger.LogInformation($""Button clicked. Counter: {_counter}"");
    }
}{{ end }}";
            File.WriteAllText(razorTemplate, razorContent);
        }
        
        var codeTemplate = Path.Combine(_templateDirectory, "Component.razor.cs.scriban");
        if (!File.Exists(codeTemplate))
        {
            var codeContent = @"using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace {{ namespace }};

public partial class {{ name }}Base : ComponentBase
{
    [Inject]
    public ILogger<{{ name }}> Logger { get; set; } = default!;
    
    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    private int _counter = 0;

    protected override void OnInitialized()
    {
        Logger.LogInformation($""{{ name }} initialized"");
    }

    private void HandleClick()
    {
        _counter++;
        Logger.LogInformation($""Button clicked. Counter: {_counter}"");
    }
}";
            File.WriteAllText(codeTemplate, codeContent);
        }
        
        var cssTemplate = Path.Combine(_templateDirectory, "Component.razor.css.scriban");
        if (!File.Exists(cssTemplate))
        {
            var cssContent = @".{{ name | string.downcase }} {
    padding: 1rem;
    border: 1px solid #dee2e6;
    border-radius: 0.25rem;
    background-color: #f8f9fa;
}

.{{ name | string.downcase }} h3 {
    color: #495057;
    margin-bottom: 1rem;
}

.{{ name | string.downcase }} button {
    margin: 0.5rem 0;
}

.{{ name | string.downcase }} p {
    margin: 0.5rem 0;
    color: #6c757d;
}";
            File.WriteAllText(cssTemplate, cssContent);
        }
    }
    
    [Fact]
    public async Task GenerateComponentAsync_CreatesBasicComponent()
    {
        // Arrange
        var generator = new ComponentGenerator();
        var componentName = "TestComponent";
        var outputPath = Path.Combine(_testDirectory, "Components");
        
        // Act
        await generator.GenerateComponentAsync(componentName, outputPath, false, false);
        
        // Assert
        Directory.Exists(outputPath).Should().BeTrue();
        
        var razorFile = Path.Combine(outputPath, $"{componentName}.razor");
        File.Exists(razorFile).Should().BeTrue();
        
        var content = await File.ReadAllTextAsync(razorFile);
        content.Should().Contain($"<div class=\"{componentName.ToLower()}\">");
        content.Should().Contain($"<h3>{componentName}</h3>");
        content.Should().Contain("@code {");
        content.Should().Contain("@inject ILogger<TestComponent> Logger");
        content.Should().Contain("@inject NavigationManager Navigation");
        content.Should().NotContain("@inherits");
        content.Should().Contain("private int _counter = 0;");
        content.Should().Contain("protected override void OnInitialized()");
        content.Should().Contain("private void HandleClick()");
    }
    
    [Fact]
    public async Task GenerateComponentAsync_CreatesComponentWithCodeBehind()
    {
        // Arrange
        var generator = new ComponentGenerator();
        var componentName = "UserProfile";
        var outputPath = Path.Combine(_testDirectory, "Components");
        
        // Act
        await generator.GenerateComponentAsync(componentName, outputPath, true, false);
        
        // Assert
        // Check razor file
        var razorFile = Path.Combine(outputPath, $"{componentName}.razor");
        File.Exists(razorFile).Should().BeTrue();
        
        var razorContent = await File.ReadAllTextAsync(razorFile);
        razorContent.Should().Contain("@inherits UserProfileBase");
        razorContent.Should().NotContain("@code {");
        razorContent.Should().NotContain("@inject ILogger");
        razorContent.Should().NotContain("@inject NavigationManager");
        razorContent.Should().Contain($"<div class=\"{componentName.ToLower()}\">");
        razorContent.Should().Contain($"<h3>{componentName}</h3>");
        
        // Check code-behind file
        var codeBehindFile = Path.Combine(outputPath, $"{componentName}.razor.cs");
        File.Exists(codeBehindFile).Should().BeTrue();
        
        var codeBehindContent = await File.ReadAllTextAsync(codeBehindFile);
        codeBehindContent.Should().Contain("namespace MyApp.Components;");
        codeBehindContent.Should().Contain($"public partial class {componentName}Base : ComponentBase");
        codeBehindContent.Should().Contain("[Inject]");
        codeBehindContent.Should().Contain("public ILogger<UserProfile> Logger { get; set; } = default!;");
        codeBehindContent.Should().Contain("public NavigationManager Navigation { get; set; } = default!;");
        codeBehindContent.Should().Contain("private int _counter = 0;");
        codeBehindContent.Should().Contain("protected override void OnInitialized()");
        codeBehindContent.Should().Contain("private void HandleClick()");
    }
    
    [Fact]
    public async Task GenerateComponentAsync_CreatesComponentWithCss()
    {
        // Arrange
        var generator = new ComponentGenerator();
        var componentName = "StyledButton";
        var outputPath = Path.Combine(_testDirectory, "Components");
        
        // Act
        await generator.GenerateComponentAsync(componentName, outputPath, false, true);
        
        // Assert
        // Check CSS file
        var cssFile = Path.Combine(outputPath, $"{componentName}.razor.css");
        File.Exists(cssFile).Should().BeTrue();
        
        var cssContent = await File.ReadAllTextAsync(cssFile);
        cssContent.Should().Contain(".styledbutton");
        cssContent.Should().Contain("padding: 1rem;");
        cssContent.Should().Contain(".styledbutton h3");
        cssContent.Should().Contain("color: #495057;");
        cssContent.Should().Contain("margin-bottom: 1rem;");
    }
    
    [Fact]
    public async Task GenerateComponentAsync_CreatesFullComponent_WithCodeBehindAndCss()
    {
        // Arrange
        var generator = new ComponentGenerator();
        var componentName = "FullFeatureComponent";
        var outputPath = Path.Combine(_testDirectory, "Components");
        
        // Act
        await generator.GenerateComponentAsync(componentName, outputPath, true, true);
        
        // Assert
        var razorFile = Path.Combine(outputPath, $"{componentName}.razor");
        var codeBehindFile = Path.Combine(outputPath, $"{componentName}.razor.cs");
        var cssFile = Path.Combine(outputPath, $"{componentName}.razor.css");
        
        File.Exists(razorFile).Should().BeTrue();
        File.Exists(codeBehindFile).Should().BeTrue();
        File.Exists(cssFile).Should().BeTrue();
    }
    
    [Fact]
    public async Task GenerateComponentAsync_CreatesOutputDirectory_WhenNotExists()
    {
        // Arrange
        var generator = new ComponentGenerator();
        var componentName = "TestComponent";
        var outputPath = Path.Combine(_testDirectory, "NewDirectory", "Components");
        
        Directory.Exists(outputPath).Should().BeFalse();
        
        // Act
        await generator.GenerateComponentAsync(componentName, outputPath, false, false);
        
        // Assert
        Directory.Exists(outputPath).Should().BeTrue();
        File.Exists(Path.Combine(outputPath, $"{componentName}.razor")).Should().BeTrue();
    }
    
    [Fact]
    public async Task GenerateComponentAsync_HandlesSpecialCharactersInName()
    {
        // Arrange
        var generator = new ComponentGenerator();
        var componentName = "User-Profile_Component123";
        var outputPath = Path.Combine(_testDirectory, "Components");
        
        // Act
        await generator.GenerateComponentAsync(componentName, outputPath, false, false);
        
        // Assert
        var razorFile = Path.Combine(outputPath, $"{componentName}.razor");
        File.Exists(razorFile).Should().BeTrue();
        
        var content = await File.ReadAllTextAsync(razorFile);
        content.Should().Contain($"<div class=\"user-profile_component123\">");
        content.Should().Contain($"<h3>{componentName}</h3>");
    }
    
    [Fact]
    public async Task GenerateComponentAsync_OverwritesExistingFiles()
    {
        // Arrange
        var generator = new ComponentGenerator();
        var componentName = "ExistingComponent";
        var outputPath = Path.Combine(_testDirectory, "Components");
        Directory.CreateDirectory(outputPath);
        
        // Create existing file with different content
        var existingFile = Path.Combine(outputPath, $"{componentName}.razor");
        await File.WriteAllTextAsync(existingFile, "OLD CONTENT");
        
        // Act
        await generator.GenerateComponentAsync(componentName, outputPath, false, false);
        
        // Assert
        var content = await File.ReadAllTextAsync(existingFile);
        content.Should().NotBe("OLD CONTENT");
        content.Should().Contain($"<div class=\"{componentName.ToLower()}\">");
        content.Should().Contain($"<h3>{componentName}</h3>");
        content.Should().Contain("@inject ILogger<ExistingComponent> Logger");
    }
}