# BlazorLore Scaffold CLI

A powerful command-line tool for scaffolding and refactoring Blazor components with modern C# patterns. Generate components, forms, services, and more with ease while following best practices.

[![NuGet](https://img.shields.io/nuget/v/BlazorLore.Scaffold.Cli.svg)](https://www.nuget.org/packages/BlazorLore.Scaffold.Cli/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## ✨ Features

- 🚀 **Component Generation** - Create Blazor components with optional code-behind and CSS isolation
- 📝 **Form Generation** - Generate forms from C# model classes with validation support
- 🏗️ **Service Generation** - Create service classes with interfaces and repository patterns
- 🎨 **Custom Templates** - Create and use your own Scriban templates for code generation
- 🔧 **Code Refactoring** - Extract inline code to code-behind files using partial classes
- 🎯 **Modern C# Support** - Convert legacy `[Inject]` attributes to constructor injection
- 🔍 **Smart Namespace Detection** - Automatically detects namespaces from project structure
- ⚡ **AOT Compiled** - Fast startup and execution with ahead-of-time compilation
- 📦 **Template-Based** - Customizable Scriban templates for all code generation

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

Create a new Blazor component with simplified syntax:

```bash
# Basic component
blazor-scaffold component MyComponent -o ./Components

# With code-behind and CSS
blazor-scaffold component MyComponent -o ./Components -c -s

# Using a custom template
blazor-scaffold component MyComponent --template my-custom --vars "author=John Doe,version=1.0"
```

### Generate a Service

Create service classes with various patterns:

```bash
# Basic service with interface
blazor-scaffold service UserService -o ./Services

# Repository pattern with CRUD operations
blazor-scaffold service ProductService -o ./Services --repository --entity Product --id-type Guid

# Service with custom dependencies
blazor-scaffold service OrderService -o ./Services --dependencies "IDbContext:db" "IMapper:mapper"
```

### Generate a Form from a Model

Create a form from a C# model class:

```bash
# Generate create form
blazor-scaffold form generate ./Models/Product.cs

# Generate edit form with custom name
blazor-scaffold form generate ./Models/Product.cs --name ProductEditForm --edit
```

### Refactor Existing Components

Extract inline code to code-behind:

```bash
# Extract @code block to code-behind with partial classes
blazor-scaffold refactor ./Components/MyComponent.razor --extract-code

# Modernize code-behind to use constructor injection
blazor-scaffold refactor ./Components/MyComponent.razor.cs --modernize
```

## 📚 Commands

### Component Command

Generate a new Blazor component.

**Arguments:**
- `name` - The name of the component (required)

**Options:**
- `--output, -o` - Output directory (default: current directory)
- `--code-behind, -c` - Generate with code-behind file
- `--css, -s` - Generate with CSS isolation file
- `--template, -t` - Use a custom template
- `--vars, -v` - Custom variables for template (format: key=value,key2=value2)

**Example:**
```bash
blazor-scaffold component UserProfile -o ./Components/Users -c -s
```

### Service Command

Generate a new service class.

**Arguments:**
- `name` - The name of the service (required)

**Options:**
- `--output, -o` - Output directory (default: current directory)
- `--interface, -i` - Generate an interface (default: true)
- `--repository, -r` - Generate repository pattern with CRUD operations
- `--entity, -e` - Entity name for repository pattern
- `--id-type, -t` - Entity ID type (default: int)
- `--dependencies, -d` - Constructor dependencies (format: Type:parameterName)

**Examples:**
```bash
# Basic service
blazor-scaffold service UserService -o ./Services

# Repository pattern
blazor-scaffold service ProductService -o ./Services -r -e Product -t Guid

# With dependencies
blazor-scaffold service OrderService -o ./Services -d "IDbContext:context" "ILogger<OrderService>:logger"
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

### Refactor Command

Refactor existing Blazor components.

**Arguments:**
- `file` - The component or code-behind file to refactor (required)

**Options:**
- `--extract-code, -e` - Extract @code block to code-behind file
- `--modernize, -m` - Modernize code-behind to use constructor injection

**Examples:**
```bash
# Extract code to code-behind
blazor-scaffold refactor ./Components/Dashboard.razor -e

# Modernize to constructor injection
blazor-scaffold refactor ./Components/Dashboard.razor.cs -m
```

### Template Commands

#### `init-templates`
Initialize custom templates in your project.

**Options:**
- `--path, -p` - Path where templates will be initialized (default: .blazor-templates)

**Example:**
```bash
blazor-scaffold init-templates
```

#### `list-templates`
List all available templates.

**Options:**
- `--category, -c` - Filter by category (component, form, service)
- `--custom-only` - Show only custom templates

**Example:**
```bash
blazor-scaffold list-templates --category component
```

## 🎨 Custom Templates

Create your own templates to match your coding standards:

1. Initialize templates in your project:
   ```bash
   blazor-scaffold init-templates
   ```

2. Edit the generated templates in `.blazor-templates/`

3. Use your custom template:
   ```bash
   blazor-scaffold component MyComponent --template my-custom --vars "author=Your Name,version=1.0"
   ```

### Template Variables

Templates have access to many variables:

**Common Variables:**
- `name` - Component/Service/Form name
- `namespace` - Detected or specified namespace
- `timestamp` - Current timestamp
- `user` - Current system user

**Component Variables:**
- `has_code_behind` - Generate code-behind file
- `has_css` - Generate CSS file

**Service Variables:**
- `service_name` - Service class name
- `interface_name` - Interface name
- `dependencies` - Constructor dependencies

See [TEMPLATE_VARIABLES.md](TEMPLATE_VARIABLES.md) for complete documentation.

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

2. Generate a service for the model:
```bash
blazor-scaffold service ProductService -o ./Services --repository --entity Product
```

3. Generate forms for the model:
```bash
# Create form
blazor-scaffold form generate ./Models/Product.cs --name ProductCreateForm --path ./Components/Products

# Edit form
blazor-scaffold form generate ./Models/Product.cs --name ProductEditForm --path ./Components/Products --edit
```

4. Generate a component to display products:
```bash
blazor-scaffold component ProductList -o ./Components/Products -c
```

### Refactoring Legacy Components

1. Extract inline code from an existing component:
```bash
blazor-scaffold refactor ./Legacy/OldComponent.razor --extract-code
```

2. Modernize the extracted code-behind:
```bash
blazor-scaffold refactor ./Legacy/OldComponent.razor.cs --modernize
```

## 🔧 What's New in v1.0.1

### Bug Fixes
- ✅ Fixed code-behind extraction to use partial classes instead of inheritance pattern
- ✅ Fixed form generation to include model namespace in @using directives
- ✅ Fixed form generation event callbacks (OnFormSubmit/OnFormCancel) to avoid naming conflicts
- ✅ Fixed namespace detection in refactoring to use project structure instead of hardcoded values
- ✅ Added missing `@using Microsoft.AspNetCore.Components.Forms` to form templates

### New Features
- 🎉 Service generation with interface and repository patterns
- 🎉 Custom template system with full Scriban support
- 🎉 Simplified command syntax (no more subcommands for component)
- 🎉 Template discovery from multiple locations
- 🎉 Smart namespace detection from project structure

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
│   ├── ComponentGenerator.cs
│   ├── FormGenerator.cs
│   ├── ServiceGenerator.cs
│   ├── ComponentRefactorer.cs
│   ├── CustomTemplateService.cs
│   └── ModelAnalyzer.cs
├── Templates/         # Scriban templates
│   ├── Component/
│   ├── Form/
│   └── Service/
└── Program.cs        # Entry point
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

- 🐛 Issues: [GitHub Issues](https://github.com/yourusername/BlazorLore.Scaffold/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/yourusername/BlazorLore.Scaffold/discussions)

---

Made with ⚡ by the BlazorLore team