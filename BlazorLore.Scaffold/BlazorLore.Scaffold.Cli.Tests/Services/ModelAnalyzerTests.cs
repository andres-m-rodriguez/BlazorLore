using BlazorLore.Scaffold.Cli.Services;
using FluentAssertions;

namespace BlazorLore.Scaffold.Cli.Tests.Services;

public class ModelAnalyzerTests : IDisposable
{
    private readonly string _testDirectory;
    
    public ModelAnalyzerTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ModelAnalyzerTests_{Guid.NewGuid()}");
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
    public async Task AnalyzeModelAsync_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var analyzer = new ModelAnalyzer();
        var nonExistentPath = Path.Combine(_testDirectory, "NonExistent.cs");
        
        // Act & Assert
        await analyzer.Invoking(a => a.AnalyzeModelAsync(nonExistentPath))
            .Should().ThrowAsync<FileNotFoundException>()
            .WithMessage($"Model file not found: {nonExistentPath}");
    }
    
    [Fact]
    public async Task AnalyzeModelAsync_ExtractsNamespace_Correctly()
    {
        // Arrange
        var analyzer = new ModelAnalyzer();
        var modelPath = Path.Combine(_testDirectory, "TestModel.cs");
        var content = @"
namespace MyApp.Models
{
    public class TestModel
    {
        public string Name { get; set; }
    }
}";
        await File.WriteAllTextAsync(modelPath, content);
        
        // Act
        var result = await analyzer.AnalyzeModelAsync(modelPath);
        
        // Assert
        result.Namespace.Should().Be("MyApp.Models");
    }
    
    [Fact]
    public async Task AnalyzeModelAsync_ExtractsClassName_Correctly()
    {
        // Arrange
        var analyzer = new ModelAnalyzer();
        var modelPath = Path.Combine(_testDirectory, "Product.cs");
        var content = @"
namespace MyApp.Models
{
    public class Product
    {
        public string Name { get; set; }
    }
}";
        await File.WriteAllTextAsync(modelPath, content);
        
        // Act
        var result = await analyzer.AnalyzeModelAsync(modelPath);
        
        // Assert
        result.Name.Should().Be("Product");
        result.IsRecord.Should().BeFalse();
    }
    
    [Fact]
    public async Task AnalyzeModelAsync_ExtractsRecordName_Correctly()
    {
        // Arrange
        var analyzer = new ModelAnalyzer();
        var modelPath = Path.Combine(_testDirectory, "ProductRecord.cs");
        var content = @"
namespace MyApp.Models
{
    public record ProductRecord
    {
        public string Name { get; set; }
    }
}";
        await File.WriteAllTextAsync(modelPath, content);
        
        // Act
        var result = await analyzer.AnalyzeModelAsync(modelPath);
        
        // Assert
        result.Name.Should().Be("ProductRecord");
        result.IsRecord.Should().BeTrue();
    }
    
    [Fact]
    public async Task AnalyzeModelAsync_ExtractsProperties_WithTypes()
    {
        // Arrange
        var analyzer = new ModelAnalyzer();
        var modelPath = Path.Combine(_testDirectory, "Customer.cs");
        var content = @"
namespace MyApp.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; init; }
    }
}";
        await File.WriteAllTextAsync(modelPath, content);
        
        // Act
        var result = await analyzer.AnalyzeModelAsync(modelPath);
        
        // Assert
        result.Properties.Should().HaveCount(5);
        
        result.Properties[0].Name.Should().Be("Id");
        result.Properties[0].Type.Should().Be("int");
        result.Properties[0].IsNullable.Should().BeFalse();
        
        result.Properties[1].Name.Should().Be("Name");
        result.Properties[1].Type.Should().Be("string");
        result.Properties[1].IsNullable.Should().BeFalse();
        
        result.Properties[2].Name.Should().Be("Email");
        result.Properties[2].Type.Should().Be("string?");
        result.Properties[2].IsNullable.Should().BeTrue();
        
        result.Properties[3].Name.Should().Be("DateOfBirth");
        result.Properties[3].Type.Should().Be("DateTime");
        
        result.Properties[4].Name.Should().Be("IsActive");
        result.Properties[4].Type.Should().Be("bool");
    }
    
    [Fact]
    public async Task AnalyzeModelAsync_ExtractsValidationAttributes_Correctly()
    {
        // Arrange
        var analyzer = new ModelAnalyzer();
        var modelPath = Path.Combine(_testDirectory, "User.cs");
        var content = @"
using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
    public class User
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }
        
        [Range(1, 120)]
        public int Age { get; set; }
    }
}";
        await File.WriteAllTextAsync(modelPath, content);
        
        // Act
        var result = await analyzer.AnalyzeModelAsync(modelPath);
        
        // Assert
        result.Properties.Should().HaveCount(4);
        
        // Username
        result.Properties[0].ValidationAttributes.Should().HaveCount(1);
        result.Properties[0].ValidationAttributes[0].Name.Should().Be("Required");
        
        // Email
        result.Properties[1].ValidationAttributes.Should().HaveCount(2);
        result.Properties[1].ValidationAttributes[0].Name.Should().Be("Required");
        result.Properties[1].ValidationAttributes[1].Name.Should().Be("EmailAddress");
        
        // Password
        result.Properties[2].ValidationAttributes.Should().HaveCount(1);
        result.Properties[2].ValidationAttributes[0].Name.Should().Be("StringLength");
        result.Properties[2].ValidationAttributes[0].Parameters.Should().ContainKey("Value");
        result.Properties[2].ValidationAttributes[0].Parameters["Value"].Should().Be("100");
        result.Properties[2].ValidationAttributes[0].Parameters.Should().ContainKey("MinimumLength");
        result.Properties[2].ValidationAttributes[0].Parameters["MinimumLength"].Should().Be("6");
        
        // Age
        result.Properties[3].ValidationAttributes.Should().HaveCount(1);
        result.Properties[3].ValidationAttributes[0].Name.Should().Be("Range");
        result.Properties[3].ValidationAttributes[0].Parameters.Should().ContainKey("Value");
    }
    
    [Fact]
    public async Task AnalyzeModelAsync_HandlesRecordWithPrimaryConstructor()
    {
        // Arrange
        var analyzer = new ModelAnalyzer();
        var modelPath = Path.Combine(_testDirectory, "PersonRecord.cs");
        var content = @"
using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
    public record Person(
        [Required] string FirstName,
        [Required] string LastName,
        string? Email,
        [Range(0, 150)] int Age
    );
}";
        await File.WriteAllTextAsync(modelPath, content);
        
        // Act
        var result = await analyzer.AnalyzeModelAsync(modelPath);
        
        // Assert
        result.IsRecord.Should().BeTrue();
        result.Name.Should().Be("Person");
        result.Properties.Should().HaveCount(4);
        
        result.Properties[0].Name.Should().Be("FirstName");
        result.Properties[0].Type.Should().Be("string");
        result.Properties[0].ValidationAttributes.Should().ContainSingle(a => a.Name == "Required");
        
        result.Properties[1].Name.Should().Be("LastName");
        result.Properties[1].Type.Should().Be("string");
        result.Properties[1].ValidationAttributes.Should().ContainSingle(a => a.Name == "Required");
        
        result.Properties[2].Name.Should().Be("Email");
        result.Properties[2].Type.Should().Be("string?");
        result.Properties[2].IsNullable.Should().BeTrue();
        
        result.Properties[3].Name.Should().Be("Age");
        result.Properties[3].Type.Should().Be("int");
        result.Properties[3].ValidationAttributes.Should().ContainSingle(a => a.Name == "Range");
    }
    
    [Fact]
    public async Task AnalyzeModelAsync_HandlesSealedClass()
    {
        // Arrange
        var analyzer = new ModelAnalyzer();
        var modelPath = Path.Combine(_testDirectory, "SealedModel.cs");
        var content = @"
namespace MyApp.Models
{
    public sealed class SealedModel
    {
        public string Name { get; set; }
    }
}";
        await File.WriteAllTextAsync(modelPath, content);
        
        // Act
        var result = await analyzer.AnalyzeModelAsync(modelPath);
        
        // Assert
        result.Name.Should().Be("SealedModel");
        result.Properties.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task AnalyzeModelAsync_HandlesAbstractClass()
    {
        // Arrange
        var analyzer = new ModelAnalyzer();
        var modelPath = Path.Combine(_testDirectory, "AbstractModel.cs");
        var content = @"
namespace MyApp.Models
{
    public abstract class AbstractModel
    {
        public string Name { get; set; }
    }
}";
        await File.WriteAllTextAsync(modelPath, content);
        
        // Act
        var result = await analyzer.AnalyzeModelAsync(modelPath);
        
        // Assert
        result.Name.Should().Be("AbstractModel");
        result.Properties.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task AnalyzeModelAsync_ThrowsException_WhenNoClassOrRecordFound()
    {
        // Arrange
        var analyzer = new ModelAnalyzer();
        var modelPath = Path.Combine(_testDirectory, "InvalidFile.cs");
        var content = @"
namespace MyApp.Models
{
    // No class or record declaration
}";
        await File.WriteAllTextAsync(modelPath, content);
        
        // Act & Assert
        await analyzer.Invoking(a => a.AnalyzeModelAsync(modelPath))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Could not find class or record declaration in the file.");
    }
    
    [Fact]
    public async Task AnalyzeModelAsync_HandlesComplexAttributeParameters()
    {
        // Arrange
        var analyzer = new ModelAnalyzer();
        var modelPath = Path.Combine(_testDirectory, "ComplexModel.cs");
        var content = @"
using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
    public class ComplexModel
    {
        [StringLength(50, ErrorMessage = ""Name must be between {2} and {1} characters"", MinimumLength = 3)]
        public string Name { get; set; }
        
        [RegularExpression(@""^[a-zA-Z]+$"", ErrorMessage = ""Only letters allowed"")]
        public string Code { get; set; }
    }
}";
        await File.WriteAllTextAsync(modelPath, content);
        
        // Act
        var result = await analyzer.AnalyzeModelAsync(modelPath);
        
        // Assert
        result.Properties.Should().HaveCount(2);
        
        // Name property
        var nameAttr = result.Properties[0].ValidationAttributes[0];
        nameAttr.Name.Should().Be("StringLength");
        nameAttr.Parameters.Should().ContainKey("ErrorMessage");
        nameAttr.Parameters["ErrorMessage"].Should().Be("Name must be between {2} and {1} characters");
        nameAttr.Parameters.Should().ContainKey("MinimumLength");
        nameAttr.Parameters["MinimumLength"].Should().Be("3");
        
        // Code property
        var codeAttr = result.Properties[1].ValidationAttributes[0];
        codeAttr.Name.Should().Be("RegularExpression");
        codeAttr.Parameters.Should().ContainKey("ErrorMessage");
        codeAttr.Parameters["ErrorMessage"].Should().Be("Only letters allowed");
    }
}