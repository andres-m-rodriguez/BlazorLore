# BlazorLore.Scaffold.Cli Tests

This project contains comprehensive unit and integration tests for the BlazorLore Scaffold CLI tool.

## Test Structure

### Unit Tests

#### Services
- **ModelAnalyzerTests** - Tests for analyzing C# model files and extracting properties, attributes, and metadata
- **ComponentGeneratorTests** - Tests for generating Blazor components with optional code-behind and CSS files
- **ComponentRefactorerTests** - Tests for refactoring components (extracting code-behind, modernizing to constructor injection)
- **FormGeneratorTests** - Tests for generating forms from model files

#### Commands
- **ComponentCommandTests** - Integration tests for the `component` command and its subcommands
- **FormCommandTests** - Integration tests for the `form` command and its subcommands

#### Other
- **ProgramTests** - Tests for the main program entry point and CLI setup

### Test Utilities
- **TestHelpers** - Helper methods for creating temporary directories, mock files, and test templates

## Running Tests

To run all tests:
```bash
dotnet test
```

To run tests with detailed output:
```bash
dotnet test --logger "console;verbosity=detailed"
```

To run a specific test category:
```bash
dotnet test --filter "FullyQualifiedName~ModelAnalyzerTests"
```

## Test Coverage

The tests cover:
- File generation and template rendering
- Model analysis and property extraction
- Validation attribute parsing
- Code refactoring operations
- CLI command parsing and execution
- Error handling scenarios
- Edge cases and invalid inputs

## Dependencies

The test project uses:
- **xUnit** - Test framework
- **FluentAssertions** - Fluent assertion library
- **Moq** - Mocking framework
- **System.CommandLine** - For testing CLI commands

## Notes

- Tests use temporary directories for file operations to avoid conflicts
- Template files are created dynamically during test setup
- All file system operations are cleaned up after tests complete