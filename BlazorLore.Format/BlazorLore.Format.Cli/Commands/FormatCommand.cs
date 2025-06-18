using BlazorLore.Format.Cli.Configuration;
using BlazorLore.Format.Core;
using BlazorLore.Format.Core.Extensions;
using System.CommandLine;

namespace BlazorLore.Format.Cli.Commands;

public static class FormatCommand
{
    public static Command Create()
    {
        var fileArgument = new Argument<string[]>(
            name: "files",
            description: "Blazor/Razor files to format",
            getDefaultValue: () => Array.Empty<string>())
        {
            Arity = ArgumentArity.ZeroOrMore
        };

        var writeOption = new Option<bool>(
            aliases: new[] { "--write", "-w" },
            description: "Write formatted output back to files");

        var checkOption = new Option<bool>(
            aliases: new[] { "--check", "-c" },
            description: "Check if files are formatted (exit with error if not)");

        var configOption = new Option<string?>(
            aliases: new[] { "--config" },
            description: "Path to configuration file");

        var indentSizeOption = new Option<int?>(
            aliases: new[] { "--indent-size" },
            description: "Number of spaces for indentation");

        var useTabsOption = new Option<bool?>(
            aliases: new[] { "--use-tabs" },
            description: "Use tabs instead of spaces");

        var attributeBreakThresholdOption = new Option<int?>(
            aliases: new[] { "--attribute-break-threshold" },
            description: "Number of attributes before breaking to multiple lines");

        var contentBreakThresholdOption = new Option<int?>(
            aliases: new[] { "--content-break-threshold" },
            description: "Number of attributes before breaking content to new line");

        var breakContentWithManyAttributesOption = new Option<bool?>(
            aliases: new[] { "--break-content-with-many-attributes" },
            description: "Break content to new line when element has many attributes");

        var command = new Command("format", "Format Blazor/Razor files")
        {
            fileArgument,
            writeOption,
            checkOption,
            configOption,
            indentSizeOption,
            useTabsOption,
            attributeBreakThresholdOption,
            contentBreakThresholdOption,
            breakContentWithManyAttributesOption
        };

        command.SetHandler(async (context) =>
        {
            var files = context.ParseResult.GetValueForArgument(fileArgument);
            var write = context.ParseResult.GetValueForOption(writeOption);
            var check = context.ParseResult.GetValueForOption(checkOption);
            var configPath = context.ParseResult.GetValueForOption(configOption);
            var indentSize = context.ParseResult.GetValueForOption(indentSizeOption);
            var useTabs = context.ParseResult.GetValueForOption(useTabsOption);
            var attributeBreakThreshold = context.ParseResult.GetValueForOption(attributeBreakThresholdOption);
            var contentBreakThreshold = context.ParseResult.GetValueForOption(contentBreakThresholdOption);
            var breakContentWithManyAttributes = context.ParseResult.GetValueForOption(breakContentWithManyAttributesOption);

            var exitCode = await HandleFormatCommand(files, write, check, configPath, indentSize, useTabs,
                attributeBreakThreshold, contentBreakThreshold, breakContentWithManyAttributes);
            context.ExitCode = exitCode;
        });

        return command;
    }

    private static async Task<int> HandleFormatCommand(
        string[] files,
        bool write,
        bool check,
        string? configPath,
        int? indentSize,
        bool? useTabs,
        int? attributeBreakThreshold,
        int? contentBreakThreshold,
        bool? breakContentWithManyAttributes)
    {
        // Load configuration
        var options = ConfigurationLoader.LoadConfiguration(configPath);

        // Override with command line options
        if (indentSize.HasValue)
            options.IndentSize = indentSize.Value;
        if (useTabs.HasValue)
            options.UseTabs = useTabs.Value;
        if (attributeBreakThreshold.HasValue)
            options.AttributeBreakThreshold = attributeBreakThreshold.Value;
        if (contentBreakThreshold.HasValue)
            options.ContentBreakThreshold = contentBreakThreshold.Value;
        if (breakContentWithManyAttributes.HasValue)
            options.BreakContentWithManyAttributes = breakContentWithManyAttributes.Value;

        var formatter = new BlazorFormatter();
        
        // Check if reading from stdin
        bool readFromStdin = files.Length == 1 && files[0] == "-";
        
        if (readFromStdin)
        {
            // Read from stdin
            using var reader = new StreamReader(Console.OpenStandardInput());
            var content = await reader.ReadToEndAsync();
            var formatted = formatter.Format(content, options);
            
            // Write to stdout
            Console.Write(formatted);
            return 0;
        }
        
        // If no files specified, find all .razor files in current directory
        if (files.Length == 0)
        {
            files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.razor", SearchOption.AllDirectories)
                .Union(Directory.GetFiles(Directory.GetCurrentDirectory(), "*.cshtml", SearchOption.AllDirectories))
                .ToArray();
        }

        var hasChanges = false;
        var errorCount = 0;

        foreach (var file in files)
        {
            try
            {
                if (!File.Exists(file))
                {
                    Console.Error.WriteLine($"File not found: {file}");
                    errorCount++;
                    continue;
                }

                var content = await File.ReadAllTextAsync(file);
                var formatted = formatter.Format(content, options);

                if (content != formatted)
                {
                    hasChanges = true;

                    if (check)
                    {
                        Console.WriteLine($"✗ {file} - needs formatting");
                    }
                    else if (write)
                    {
                        await File.WriteAllTextAsync(file, formatted);
                        Console.WriteLine($"✓ {file} - formatted");
                    }
                    else
                    {
                        Console.WriteLine($"=== {file} ===");
                        Console.WriteLine(formatted);
                        Console.WriteLine();
                    }
                }
                else if (!check)
                {
                    Console.WriteLine($"✓ {file} - already formatted");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error formatting {file}: {ex.Message}");
                errorCount++;
            }
        }

        if (check && hasChanges)
        {
            Console.Error.WriteLine("\nSome files need formatting. Run with --write to fix.");
            return 1;
        }

        return errorCount > 0 ? 1 : 0;
    }
}