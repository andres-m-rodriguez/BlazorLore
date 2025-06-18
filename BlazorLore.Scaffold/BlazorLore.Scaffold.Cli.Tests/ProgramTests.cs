using System.CommandLine;
using System.CommandLine.Parsing;
using FluentAssertions;

namespace BlazorLore.Scaffold.Cli.Tests;

public class ProgramTests
{
    
    [Fact]
    public async Task Program_ShowsHelp_WhenNoArgumentsProvided()
    {
        // Arrange
        var args = Array.Empty<string>();
        
        // Act
        var result = await RunProgramAsync(args);
        
        // Assert
        result.Should().Be(0);
    }
    
    [Fact]
    public async Task Program_ShowsHelp_WithHelpFlag()
    {
        // Arrange
        var args = new[] { "--help" };
        
        // Act
        var result = await RunProgramAsync(args);
        
        // Assert
        result.Should().Be(0);
    }
    
    [Fact]
    public async Task Program_ShowsVersion_WithVersionFlag()
    {
        // Arrange
        var args = new[] { "--version" };
        
        // Act
        var result = await RunProgramAsync(args);
        
        // Assert
        result.Should().Be(0);
    }
    
    [Fact]
    public async Task Program_ShowsError_ForInvalidCommand()
    {
        // Arrange
        var args = new[] { "invalid-command" };
        
        // Act
        var result = await RunProgramAsync(args);
        
        // Assert
        result.Should().NotBe(0);
    }
    
    [Fact]
    public async Task Program_ShowsComponentHelp()
    {
        // Arrange
        var args = new[] { "component", "--help" };
        
        // Act
        var result = await RunProgramAsync(args);
        
        // Assert
        result.Should().Be(0);
    }
    
    [Fact]
    public async Task Program_ShowsFormHelp()
    {
        // Arrange
        var args = new[] { "form", "--help" };
        
        // Act
        var result = await RunProgramAsync(args);
        
        // Assert
        result.Should().Be(0);
    }
    
    private async Task<int> RunProgramAsync(string[] args)
    {
        // Create the root command as it's created in Program.cs
        var rootCommand = new RootCommand("Blazor scaffolding CLI tool");
        
        // Add component command
        var componentCommand = new BlazorLore.Scaffold.Cli.Commands.ComponentCommand();
        rootCommand.AddCommand(componentCommand.GetCommand());
        
        // Add form command
        var formCommand = new BlazorLore.Scaffold.Cli.Commands.FormCommand();
        rootCommand.AddCommand(formCommand.GetCommand());
        
        return await rootCommand.InvokeAsync(args);
    }
}