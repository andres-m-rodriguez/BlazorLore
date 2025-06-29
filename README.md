# BlazorLore

> Enriching the Blazor ecosystem with powerful CLI tools, libraries, and development utilities

[![NuGet Version](https://img.shields.io/nuget/v/BlazorLore.Format.Cli?label=blazorfmt)](https://www.nuget.org/packages/BlazorLore.Format.Cli/)
[![NuGet Version](https://img.shields.io/nuget/v/BlazorLore.Scaffold.Cli?label=blazor-scaffold)](https://www.nuget.org/packages/BlazorLore.Scaffold.Cli/)
[![VS Code Extension](https://img.shields.io/visual-studio-marketplace/v/blazor-formatter?label=vscode-extension)](https://marketplace.visualstudio.com/items?itemName=blazor-formatter)

BlazorLore is a comprehensive suite of development tools designed to enhance productivity and code quality in Blazor applications. From automatic code formatting to intelligent scaffolding, BlazorLore provides essential utilities that streamline the Blazor development experience.

## 📦 Projects

### BlazorLore.Format - Code Formatter for Blazor

A Prettier-like code formatter specifically designed for Blazor and Razor components.

**Features:**
- 🎨 Automatic formatting of `.razor` files
- ⚙️ Configurable formatting rules
- 🔧 VS Code extension for format-on-save
- 🚀 Native AOT compilation for blazing-fast performance
- 📝 Support for all Blazor syntax including render fragments and cascading parameters

[Learn more about BlazorLore.Format →](./BlazorLore.Format/BlazorLore.Format.Cli/README.md)

### BlazorLore.Scaffold - Intelligent Code Generation

A powerful scaffolding tool that generates production-ready Blazor components, forms, services, and more.

**Features:**
- 🏗️ Generate components with optional code-behind and CSS isolation
- 📋 Create forms from C# models with built-in validation
- 🔌 Generate service classes with dependency injection
- 🗄️ Repository pattern generation with Entity Framework Core
- 🌐 Server-Side Rendering (SSR) component generation
- 🔄 Refactor existing components (extract code-behind, modernize DI)
- 🎯 Custom template support with Scriban

[Learn more about BlazorLore.Scaffold →](./BlazorLore.Scaffold/BlazorLore.Scaffold.Cli/README.md)

## 💡 Core Capabilities

### Code Formatting
BlazorLore.Format brings Prettier-style formatting to the Blazor world, automatically organizing and beautifying Razor components with configurable rules for indentation, line breaks, and attribute formatting. The formatter understands all Blazor-specific syntax including render fragments, cascading parameters, and complex nested structures.

### Intelligent Scaffolding
BlazorLore.Scaffold generates production-ready code from simple commands:
- **Components**: Full Blazor components with optional code-behind and CSS isolation
- **Forms**: Data entry forms generated from C# models with built-in validation
- **Services**: Dependency-injected service classes following best practices
- **Repositories**: Complete repository pattern implementation with Entity Framework Core
- **SSR Components**: Server-side rendered components with all Blazor render modes

### Code Refactoring
Transform existing code to modern patterns:
- Extract inline code to code-behind files for better separation of concerns
- Modernize legacy dependency injection to use primary constructors
- Update components to use the latest C# and Blazor features

## 🎯 Key Features

### Native Performance
Both CLI tools are compiled with Native AOT, providing instant startup times and minimal memory footprint.

### Extensible Template System
Create custom templates using the Scriban templating engine:

```bash
# Initialize custom templates
blazor-scaffold init-templates

# List available templates
blazor-scaffold list-templates
```

### Smart Detection
- Automatic namespace detection from project structure
- Intelligent import resolution
- Context-aware code generation

### Modern C# Support
- Primary constructor support
- File-scoped namespaces
- Latest C# language features
- All Blazor render modes (Server, WebAssembly, Auto, None)

## 📖 Documentation

- [Format CLI Documentation](./BlazorLore.Format/BlazorLore.Format.Cli/README.md)
- [Scaffold CLI Documentation](./BlazorLore.Scaffold/BlazorLore.Scaffold.Cli/README.md)
- [VS Code Extension Guide](./BlazorLore.Format/BlazorLore.Format.VSCode/README.md)
- [Custom Templates Guide](./BlazorLore.Scaffold/example-templates/README.md)

## 🤝 Contributing

We welcome contributions! Whether it's bug reports, feature requests, or pull requests, your input helps make BlazorLore better for everyone. The project follows modern .NET development practices with comprehensive test coverage and automated CI/CD pipelines.

## 🗺️ Future Vision

BlazorLore continues to evolve with the Blazor ecosystem:
- **Component Library**: Production-ready UI components for Blazor applications
- **HTTP Client Abstractions**: Reactive API integration patterns for seamless backend communication
- Visual Studio extension for integrated formatting
- Advanced scaffolding templates for APIs and microservices
- AI-powered code suggestions and optimizations
- Blazor-specific linting and best practice enforcement
- Automated component documentation generation
- Integration with .NET CLI template system

## 🏗️ Architecture

BlazorLore leverages cutting-edge .NET technologies:
- **Native AOT Compilation**: Both CLI tools compile to native code for instant startup
- **Scriban Templates**: Flexible, customizable code generation
- **System.CommandLine**: Modern, intuitive command-line interface
- **Roslyn APIs**: Deep integration with C# compiler services
- **Custom Parsers**: Specialized Blazor/Razor syntax understanding

---

<p align="center">
  Made with ❤️ for the Blazor community
</p>