using System.Text;
using System.Text.RegularExpressions;

namespace BlazorLore.Scaffold.Cli.Services;

public class ComponentRefactorer
{
    public async Task ExtractCodeBehindAsync(string componentPath)
    {
        if (!File.Exists(componentPath))
        {
            throw new FileNotFoundException($"Component file not found: {componentPath}");
        }

        var content = await File.ReadAllTextAsync(componentPath);
        var componentName = Path.GetFileNameWithoutExtension(componentPath);
        var componentDir = Path.GetDirectoryName(componentPath) ?? ".";
        
        // Detect namespace
        var detectedNamespace = await DetectNamespaceAsync(componentPath, content);

        // Extract @inject directives
        var injectPattern = @"@inject\s+(\S+)\s+(\S+)";
        var injectMatches = Regex.Matches(content, injectPattern);
        var injects = new List<(string type, string name)>();

        foreach (Match match in injectMatches)
        {
            injects.Add((match.Groups[1].Value, match.Groups[2].Value));
        }

        // Extract @code block
        var codeBlockPattern = @"@code\s*{((?:[^{}]|{(?:[^{}]|{[^{}]*})*})*)}";
        var codeMatch = Regex.Match(content, codeBlockPattern, RegexOptions.Singleline);

        if (!codeMatch.Success)
        {
            Console.WriteLine("No @code block found in the component.");
            return;
        }

        var codeContent = codeMatch.Groups[1].Value.Trim();

        // Remove @inject directives and @code block from the original file
        var updatedContent = Regex.Replace(content, injectPattern, "");
        updatedContent = Regex.Replace(updatedContent, codeBlockPattern, "").Trim();

        // No need for @inherits directive with partial classes
        // The code-behind will be a partial class with the same name

        // Generate code-behind file
        var codeBehindPath = Path.Combine(componentDir, $"{componentName}.razor.cs");
        var codeBehindContent = GenerateCodeBehindContent(componentName, detectedNamespace, injects, codeContent);

        // Write updated files
        await File.WriteAllTextAsync(componentPath, updatedContent);
        await File.WriteAllTextAsync(codeBehindPath, codeBehindContent);
    }

    public async Task ConvertToConstructorInjectionAsync(string codeBehindPath)
    {
        if (!File.Exists(codeBehindPath))
        {
            throw new FileNotFoundException($"Code-behind file not found: {codeBehindPath}");
        }

        var content = await File.ReadAllTextAsync(codeBehindPath);

        // Find all [Inject] properties
        var injectPattern = @"\[Inject\]\s*(?:public\s+)?(\S+)\s+(\S+)\s*{\s*get;\s*set;\s*}";
        var matches = Regex.Matches(content, injectPattern);

        if (!matches.Any())
        {
            Console.WriteLine("No [Inject] properties found.");
            return;
        }

        var injectedServices = new List<(string type, string name)>();
        foreach (Match match in matches)
        {
            injectedServices.Add((match.Groups[1].Value, match.Groups[2].Value));
        }

        // Extract class name and namespace
        var classPattern = @"public\s+(?:partial\s+)?class\s+(\w+)(?:\s*:\s*ComponentBase)?";
        var classMatch = Regex.Match(content, classPattern);
        
        if (!classMatch.Success)
        {
            throw new InvalidOperationException("Could not find class declaration.");
        }

        var className = classMatch.Groups[1].Value;

        // Extract namespace
        var namespacePattern = @"namespace\s+([\w.]+)";
        var namespaceMatch = Regex.Match(content, namespacePattern);
        var namespaceName = namespaceMatch.Success ? namespaceMatch.Groups[1].Value : "MyApp.Components";

        // Generate modernized content with primary constructor
        var modernizedContent = GenerateModernizedCodeBehind(namespaceName, className, injectedServices, content);

        await File.WriteAllTextAsync(codeBehindPath, modernizedContent);
    }

