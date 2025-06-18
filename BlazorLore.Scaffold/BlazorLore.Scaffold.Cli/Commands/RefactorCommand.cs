using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using BlazorLore.Scaffold.Cli.Services;

namespace BlazorLore.Scaffold.Cli.Commands;

public class RefactorCommand : Command
{
    public RefactorCommand() : base("refactor", "Refactor existing Blazor components")
    {
        AddArgument(new Argument<string>("file", "The component file to refactor"));
        
        AddOption(new Option<bool>(
            new[] { "--extract-code", "-e" },
            getDefaultValue: () => false,
            "Extract @code block to code-behind file"));
            
        AddOption(new Option<bool>(
            new[] { "--modernize", "-m" },
            getDefaultValue: () => false,
            "Modernize code-behind to use constructor injection"));

        Handler = CommandHandler.Create<string, bool, bool>(HandleCommand);
    }

    private async Task<int> HandleCommand(string file, bool extractCode, bool modernize)
    {
        try
        {
            if (!extractCode && !modernize)
            {
                Console.WriteLine("❌ Please specify a refactoring option:");
                Console.WriteLine("   --extract-code (-e): Extract @code block to code-behind");
                Console.WriteLine("   --modernize (-m): Convert to constructor injection");
                return 1;
            }

            var refactorer = new ComponentRefactorer();

            if (extractCode)
            {
                if (!file.EndsWith(".razor"))
                {
                    Console.WriteLine("❌ Extract code requires a .razor file");
                    return 1;
                }

                await refactorer.ExtractCodeBehindAsync(file);
                Console.WriteLine($"✅ Code-behind extracted for '{Path.GetFileName(file)}'!");
                
                var codeBehindFile = file.Replace(".razor", ".razor.cs");
                Console.WriteLine($"   - Code-behind: {Path.GetFileName(codeBehindFile)}");
            }

            if (modernize)
            {
                if (!file.EndsWith(".razor.cs"))
                {
                    Console.WriteLine("❌ Modernize requires a .razor.cs file");
                    return 1;
                }

                await refactorer.ConvertToConstructorInjectionAsync(file);
                Console.WriteLine($"✅ Code-behind modernized for '{Path.GetFileName(file)}'!");
                Console.WriteLine($"   - Using constructor injection pattern");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error refactoring component: {ex.Message}");
            return 1;
        }
    }
}