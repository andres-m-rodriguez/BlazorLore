using System.CommandLine;

namespace BlazorLore.Scaffold.Cli.Commands;

public interface IEntityCommand
{
    Command GetCommand();
    string EntityName { get; }
    string Description { get; }
}