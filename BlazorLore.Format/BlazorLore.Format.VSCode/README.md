# BlazorLore Format - VS Code Extension

A powerful formatter for Blazor and Razor components, similar to Prettier for HTML/CSS/JS. This extension provides seamless integration with VS Code to format your Blazor components with proper indentation, smart line breaking, and configurable formatting options.

## Features

- 🎨 **Format on Save**: Automatically format your Blazor/Razor files when you save
- 📏 **Smart Line Breaking**: Intelligently break attributes and content based on configurable thresholds
- ⚙️ **Highly Configurable**: Customize formatting rules to match your team's style guide
- 🚀 **Fast Performance**: Powered by Native AOT compiled CLI for instant formatting
- 📝 **Supports .razor and .cshtml**: Works with both Blazor components and Razor views
- 🔧 **Status Bar Integration**: See formatter status and version in the status bar

## Requirements

The extension will automatically prompt you to install the BlazorLore Format CLI tool on first use. 

You can also install it manually:
```bash
dotnet tool install --global BlazorLore.Format.Cli
```

Or use the command palette: `Blazor Formatter: Install/Update CLI Tool`

## Extension Settings

This extension contributes the following settings:

* `blazorFormatter.formatOnSave`: Enable/disable formatting on save (default: `true`)
* `blazorFormatter.executablePath`: Path to the blazorfmt executable (default: `blazorfmt`)
* `blazorFormatter.indentSize`: Number of spaces for indentation
* `blazorFormatter.useTabs`: Use tabs instead of spaces
* `blazorFormatter.attributeBreakThreshold`: Number of attributes before breaking to multiple lines (default: `3`)
* `blazorFormatter.contentBreakThreshold`: Number of attributes before breaking content to new line (default: `2`)
* `blazorFormatter.breakContentWithManyAttributes`: Break content to new line when element has many attributes (default: `true`)
* `blazorFormatter.configPath`: Path to .blazorfmt.json configuration file

## Usage

### Format Document

1. Open a `.razor` or `.cshtml` file
2. Use one of these methods:
   - Press `Shift+Alt+F` (default VS Code format command)
   - Right-click and select "Format Document"
   - Command Palette: "Blazor Formatter: Format Document"

### Create Configuration File

1. Command Palette: "Blazor Formatter: Create Configuration File"
2. This creates a `.blazorfmt.json` file in your workspace root

## Configuration File

Create a `.blazorfmt.json` file in your project root for project-wide settings:

```json
{
  "IndentSize": 4,
  "UseTabs": false,
  "AttributeFormatting": "multilineWhenMany",
  "AttributeBreakThreshold": 3,
  "BreakContentWithManyAttributes": true,
  "ContentBreakThreshold": 2,
  "QuoteStyle": "double"
}
```

## Examples

### Before formatting:
```razor
<button class="btn btn-primary" @onclick="IncrementCount" disabled="@IsDisabled" title="Click me">Click me</button>
```

### After formatting (with default settings):
```razor
<button
    class="btn btn-primary"
    @onclick="IncrementCount"
    disabled="@IsDisabled"
    title="Click me">
    Click me
</button>
```

### With fewer attributes:
```razor
<button class="btn" @onclick="Click">Click me</button>
```

## Known Issues

- The formatter requires the CLI tool to be installed globally
- Large files may take a moment to format

## Release Notes

### 1.0.0

Initial release of BlazorLore Format for VS Code:
- Format on save functionality
- Configurable formatting options
- Status bar integration
- Support for .razor and .cshtml files

## Contributing

Found a bug or have a feature request? Please open an issue on our [GitHub repository](https://github.com/blazorlore/blazor-formatter).

## License

MIT License - see LICENSE file for details