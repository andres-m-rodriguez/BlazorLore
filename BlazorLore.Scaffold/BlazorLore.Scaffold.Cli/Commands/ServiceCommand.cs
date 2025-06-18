using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using BlazorLore.Scaffold.Cli.Services;

namespace BlazorLore.Scaffold.Cli.Commands;

public class ServiceCommand : Command
{
    public ServiceCommand() : base("service", "Generate a new service class")
    {
        AddArgument(new Argument<string>("name", "The name of the service (e.g., UserService)"));
        
        AddOption(new Option<string>(
            new[] { "--output", "-o" },
            getDefaultValue: () => ".",
            "Output directory for the generated service"));
            
        AddOption(new Option<bool>(
            new[] { "--interface", "-i" },
            getDefaultValue: () => true,
            "Generate an interface for the service"));
            
        AddOption(new Option<bool>(
            new[] { "--repository", "-r" },
            getDefaultValue: () => false,
            "Generate a repository pattern service with CRUD operations"));
            
        AddOption(new Option<string?>(
            new[] { "--entity", "-e" },
            "Entity name for repository pattern (e.g., User)"));
            
        AddOption(new Option<string>(
            new[] { "--id-type", "-t" },
            getDefaultValue: () => "int",
            "Entity ID type for repository pattern (int, Guid, string, etc.)"));
            
        AddOption(new Option<List<string>?>(
            new[] { "--dependencies", "-d" },
            "Additional constructor dependencies (format: Type:parameterName)"));

        Handler = CommandHandler.Create<string, string, bool, bool, string?, string, List<string>?>(HandleCommand);
    }

    private async Task<int> HandleCommand(
        string name,
        string output,
        bool @interface,
        bool repository,
        string? entity,
        string idType,
        List<string>? dependencies)
    {
        try
        {
            if (!name.EndsWith("Service"))
            {
                name += "Service";
            }

            // Parse dependencies
            var parsedDependencies = new List<(string type, string name)>();
            if (dependencies != null)
            {
                foreach (var dep in dependencies)
                {
                    var parts = dep.Split(':');
                    if (parts.Length == 2)
                    {
                        parsedDependencies.Add((parts[0], parts[1]));
                    }
                    else if (parts.Length == 1)
                    {
                        // Auto-generate parameter name from type
                        var paramName = char.ToLower(parts[0][0]) + parts[0].Substring(1);
                        if (parts[0].StartsWith("I") && parts[0].Length > 1)
                        {
                            paramName = char.ToLower(parts[0][1]) + parts[0].Substring(2);
                        }
                        parsedDependencies.Add((parts[0], paramName));
                    }
                }
            }

            var generator = new ServiceGenerator();
            await generator.GenerateServiceAsync(
                name,
                output,
                @interface,
                repository,
                entity,
                idType,
                parsedDependencies);

            Console.WriteLine($"✅ Service '{name}' generated successfully!");
            
            if (@interface)
            {
                Console.WriteLine($"   - Interface: I{name}.cs");
                Console.WriteLine($"   - Implementation: {name}.cs");
            }
            else
            {
                Console.WriteLine($"   - Service: {name}.cs");
            }

            if (repository)
            {
                Console.WriteLine($"   - Repository pattern with CRUD operations for '{entity ?? "Entity"}'");
            }

            Console.WriteLine($"\n💡 Don't forget to register your service in Program.cs:");
            if (@interface)
            {
                Console.WriteLine($"   builder.Services.AddScoped<I{name}, {name}>();");
            }
            else
            {
                Console.WriteLine($"   builder.Services.AddScoped<{name}>();");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error generating service: {ex.Message}");
            return 1;
        }
    }
}