    private string GenerateCodeBehindContent(string componentName, string namespaceName, List<(string type, string name)> injects, string codeContent)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("using Microsoft.AspNetCore.Components;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine($"public partial class {componentName} : ComponentBase");
        sb.AppendLine("{");

        // Add injected properties
        foreach (var (type, name) in injects)
        {
            sb.AppendLine($"    [Inject]");
            sb.AppendLine($"    public {type} {name} {{ get; set; }} = default!;");
            sb.AppendLine();
        }

        // Add code content
        if (!string.IsNullOrWhiteSpace(codeContent))
        {
            // Indent the code content
            var lines = codeContent.Split('\n');
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine($"    {line.TrimEnd()}");
                }
                else
                {
                    sb.AppendLine();
                }
            }
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private string GenerateModernizedCodeBehind(string namespaceName, string className, List<(string type, string name)> services, string originalContent)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("using Microsoft.AspNetCore.Components;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();

        // Generate primary constructor
        if (services.Any())
        {
            var parameters = string.Join(", ", services.Select(s => $"{s.type} {char.ToLower(s.name[0]) + s.name.Substring(1)}"));
            sb.AppendLine($"public partial class {className}({parameters}) : ComponentBase");
            sb.AppendLine("{");

            // Generate private readonly fields
            foreach (var (type, name) in services)
            {
                var fieldName = char.ToLower(name[0]) + name.Substring(1);
                sb.AppendLine($"    private readonly {type} {name} = {fieldName};");
            }

            sb.AppendLine();
        }
        else
        {
            sb.AppendLine($"public partial class {className} : ComponentBase");
            sb.AppendLine("{");
        }

        // Extract and add remaining methods and properties (excluding [Inject] properties)
        var methodPattern = @"((?:public|private|protected|internal)\s+(?:async\s+)?(?:Task<?\w*>?|void|\w+)\s+\w+\s*\([^)]*\)\s*(?:where\s+\w+\s*:\s*\w+)?\s*{(?:[^{}]|{(?:[^{}]|{[^{}]*})*})*})";
        var propertyPattern = @"((?:public|private|protected|internal)\s+\w+\s+\w+\s*{\s*get;\s*(?:set;|private\s+set;)?\s*})";
        
        // Remove [Inject] properties from content
        var cleanedContent = Regex.Replace(originalContent, @"\[Inject\]\s*(?:public\s+)?\S+\s+\S+\s*{\s*get;\s*set;\s*}", "");
        
        // Find all methods
        var methodMatches = Regex.Matches(cleanedContent, methodPattern);
        foreach (Match match in methodMatches)
        {
            var method = match.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(method))
            {
                sb.AppendLine($"    {method}");
                sb.AppendLine();
            }
        }

        // Find all non-inject properties
        var propertyMatches = Regex.Matches(cleanedContent, propertyPattern);
        foreach (Match match in propertyMatches)
        {
            var property = match.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(property) && !property.Contains("[Inject]"))
            {
                sb.AppendLine($"    {property}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private async Task<string> DetectNamespaceAsync(string componentPath, string content)
    {
        // 1. Check for @namespace directive in the component
        var namespacePattern = @"@namespace\s+(\S+)";
        var namespaceMatch = Regex.Match(content, namespacePattern);
        if (namespaceMatch.Success)
        {
            return namespaceMatch.Groups[1].Value;
        }

        // 2. Look for _Imports.razor files
        var directory = Path.GetDirectoryName(componentPath);
        while (!string.IsNullOrEmpty(directory))
        {
            var importsFile = Path.Combine(directory, "_Imports.razor");
            if (File.Exists(importsFile))
            {
                var importsContent = await File.ReadAllTextAsync(importsFile);
                var importsNamespaceMatch = Regex.Match(importsContent, namespacePattern);
                if (importsNamespaceMatch.Success)
                {
                    // Calculate relative namespace based on directory structure
                    var baseNamespace = importsNamespaceMatch.Groups[1].Value;
                    var relativePath = Path.GetRelativePath(directory, Path.GetDirectoryName(componentPath)!);
                    
                    if (relativePath != ".")
                    {
                        var additionalNamespace = relativePath.Replace(Path.DirectorySeparatorChar, '.');
                        return $"{baseNamespace}.{additionalNamespace}";
                    }
                    
                    return baseNamespace;
                }
            }
            directory = Path.GetDirectoryName(directory);
        }

        // 3. Try to infer from directory structure
        // Look for common patterns like "Components/Users" -> "ProjectName.Components.Users"
        var componentDir = Path.GetDirectoryName(componentPath);
        if (componentDir != null)
        {
            // Try to find project root by looking for .csproj files
            var searchDir = componentDir;
            string? projectName = null;
            
            while (!string.IsNullOrEmpty(searchDir))
            {
                var csprojFiles = Directory.GetFiles(searchDir, "*.csproj");
                if (csprojFiles.Length > 0)
                {
                    projectName = Path.GetFileNameWithoutExtension(csprojFiles[0]);
                    var relativePath = Path.GetRelativePath(searchDir, componentDir);
                    
                    if (relativePath != ".")
                    {
                        var namespaceParts = relativePath.Replace(Path.DirectorySeparatorChar, '.');
                        return $"{projectName}.{namespaceParts}";
                    }
                    
                    return projectName;
                }
                searchDir = Path.GetDirectoryName(searchDir);
            }
        }

        // 4. Default fallback
        return "MyApp.Components";
    }
}