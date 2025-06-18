# BlazorLore Format CLI

A powerful formatter for Blazor and Razor components, similar to Prettier for HTML/CSS/JS. This tool helps maintain consistent code style across your Blazor projects.

## Installation

```bash
dotnet tool install --global BlazorLore.Format.Cli
```

## Usage

### Format files

```bash
# Format all .razor files in current directory
blazorfmt format

# Format specific files
blazorfmt format Component1.razor Component2.razor

# Format and write changes
blazorfmt format --write

# Check if files are formatted
blazorfmt format --check
```

### Initialize configuration

```bash
# Create a .blazorfmt.json config file
blazorfmt init
```

## Configuration

Create a `.blazorfmt.json` file in your project root:

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

### Configuration Options

- **IndentSize**: Number of spaces for indentation (default: 4)
- **UseTabs**: Use tabs instead of spaces (default: false)
- **AttributeFormatting**: How to format attributes
  - `"singleLine"`: Keep all attributes on one line
  - `"multilineAlways"`: Always put each attribute on a new line
  - `"multilineWhenMany"`: Break to multiple lines when threshold is reached
- **AttributeBreakThreshold**: Number of attributes before breaking to multiple lines (default: 3)
- **BreakContentWithManyAttributes**: Break content to new line when element has many attributes (default: true)
- **ContentBreakThreshold**: Number of attributes before breaking content to new line (default: 2)
- **QuoteStyle**: Quote style for attributes (`"double"` or `"single"`)

## Command Line Options

- `--write, -w`: Write formatted output back to files
- `--check, -c`: Check if files are formatted (exit with error if not)
- `--config`: Path to configuration file
- `--indent-size`: Override indent size
- `--use-tabs`: Override to use tabs
- `--attribute-break-threshold`: Override attribute break threshold
- `--content-break-threshold`: Override content break threshold
- `--break-content-with-many-attributes`: Override content breaking behavior

## VS Code Extension

For the best experience, install the BlazorLore Format VS Code extension which integrates seamlessly with this CLI tool.

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

## License

MIT License - see LICENSE file for details