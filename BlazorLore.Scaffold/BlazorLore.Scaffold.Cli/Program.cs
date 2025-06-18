using System.CommandLine;
using BlazorLore.Scaffold.Cli.Commands;


var rootCommand = new RootCommand("BlazorLore Scaffold CLI - Generate and refactor Blazor components with ease");

// Component command (simplified syntax)
rootCommand.AddCommand(new ComponentCommand());

// Form command (keeping the old syntax for now)
var formCommand = new FormCommand();
rootCommand.AddCommand(formCommand.GetCommand());

// Service command
rootCommand.AddCommand(new ServiceCommand());

// Refactor command
rootCommand.AddCommand(new RefactorCommand());

// Template management commands
rootCommand.AddCommand(new InitTemplatesCommand());
rootCommand.AddCommand(new ListTemplatesCommand());

// Future entity commands can be added here:
// rootCommand.AddCommand(new PageCommand());

return await rootCommand.InvokeAsync(args);