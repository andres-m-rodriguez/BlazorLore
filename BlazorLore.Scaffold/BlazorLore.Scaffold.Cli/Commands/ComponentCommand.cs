using System.CommandLine;
using BlazorLore.Scaffold.Cli.Services;

namespace BlazorLore.Scaffold.Cli.Commands;

public class ComponentCommand : IEntityCommand
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