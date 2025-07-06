using BlazorLore.Format.Core.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
            var codeContent = codeBlock.Code.Substring(5); // Remove "code " prefix
            var formattedCode = FormatCSharpCode(codeContent, context);
            var lines = formattedCode.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            
            context.CurrentIndentLevel++;
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    context.WriteLine(line.TrimEnd());
                }
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
                .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, context.Options.IndentSize);
            
            var formattedRoot = Formatter.Format(root, workspace, options);
            
            // Extract the formatted code from inside the class
            var classDeclaration = formattedRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var members = classDeclaration.Members;
            
            var result = new StringBuilder();
            foreach (var member in members)
            {
                var memberText = member.ToFullString();
                // Remove extra indentation that was added by wrapping in a class
                var lines = memberText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                
                // Find the minimum indentation in the member (excluding empty lines)
                int minIndent = int.MaxValue;
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var leadingSpaces = line.TakeWhile(char.IsWhiteSpace).Count();
                        minIndent = Math.Min(minIndent, leadingSpaces);
                    }
                }
                
                // Remove the extra indentation from each line
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        result.AppendLine();
                    }
                    else
                    {
                        // Remove the minimum indentation found
                        var trimmedLine = line.Length > minIndent ? line.Substring(minIndent) : line.TrimStart();
                        result.AppendLine(trimmedLine.TrimEnd());
                    }
                }
                if (member != members.Last())
                {
                    result.AppendLine();
                }
            }
            
            return result.ToString().TrimEnd();
        }
        catch
        {
            // If formatting fails, return the original code
            return code;
        }
    }
}