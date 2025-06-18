using BlazorLore.Format.Cli.Configuration;
using BlazorLore.Format.Core;
using System.CommandLine;

namespace BlazorLore.Format.Cli.Commands;

public static class InitCommand
{
    public static Command Create()
    {
        var forceOption = new Option<bool>(
            aliases: new[] { "--force", "-f" },
            description: "Overwrite existing configuration file");

        var command = new Command("init", "Initialize a new configuration file")
        {
            forceOption
        };

        command.SetHandler(async (context) =>
        {
            var force = context.ParseResult.GetValueForOption(forceOption);
            var exitCode = await HandleInitCommand(force);
            context.ExitCode = exitCode;
        });

        return command;
    }

    private static Task<int> HandleInitCommand(bool force)
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), ".blazorfmt.json");

        if (File.Exists(configPath) && !force)
        {
            Console.Error.WriteLine($"Configuration file already exists: {configPath}");
            Console.Error.WriteLine("Use --force to overwrite.");
            return Task.FromResult(1);
        }

        var defaultOptions = new BlazorFormatterOptions();
        ConfigurationLoader.SaveConfiguration(defaultOptions, configPath);

        Console.WriteLine($"✓ Created configuration file: {configPath}");
        Console.WriteLine("\nDefault configuration:");
        Console.WriteLine($"  Indent Size: {defaultOptions.IndentSize}");
        Console.WriteLine($"  Use Tabs: {defaultOptions.UseTabs}");
        Console.WriteLine($"  Max Line Length: {defaultOptions.MaxLineLength}");
        Console.WriteLine($"  Attribute Formatting: {defaultOptions.AttributeFormatting}");
        Console.WriteLine($"  Quote Style: {defaultOptions.QuoteStyle}");
        Console.WriteLine("\nEdit the file to customize your formatting preferences.");

        return Task.FromResult(0);
    }
}