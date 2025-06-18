using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using BlazorLore.Scaffold.Cli.Services;

namespace BlazorLore.Scaffold.Cli.Commands;

public class ListTemplatesCommand : Command
{
    public ListTemplatesCommand() : base("list-templates", "List all available templates")
    {
        AddOption(new Option<string?>(
            new[] { "--category", "-c" },
            "Filter templates by category (component, form, service)"));

        AddOption(new Option<bool>(
            new[] { "--custom-only" },
            getDefaultValue: () => false,
            "Show only custom templates"));

        Handler = CommandHandler.Create<string?, bool>(HandleCommand);
    }

    private async Task<int> HandleCommand(string? category, bool customOnly)
    {
        try
        {
            var service = new CustomTemplateService();
            var templates = await service.DiscoverTemplatesAsync(category);

            if (customOnly)
            {
                templates = templates.Where(t => !t.IsBuiltIn).ToList();
            }

            if (!templates.Any())
            {
                Console.WriteLine("No templates found.");
                Console.WriteLine("\n💡 Run 'blazor-scaffold init-templates' to create custom templates.");
                return 0;
            }

            Console.WriteLine("\n📋 Available Templates:\n");

            var grouped = templates.GroupBy(t => t.Category).OrderBy(g => g.Key);
            
            foreach (var group in grouped)
            {
                Console.WriteLine($"  {group.Key.ToUpper()}:");
                
                foreach (var template in group.OrderBy(t => t.Name))
                {
                    var marker = template.IsBuiltIn ? "🔧" : "⭐";
                    Console.WriteLine($"    {marker} {template.Name,-20} - {template.Description}");
                }
                
                Console.WriteLine();
            }

            Console.WriteLine("💡 Usage:");
            Console.WriteLine("   blazor-scaffold component MyComponent --template <template-name>");
            Console.WriteLine("   blazor-scaffold service MyService --template <template-name>");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error listing templates: {ex.Message}");
            return 1;
        }
    }
}