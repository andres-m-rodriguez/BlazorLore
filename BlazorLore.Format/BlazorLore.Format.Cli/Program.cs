using System.CommandLine;
using BlazorLore.Format.Cli.Commands;

var rootCommand = new RootCommand("blazorfmt - A Prettier-like formatter for Blazor components")
{
    FormatCommand.Create(),
    InitCommand.Create(),
};

// Version handling is built-in for RootCommand
rootCommand.TreatUnmatchedTokensAsErrors = false;

// Handle root command (format by default if files are provided)
var fileArgument = new Argument<string[]>(
    name: "files",
    description: "Files to format (shorthand for 'format' command)",
    getDefaultValue: () => []
)
{
    Arity = ArgumentArity.ZeroOrMore,
};

rootCommand.AddArgument(fileArgument);

rootCommand.SetHandler(
    async (context) =>
    {
        var files = context.ParseResult.GetValueForArgument(fileArgument);

        // If files are provided without a command, treat it as format command
        if (files.Length > 0)
        {
            List<string> args = ["format"];
            args.AddRange(files);

            // Pass along any other options
            if (context.ParseResult.Tokens.Any(t => t.Value == "--write" || t.Value == "-w"))
                args.Add("--write");
            if (context.ParseResult.Tokens.Any(t => t.Value == "--check" || t.Value == "-c"))
                args.Add("--check");

            context.ExitCode = await rootCommand.InvokeAsync(args.ToArray());
            return;
        }

        // Otherwise show help
        context.ExitCode = 0;
        rootCommand.Invoke("--help");
    }
);

return await rootCommand.InvokeAsync(args);
