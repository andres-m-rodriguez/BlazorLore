using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using BlazorLore.Scaffold.Cli.Services;

namespace BlazorLore.Scaffold.Cli.Commands;

public class InitTemplatesCommand : Command
{
    public InitTemplatesCommand() : base("init-templates", "Initialize custom templates in the current project")
    {
        AddOption(new Option<string>(
            new[] { "--path", "-p" },
            getDefaultValue: () => ".blazor-templates",
            "Path where templates will be initialized"));

        Handler = CommandHandler.Create<string>(HandleCommand);
    }

    private async Task<int> HandleCommand(string path)
    {
        try
        {
            var service = new CustomTemplateService();
            await service.InitializeTemplatesAsync(path);
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error initializing templates: {ex.Message}");
            return 1;
        }
    }
}