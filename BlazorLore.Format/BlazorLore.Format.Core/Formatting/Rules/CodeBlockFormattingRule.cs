using BlazorLore.Format.Core.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using System.Text;

namespace BlazorLore.Format.Core.Formatting.Rules;

public class CodeBlockFormattingRule : IFormattingRule
{
    public string Name => "CodeBlockFormatting";
    public int Priority => 90;

    public bool CanApply(BlazorNode node, BlazorFormatterOptions options)
    {
        return node is CodeBlockNode;
    }

    public void Apply(BlazorNode node, FormattingContext context)
    {
        var codeBlock = (CodeBlockNode)node;
        
        switch (codeBlock.Type)
        {
            case CodeBlockType.Expression:
                FormatExpression(codeBlock, context);
                break;
            case CodeBlockType.CodeBlock:
                FormatCodeBlock(codeBlock, context);
                break;
            case CodeBlockType.Statement:
                FormatStatement(codeBlock, context);
                break;
            case CodeBlockType.Directive:
                if (codeBlock.Code.StartsWith("code "))
                {
                    FormatCodeDirective(codeBlock, context);
                }
                else
                {
                    FormatStatement(codeBlock, context);
                }
                break;
            case CodeBlockType.IfBlock:
            case CodeBlockType.ForeachBlock:
            case CodeBlockType.ElseBlock:
            case CodeBlockType.ElseIfBlock:
                // These are handled by their specific formatting rules
                break;
        }
    }

    private void FormatExpression(CodeBlockNode codeBlock, FormattingContext context)
    {
        context.Write($"@{codeBlock.Code}");
    }

    private void FormatStatement(CodeBlockNode codeBlock, FormattingContext context)
    {
        context.WriteLine($"@{codeBlock.Code}");
    }

    private void FormatCodeBlock(CodeBlockNode codeBlock, FormattingContext context)
    {
        context.WriteLine("@{");
        
        if (context.Options.FormatEmbeddedCSharp)
        {
            var formattedCode = FormatCSharpCode(codeBlock.Code, context);
            var lines = formattedCode.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            
            context.CurrentIndentLevel++;
            foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                context.WriteLine(line.TrimEnd());
            }
            context.CurrentIndentLevel--;
        }
        else
        {
            var lines = codeBlock.Code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            context.CurrentIndentLevel++;
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    context.WriteLine(line.Trim());
                }
            }
            context.CurrentIndentLevel--;
        }
        
        context.WriteLine("}");
    }

    private void FormatCodeDirective(CodeBlockNode codeBlock, FormattingContext context)
    {
        context.WriteLine();
        context.WriteLine("@code");
        context.WriteLine("{");
        
        if (context.Options.FormatEmbeddedCSharp)
        {
            var codeContent = codeBlock.Code.Substring(5).Trim(); // Remove "code " prefix and trim
            var formattedCode = FormatCSharpCode(codeContent, context);
            var lines = formattedCode.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            
            context.CurrentIndentLevel++;
            foreach (var line in lines)
            {
                // Write each line with proper indentation
                context.WriteLine(line.TrimEnd());
            }
            context.CurrentIndentLevel--;
        }
        else
        {
            var codeContent = codeBlock.Code.Substring(5); // Remove "code " prefix
            var lines = codeContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            context.CurrentIndentLevel++;
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    context.WriteLine(line.Trim());
                }
            }
            context.CurrentIndentLevel--;
        }
        
        context.WriteLine("}");
    }

    private string FormatCSharpCode(string code, FormattingContext context)
    {
        try
        {
            // Wrap the code in a class to ensure it's valid C# syntax
            var wrappedCode = $@"
using Microsoft.AspNetCore.Components;
public class Component : ComponentBase
{{
{code}
}}";
            
            var tree = CSharpSyntaxTree.ParseText(wrappedCode);
            var root = tree.GetRoot();
            
            var workspace = new AdhocWorkspace();
            var options = workspace.Options
                .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, context.Options.UseTabs)
                .WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, context.Options.IndentSize)
                .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, context.Options.IndentSize)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, true)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInTypes, true)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInProperties, true)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAccessors, true)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInControlBlocks, true);
            
            var formattedRoot = Formatter.Format(root, workspace, options);
            
            // Extract the formatted code from inside the class
            var classDeclaration = formattedRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var members = classDeclaration.Members;
            
            if (!members.Any())
            {
                return code;
            }
            
            var result = new StringBuilder();
            var isFirst = true;
            
            foreach (var member in members)
            {
                if (!isFirst)
                {
                    result.AppendLine();
                }
                isFirst = false;
                
                var memberText = member.ToString(); // Use ToString() instead of ToFullString() to avoid trivia
                var lines = memberText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                
                // Process each line, removing the class-level indentation
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    
                    // Skip completely empty lines at the start or end of a member
                    if (string.IsNullOrWhiteSpace(line) && (i == 0 || i == lines.Length - 1))
                    {
                        continue;
                    }
                    
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        // Preserve empty lines in the middle
                        result.AppendLine();
                    }
                    else
                    {
                        // Remove exactly 4 spaces of indentation that were added by the class wrapper
                        var trimmedLine = line;
                        if (line.StartsWith("    "))
                        {
                            trimmedLine = line.Substring(4);
                        }
                        else if (line.TrimStart() == line)
                        {
                            // Line has no indentation, keep as is
                        }
                        else
                        {
                            // Line has less than 4 spaces, remove what's there
                            trimmedLine = line.TrimStart();
                        }
                        
                        result.AppendLine(trimmedLine.TrimEnd());
                    }
                }
            }
            
            // Remove any trailing newlines
            var finalResult = result.ToString().TrimEnd('\r', '\n');
            return finalResult;
        }
        catch
        {
            // If formatting fails, return the original code
            return code;
        }
    }
}