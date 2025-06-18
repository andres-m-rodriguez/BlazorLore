# BlazorLore Scaffold Template Variables

This document lists all available variables that can be used in custom Scriban templates.

## Common Variables (Available in all templates)

| Variable | Type | Description | Example |
|----------|------|-------------|---------|
| `name` | string | Component/Service/Form name | `UserProfile` |
| `namespace` | string | Detected or specified namespace | `MyApp.Components` |
| `project_name` | string | Current project name | `MyBlazorApp` |
| `timestamp` | DateTime | Current timestamp | `2025-01-15 10:30:00` |
| `user` | string | Current system user | `john.doe` |

## Component Template Variables

| Variable | Type | Description | Example |
|----------|------|-------------|---------|
| `name` | string | Component name | `UserCard` |
| `namespace` | string | Component namespace | `MyApp.Components` |
| `has_code_behind` | bool | Generate code-behind file | `true` |
| `has_css` | bool | Generate CSS file | `true` |
| `base_class` | string | Base class to inherit from | `ComponentBase` |
| `interfaces` | string[] | Interfaces to implement | `["IDisposable"]` |

## Form Template Variables

| Variable | Type | Description | Example |
|----------|------|-------------|---------|
| `form_name` | string | Form component name | `ProductForm` |
| `model_instance` | string | Model instance name | `product` |
| `model_info` | object | Model information | See below |
| `model_info.name` | string | Model class name | `Product` |
| `model_info.namespace` | string | Model namespace | `MyApp.Models` |
| `model_info.properties` | array | Model properties | See below |
| `is_edit_form` | bool | Edit vs Create form | `true` |
| `submit_action` | string | Submit method name | `HandleSubmit` |

### Model Property Structure
```json
{
  "name": "Price",
  "type": "decimal",
  "is_nullable": false,
  "is_required": true,
  "max_length": null,
  "attributes": ["Required", "Range(0, 9999.99)"]
}
```

## Service Template Variables

| Variable | Type | Description | Example |
|----------|------|-------------|---------|
| `service_name` | string | Service class name | `UserService` |
| `interface_name` | string | Interface name | `IUserService` |
| `namespace` | string | Service namespace | `MyApp.Services` |
| `has_interface` | bool | Generate interface | `true` |
| `is_scoped` | bool | Service lifetime | `true` |
| `dependencies` | array | Constructor dependencies | `["ILogger<UserService>", "IDbContext"]` |

## Custom Variables

You can pass custom variables using the `--vars` parameter:

```bash
blazor-scaffold component MyComponent --template custom --vars author="John Doe",version="1.0"
```

These will be available in your template as:
- `{{ custom.author }}`
- `{{ custom.version }}`

## Scriban Functions

All built-in Scriban functions are available. Commonly used:

- `{{ name | string.downcase }}` - Lowercase
- `{{ name | string.upcase }}` - Uppercase  
- `{{ name | string.capitalize }}` - Capitalize first letter
- `{{ for item in items }}...{{ end }}` - Loops
- `{{ if condition }}...{{ else }}...{{ end }}` - Conditionals

## Example Custom Template

```scriban
@page "/{{ name | string.downcase }}"
@namespace {{ namespace }}

<div class="{{ name | string.downcase }}-container">
    <h1>{{ name | string.humanize }}</h1>
    
    {{ if custom.include_counter }}
    <Counter />
    {{ end }}
    
    @* Generated on {{ timestamp }} by {{ user }} *@
</div>

@code {
    // Component: {{ name }}
    // Version: {{ custom.version ?? "1.0" }}
}
```