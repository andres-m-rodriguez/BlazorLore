using System.CommandLine;
using System.Runtime.CompilerServices;
using BlazorLore.Scaffold.Cli.Commands;
using BlazorLore.Scaffold.Cli.Services;

// Demo mode if no arguments provided
if (args.Length == 0)
{
    Console.WriteLine("🚀 Running in demo mode...");
    Console.WriteLine();
    
    // Generate a test component
    var testComponentName = "TestComponent";
    var generator = new ComponentGenerator();
    
    Console.WriteLine($"1️⃣  Generating component '{testComponentName}' with inline @code block...");
    await generator.GenerateComponentAsync(testComponentName, "./", false, false);
    Console.WriteLine("   ✅ Component generated successfully!");
    Console.WriteLine();
    
    // Show the generated file
    var componentPath = $"./{testComponentName}.razor";
    Console.WriteLine("📄 Generated component content:");
    Console.WriteLine("================================");
    var content = await File.ReadAllTextAsync(componentPath);
    Console.WriteLine(content);
    Console.WriteLine("================================");
    Console.WriteLine();
    
    Console.WriteLine("Press any key to refactor this component to use code-behind...");
    Console.ReadKey();
    Console.WriteLine();
    
    // Refactor to extract code-behind
    Console.WriteLine("2️⃣  Refactoring component to extract @code block to partial class...");
    var refactorer = new ComponentRefactorer();
    await refactorer.ExtractCodeBehindAsync(componentPath);
    Console.WriteLine("   ✅ Code-behind extraction completed!");
    Console.WriteLine();
    
    // Show the refactored files
    Console.WriteLine("📄 Refactored component content:");
    Console.WriteLine("================================");
    content = await File.ReadAllTextAsync(componentPath);
    Console.WriteLine(content);
    Console.WriteLine("================================");
    Console.WriteLine();
    
    var codeBehindPath = $"./{testComponentName}.razor.cs";
    Console.WriteLine("📄 Generated code-behind file:");
    Console.WriteLine("================================");
    content = await File.ReadAllTextAsync(codeBehindPath);
    Console.WriteLine(content);
    Console.WriteLine("================================");
    Console.WriteLine();
    
    Console.WriteLine("✨ Demo completed! Files created:");
    Console.WriteLine($"   - {testComponentName}.razor");
    Console.WriteLine($"   - {testComponentName}.razor.cs");
    Console.WriteLine();
    
    Console.WriteLine("Press any key to modernize the code-behind file (convert [Inject] to constructor)...");
    Console.ReadKey();
    Console.WriteLine();
    
    // Modernize to use primary constructor
    Console.WriteLine("3️⃣  Modernizing code-behind to use primary constructor injection...");
    await refactorer.ConvertToConstructorInjectionAsync(codeBehindPath);
    Console.WriteLine("   ✅ Constructor injection conversion completed!");
    Console.WriteLine();
    
    Console.WriteLine("📄 Modernized code-behind file:");
    Console.WriteLine("================================");
    content = await File.ReadAllTextAsync(codeBehindPath);
    Console.WriteLine(content);
    Console.WriteLine("================================");
    
    return 0;
}

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