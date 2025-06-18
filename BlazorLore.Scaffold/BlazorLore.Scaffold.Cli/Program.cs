using System.CommandLine;
using BlazorLore.Scaffold.Cli.Commands;


var rootCommand = new RootCommand("BlazorLore Scaffold CLI - Generate and refactor Blazor components with ease");

// Component command with subcommands
var componentCommand = new ComponentCommand();
rootCommand.AddCommand(componentCommand.GetCommand());

// Form command with subcommands
var formCommand = new FormCommand();
rootCommand.AddCommand(formCommand.GetCommand());

// Future entity commands can be added here:
// rootCommand.AddCommand(new ServiceCommand().GetCommand());
// rootCommand.AddCommand(new PageCommand().GetCommand());

return await rootCommand.InvokeAsync(args);