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
        // Component.razor.scriban
        File.WriteAllText(Path.Combine(_templateDirectory, "Component.razor.scriban"), 
@"@page ""/{{ name | string.downcase }}""
{{ if has_code_behind }}
@inherits {{ name }}Base
{{ end }}

<div class=""{{ name | string.downcase }}-container"">
    <h3>{{ name }}</h3>
    <p>This is the {{ name }} component.</p>
</div>

{{ if !has_code_behind }}
@code {
    protected override void OnInitialized()
    {
        base.OnInitialized();
    }
}
{{ end }}");

        // Component.razor.cs.scriban
        File.WriteAllText(Path.Combine(_templateDirectory, "Component.razor.cs.scriban"), 
@"using Microsoft.AspNetCore.Components;

namespace {{ namespace }};

public partial class {{ name }}Base : ComponentBase
{
    protected override void OnInitialized()
    {
        base.OnInitialized();
    }
}");

        // Component.razor.css.scriban
        File.WriteAllText(Path.Combine(_templateDirectory, "Component.razor.css.scriban"), 
@".{{ name | string.downcase }}-container {
    padding: 1rem;
    border: 1px solid #ccc;
    border-radius: 4px;
}

.{{ name | string.downcase }}-container h3 {
    margin-top: 0;
    color: #333;
}");
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
        content.Should().Contain($"@page \"/testcomponent\"");
        content.Should().Contain($"<h3>{componentName}</h3>");
        content.Should().Contain("@code {");
        content.Should().NotContain("@inherits");
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
        
        // Check code-behind file
        var codeBehindFile = Path.Combine(outputPath, $"{componentName}.razor.cs");
        File.Exists(codeBehindFile).Should().BeTrue();
        
        var codeBehindContent = await File.ReadAllTextAsync(codeBehindFile);
        codeBehindContent.Should().Contain("namespace MyApp.Components;");
        codeBehindContent.Should().Contain($"public partial class {componentName}Base : ComponentBase");
        codeBehindContent.Should().Contain("protected override void OnInitialized()");
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
        cssContent.Should().Contain(".styledbutton-container");
        cssContent.Should().Contain("padding: 1rem;");
        cssContent.Should().Contain(".styledbutton-container h3");
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
        content.Should().Contain($"<h3>{componentName}</h3>");
    }
}