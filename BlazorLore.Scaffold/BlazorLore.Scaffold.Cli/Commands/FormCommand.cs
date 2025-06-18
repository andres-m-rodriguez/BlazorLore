using System.CommandLine;
using BlazorLore.Scaffold.Cli.Services;

namespace BlazorLore.Scaffold.Cli.Commands;

public class FormCommand : IEntityCommand
{
    public string EntityName => "form";
    public string Description => "Generate forms from models";

    public Command GetCommand()
    {
        var command = new Command("form", Description);

        // Generate subcommand
        var generateCommand = new Command("generate", "Generate a form from a model");
        var modelArgument = new Argument<string>("model", "The model file to generate form from");
        var nameOption = new Option<string>("--name", "The name of the form component (defaults to {Model}Form)");
        var pathOption = new Option<string>("--path", () => "./", "The output path for the form");
        var editOption = new Option<bool>("--edit", () => false, "Generate as edit form with existing data");
        var submitOption = new Option<string>("--submit-action", () => "OnSubmit", "The method name for form submission");

        generateCommand.AddArgument(modelArgument);
        generateCommand.AddOption(nameOption);
        generateCommand.AddOption(pathOption);
        generateCommand.AddOption(editOption);
        generateCommand.AddOption(submitOption);

        generateCommand.SetHandler(async (string model, string? name, string path, bool edit, string submitAction) =>
        {
            var analyzer = new ModelAnalyzer();
            var generator = new FormGenerator();

            try
            {
                var modelInfo = await analyzer.AnalyzeModelAsync(model);
                var formName = name ?? $"{modelInfo.Name}Form";
                
                await generator.GenerateFormAsync(modelInfo, formName, path, edit, submitAction);
                
                Console.WriteLine($"Form '{formName}' generated successfully from model '{modelInfo.Name}'!");
                Console.WriteLine($"Properties found: {modelInfo.Properties.Count}");
                
                if (modelInfo.Properties.Any(p => p.ValidationAttributes.Any()))
                {
                    Console.WriteLine("Validation attributes were detected and included in the form.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating form: {ex.Message}");
            }
        }, modelArgument, nameOption!, pathOption, editOption, submitOption);

        command.AddCommand(generateCommand);

        return command;
    }
}