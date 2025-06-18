# BlazorLore Scaffold CLI

A powerful command-line tool for scaffolding and refactoring Blazor components with modern C# patterns. Generate components, forms, and more with ease while following best practices.

[![NuGet](https://img.shields.io/nuget/v/BlazorLore.Scaffold.Cli.svg)](https://www.nuget.org/packages/BlazorLore.Scaffold.Cli/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## ✨ Features

- 🚀 **Component Generation** - Create Blazor components with optional code-behind and CSS isolation
- 📝 **Form Generation** - Generate forms from C# model classes with validation support
- 🔧 **Code Refactoring** - Extract inline code to code-behind files
- 🎯 **Modern C# Support** - Convert legacy `[Inject]` attributes to constructor injection
- ⚡ **AOT Compiled** - Fast startup and execution with ahead-of-time compilation
- 📦 **Template-Based** - Customizable Scriban templates for code generation

## 📥 Installation

### As a Global Tool

Install the CLI tool globally using the .NET CLI:

```bash
dotnet tool install --global BlazorLore.Scaffold.Cli
```

After installation, you can use the tool from anywhere:

```bash
blazor-scaffold --help
```

### Update the Tool

To update to the latest version:

```bash
dotnet tool update --global BlazorLore.Scaffold.Cli
```

### Uninstall

To uninstall the tool:

```bash
dotnet tool uninstall --global BlazorLore.Scaffold.Cli
```

## 🚀 Quick Start

### Generate a Component

Create a new Blazor component:

```bash
# Basic component
blazor-scaffold component generate MyComponent

# With code-behind and CSS
blazor-scaffold component generate MyComponent --code-behind --css

# Specify output path
blazor-scaffold component generate MyComponent --path ./Components
```

### Generate a Form from a Model

Create a form from a C# model class:

```bash
# Generate create form
blazor-scaffold form generate ./Models/Product.cs

# Generate edit form with custom name
blazor-scaffold form generate ./Models/Product.cs --name ProductEditForm --edit

# Custom submit action
blazor-scaffold form generate ./Models/User.cs --submit-action HandleRegistration
```

### Refactor Existing Components

Extract inline code to code-behind:

```bash
blazor-scaffold component refactor ./Components/MyComponent.razor --extract-code
```

Modernize code-behind to use constructor injection:

```bash
blazor-scaffold component modernize ./Components/MyComponent.razor.cs
```

## 📚 Commands

### Component Commands

#### `component generate`
Generate a new Blazor component.

**Arguments:**
- `name` - The name of the component (required)

**Options:**
- `--path` - Output directory (default: current directory)
- `--code-behind` - Generate with code-behind file
- `--css` - Generate with CSS isolation file

**Example:**
```bash
blazor-scaffold component generate UserProfile --path ./Components/Users --code-behind --css
```

#### `component refactor`
Refactor an existing Blazor component.

**Arguments:**
- `file` - Path to the component file (required)

**Options:**
- `--extract-code` - Extract @code block to code-behind file

**Example:**
```bash
blazor-scaffold component refactor ./Components/Dashboard.razor --extract-code
```

#### `component modernize`
Modernize component code-behind to use constructor injection.

**Arguments:**
- `file` - Path to the code-behind file (required)

**Example:**
```bash
blazor-scaffold component modernize ./Components/Dashboard.razor.cs
```

#### `component list`
List all Blazor components in a directory.

**Options:**
- `--dir` - Directory to search (default: current directory)

**Example:**
```bash
blazor-scaffold component list --dir ./src/Components
```

### Form Commands

#### `form generate`
Generate a form from a C# model class.

**Arguments:**
- `model` - Path to the model file (required)

**Options:**
- `--name` - Form component name (default: {ModelName}Form)
- `--path` - Output directory (default: current directory)
- `--edit` - Generate as edit form with parameter
- `--submit-action` - Method name for form submission (default: OnSubmit)

**Example:**
```bash
blazor-scaffold form generate ./Models/Customer.cs --name CustomerRegistrationForm --path ./Forms
```

## 🎯 Examples

### Creating a Complete Feature

1. Create a model:
```csharp
// Models/Product.cs
using System.ComponentModel.DataAnnotations;

namespace MyApp.Models;

public class Product
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [Range(0.01, 10000)]
    public decimal Price { get; set; }
    
    public bool IsActive { get; set; }
}
```

2. Generate a form for the model:
```bash
blazor-scaffold form generate ./Models/Product.cs --name ProductForm --path ./Components/Products
```

3. Generate a component to display products:
```bash
blazor-scaffold component generate ProductList --path ./Components/Products --code-behind
```

### Refactoring Legacy Components

1. Extract inline code from an existing component:
```bash
blazor-scaffold component refactor ./Legacy/OldComponent.razor --extract-code
```

2. Modernize the extracted code-behind:
```bash
blazor-scaffold component modernize ./Legacy/OldComponent.razor.cs
```

## 🔧 Configuration

### Template Customization

Templates are located in the `Templates` directory and use the Scriban template engine. You can customize them to match your coding standards.

### Demo Mode

Run without arguments to see a live demo:

```bash
blazor-scaffold
```

## 🏗️ Architecture

The CLI tool is built with:
- **.NET 9** - Latest framework features
- **System.CommandLine** - Modern command-line parsing
- **Scriban** - Powerful template engine
- **AOT Compilation** - Native performance

### Project Structure

```
BlazorLore.Scaffold.Cli/
├── Commands/          # CLI command definitions
├── Services/          # Core business logic
├── Templates/         # Scriban templates
├── Models/           # Sample models for testing
└── Program.cs        # Entry point with demo mode
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with ❤️ for the Blazor community
- Powered by [Scriban](https://github.com/scriban/scriban) template engine
- Uses [System.CommandLine](https://github.com/dotnet/command-line-api) for CLI parsing

## 📞 Support

- 📧 Email: support@blazorlore.com
- 🐛 Issues: [GitHub Issues](https://github.com/yourusername/BlazorLore.Scaffold/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/yourusername/BlazorLore.Scaffold/discussions)

---

Made with ⚡ by the BlazorLore team