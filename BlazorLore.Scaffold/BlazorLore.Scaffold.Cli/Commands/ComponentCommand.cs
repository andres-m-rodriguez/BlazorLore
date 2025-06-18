using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using BlazorLore.Scaffold.Cli.Services;

namespace BlazorLore.Scaffold.Cli.Commands;

public class ComponentCommand : Command
{
    public ComponentCommand() : base("component", "Generate a new Blazor component")
    {
        // Support both old and new syntax
        AddArgument(new Argument<string>("name", "The name of the component"));
        
        AddOption(new Option<string>(
            new[] { "--output", "-o" },
            getDefaultValue: () => ".",
            "Output directory for the generated component"));
            
        AddOption(new Option<bool>(
            new[] { "--code-behind", "-c" },
            getDefaultValue: () => false,
            "Generate with code-behind file"));
            
        AddOption(new Option<bool>(
            new[] { "--css", "-s" },
            getDefaultValue: () => false,
            "Generate with CSS file"));
            
        AddOption(new Option<string?>(
            new[] { "--template", "-t" },
            "Use a custom template"));
            
        var varsOption = new Option<string?>(
            new[] { "--vars", "-v" },
            "Custom variables for template (format: key=value,key2=value2)");
        AddOption(varsOption);

        Handler = CommandHandler.Create<string, string, bool, bool, string?, string?>(HandleCommand);
    }

    private async Task<int> HandleCommand(
        string name,
        string output,
        bool codeBehind,
        bool css,
        string? template,
        string? vars)
    {
        try
        {
            if (!string.IsNullOrEmpty(template))
            {
                // Use custom template
                var customService = new CustomTemplateService();
                var variables = new Dictionary<string, object>
                {
                    ["name"] = name,
                    ["namespace"] = await DetectNamespaceAsync(output),
                    ["has_code_behind"] = codeBehind,
                    ["has_css"] = css,
                    ["timestamp"] = DateTime.Now,
                    ["user"] = Environment.UserName
                };

                // Parse and add custom variables
                if (!string.IsNullOrEmpty(vars))
                {
                    var customVars = new Dictionary<string, string>();
                    var varPairs = vars.Split(',');
                    foreach (var varPair in varPairs)
                    {
                        var parts = varPair.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            customVars[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                    variables["custom"] = customVars;
                }

                var handled = await customService.GenerateFromCustomTemplateAsync(template, output, variables);
                
                if (handled)
                {
                    Console.WriteLine($"✅ Component '{name}' generated from template '{template}'!");
                    return 0;
                }
            }

            // Use built-in generator
            var generator = new ComponentGenerator();
            await generator.GenerateComponentAsync(name, output, codeBehind, css);
            
            Console.WriteLine($"✅ Component '{name}' generated successfully!");
            if (codeBehind) Console.WriteLine($"   - Razor file: {name}.razor");
            if (codeBehind) Console.WriteLine($"   - Code-behind: {name}.razor.cs");
            if (css) Console.WriteLine($"   - Styles: {name}.razor.css");
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error generating component: {ex.Message}");
            return 1;
        }
    }

    private async Task<string> DetectNamespaceAsync(string outputPath)
    {
        // Similar logic to ComponentRefactorer.DetectNamespaceAsync
        var directory = new DirectoryInfo(outputPath);
        
        while (directory != null)
        {
            var csprojFiles = directory.GetFiles("*.csproj");
            if (csprojFiles.Length > 0)
            {
                var projectName = Path.GetFileNameWithoutExtension(csprojFiles[0].Name);
                var relativePath = Path.GetRelativePath(directory.FullName, outputPath);
                
                if (relativePath != ".")
                {
                    var namespaceParts = relativePath.Replace(Path.DirectorySeparatorChar, '.');
                    return await Task.FromResult($"{projectName}.{namespaceParts}");
                }
                
                return await Task.FromResult(projectName);
            }
            
            directory = directory.Parent;
        }

        return await Task.FromResult("MyApp.Components");
    }
}

// Keep the old interface-based approach for backwards compatibility
public class ComponentCommandLegacy : IEntityCommand
{
    public string EntityName => "component";
    public string Description => "Generate and manage Blazor components";

    public Command GetCommand()
    {
        var command = new Command("component", Description);

        // Generate subcommand
        var generateCommand = new Command("generate", "Generate a new Blazor component");
        var nameArgument = new Argument<string>("name", "The name of the component");
        var pathOption = new Option<string>("--path", () => "./", "The output path for the component");
        var codeOption = new Option<bool>("--code-behind", () => false, "Generate with code-behind file");
        var cssOption = new Option<bool>("--css", () => false, "Generate with CSS file");

        generateCommand.AddArgument(nameArgument);
        generateCommand.AddOption(pathOption);
        generateCommand.AddOption(codeOption);
        generateCommand.AddOption(cssOption);

        generateCommand.SetHandler(async (string name, string path, bool codeBehind, bool css) =>
        {
            var generator = new ComponentGenerator();
            await generator.GenerateComponentAsync(name, path, codeBehind, css);
            Console.WriteLine($"Component '{name}' generated successfully!");
        }, nameArgument, pathOption, codeOption, cssOption);

        // Refactor subcommand
        var refactorCommand = new Command("refactor", "Refactor an existing Blazor component");
        var fileArgument = new Argument<string>("file", "The component file to refactor");
        var extractOption = new Option<bool>("--extract-code", () => false, "Extract @code block to code-behind");

        refactorCommand.AddArgument(fileArgument);
        refactorCommand.AddOption(extractOption);

        refactorCommand.SetHandler(async (string file, bool extract) =>
        {
            if (extract)
            {
                var refactorer = new ComponentRefactorer();
                await refactorer.ExtractCodeBehindAsync(file);
                Console.WriteLine($"Code-behind extracted for '{file}' successfully!");
            }
            else
            {
                Console.WriteLine("Please specify a refactoring option (e.g., --extract-code)");
            }
        }, fileArgument, extractOption);

        // List subcommand
        var listCommand = new Command("list", "List all Blazor components in a directory");
        var dirOption = new Option<string>("--dir", () => "./", "The directory to search");

        listCommand.AddOption(dirOption);

        listCommand.SetHandler(async (string dir) =>
        {
            var razorFiles = Directory.GetFiles(dir, "*.razor", SearchOption.AllDirectories);
            Console.WriteLine($"Found {razorFiles.Length} components:");
            foreach (var file in razorFiles)
            {
                var relativePath = Path.GetRelativePath(dir, file);
                Console.WriteLine($"  - {relativePath}");
            }
            await Task.CompletedTask;
        }, dirOption);

        // Modernize subcommand
        var modernizeCommand = new Command("modernize", "Modernize component code-behind to use constructor injection");
        var modernizeFileArgument = new Argument<string>("file", "The code-behind file to modernize");

        modernizeCommand.AddArgument(modernizeFileArgument);

        modernizeCommand.SetHandler(async (string file) =>
        {
            var refactorer = new ComponentRefactorer();
            await refactorer.ConvertToConstructorInjectionAsync(file);
            Console.WriteLine($"Code-behind file '{file}' modernized successfully!");
        }, modernizeFileArgument);

        command.AddCommand(generateCommand);
        command.AddCommand(refactorCommand);
        command.AddCommand(listCommand);
        command.AddCommand(modernizeCommand);

        return command;
    }
}