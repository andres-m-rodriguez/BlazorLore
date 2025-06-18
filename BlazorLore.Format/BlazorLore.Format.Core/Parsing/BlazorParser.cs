using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using System.Text;
using System.Text.RegularExpressions;

namespace BlazorLore.Format.Core.Parsing;

public class BlazorParser : IBlazorParser
{
    private readonly RazorProjectEngine _projectEngine;

    public BlazorParser()
    {
        var fileSystem = RazorProjectFileSystem.Create(".");
        var projectEngine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            fileSystem,
            builder =>
            {
                builder.SetRootNamespace("BlazorLore.Format");
            });
        
        _projectEngine = projectEngine;
    }

    public RazorSyntaxTree Parse(string razorContent)
    {
        var sourceDocument = RazorSourceDocument.Create(razorContent, "Component.razor");
        var codeDocument = _projectEngine.Process(sourceDocument, "Component", Array.Empty<RazorSourceDocument>(), Array.Empty<TagHelperDescriptor>());
        return codeDocument.GetSyntaxTree();
    }

    public BlazorDocument ParseDocument(string razorContent)
    {
        var document = new BlazorDocument
        {
            OriginalContent = razorContent
        };

        var parser = new SimpleRazorParser();
        document.Nodes = parser.Parse(razorContent);

        return document;
    }
}

public class SimpleRazorParser
{
    private static readonly Regex ElementRegex = new(@"<(?<tag>\w+)(?<attrs>[^>]*)>(?<content>.*?)</\k<tag>>|<(?<selfclosing>\w+)(?<selfattrs>[^>]*)/?>", RegexOptions.Singleline);
    private static readonly Regex AttributeRegex = new(@"(?<name>@?[\w-]+(?::\w+)?)\s*=\s*[""'](?<value>[^""']*)[""']|(?<boolname>@?[\w-]+(?::\w+)?)(?=\s|>|/>)", RegexOptions.IgnoreCase);
    private static readonly Regex CodeBlockRegex = new(@"@\{(?<code>.*?)\}", RegexOptions.Singleline);
    private static readonly Regex ExpressionRegex = new(@"@(?!code\s*\{)(?<expr>[a-zA-Z_]\w*(?:\.\w+)*(?:\([^)]*\))?)", RegexOptions.Singleline);
    private static readonly Regex CodeDirectiveRegex = new(@"@code\s*\{(?<code>(?:[^{}]|\{(?:[^{}]|\{[^{}]*\})*\})*)\}", RegexOptions.Singleline);
    private static readonly Regex DirectiveRegex = new(@"@(?<directive>page|using|inject|inherits|layout|implements)\s+(?<value>.*?)$", RegexOptions.Multiline);
    private static readonly Regex IfBlockRegex = new(@"@if\s*\((?<condition>[^)]+)\)\s*\{(?<content>.*?)\}(?=\s*(@|$))", RegexOptions.Singleline);

    public List<BlazorNode> Parse(string content)
    {
        var nodes = new List<BlazorNode>();
        
        // First, check for @code directive at the end
        var codeDirectiveMatch = CodeDirectiveRegex.Match(content);
        if (codeDirectiveMatch.Success)
        {
            // Parse everything before @code
            var beforeCode = content.Substring(0, codeDirectiveMatch.Index);
            nodes.AddRange(ParseContent(beforeCode));
            
            // Add the @code block - note we need to include "@code" in the Code property
            nodes.Add(new CodeBlockNode
            {
                Code = "code " + codeDirectiveMatch.Groups["code"].Value.Trim(),
                Type = CodeBlockType.Directive,
                StartPosition = codeDirectiveMatch.Index,
                EndPosition = codeDirectiveMatch.Index + codeDirectiveMatch.Length
            });
        }
        else
        {
            nodes.AddRange(ParseContent(content));
        }
        
        return nodes;
    }
    
    public List<BlazorNode> ParseContent(string content)
    {
        var nodes = new List<BlazorNode>();
        var position = 0;

        while (position < content.Length)
        {
            var codeBlockMatch = CodeBlockRegex.Match(content, position);
            var expressionMatch = ExpressionRegex.Match(content, position);
            var elementMatch = ElementRegex.Match(content, position);
            var directiveMatch = DirectiveRegex.Match(content, position);
            var ifBlockMatch = IfBlockRegex.Match(content, position);

            var matches = new[]
            {
                (match: directiveMatch, type: "directive"),
                (match: ifBlockMatch, type: "ifblock"),
                (match: codeBlockMatch, type: "codeblock"),
                (match: expressionMatch, type: "expression"),
                (match: elementMatch, type: "element")
            }
            .Where(m => m.match.Success && m.match.Index >= position)
            .OrderBy(m => m.match.Index)
            .FirstOrDefault();

            if (matches.match == null || !matches.match.Success)
            {
                var remainingText = content.Substring(position);
                if (!string.IsNullOrWhiteSpace(remainingText))
                {
                    nodes.Add(new TextNode
                    {
                        Content = remainingText,
                        StartPosition = position,
                        EndPosition = content.Length
                    });
                }
                break;
            }

            if (matches.match.Index > position)
            {
                var textBefore = content.Substring(position, matches.match.Index - position);
                if (!string.IsNullOrWhiteSpace(textBefore))
                {
                    nodes.Add(new TextNode
                    {
                        Content = textBefore,
                        StartPosition = position,
                        EndPosition = matches.match.Index
                    });
                }
            }

            switch (matches.type)
            {
                case "directive":
                    nodes.Add(new CodeBlockNode
                    {
                        Code = $"{matches.match.Groups["directive"].Value} {matches.match.Groups["value"].Value}",
                        Type = CodeBlockType.Directive,
                        StartPosition = matches.match.Index,
                        EndPosition = matches.match.Index + matches.match.Length
                    });
                    break;
                    
                case "codeblock":
                    nodes.Add(new CodeBlockNode
                    {
                        Code = matches.match.Groups["code"].Value,
                        Type = CodeBlockType.CodeBlock,
                        StartPosition = matches.match.Index,
                        EndPosition = matches.match.Index + matches.match.Length
                    });
                    break;

                case "expression":
                    nodes.Add(new CodeBlockNode
                    {
                        Code = matches.match.Groups["expr"].Value,
                        Type = CodeBlockType.Expression,
                        StartPosition = matches.match.Index,
                        EndPosition = matches.match.Index + matches.match.Length
                    });
                    break;

                case "element":
                    var elementNode = ParseElement(matches.match, content);
                    if (elementNode != null)
                    {
                        nodes.Add(elementNode);
                    }
                    break;
                    
                case "ifblock":
                    // Find the matching closing brace
                    var ifStart = matches.match.Index;
                    var braceStart = content.IndexOf('{', ifStart);
                    if (braceStart != -1)
                    {
                        var braceEnd = FindMatchingBrace(content, braceStart);
                        if (braceEnd != -1)
                        {
                            var ifContent = content.Substring(braceStart + 1, braceEnd - braceStart - 1);
                            var ifCondition = matches.match.Groups["condition"].Value;
                            nodes.Add(new CodeBlockNode
                            {
                                Code = $"if({ifCondition})\n{{\n{ifContent}\n}}",
                                Type = CodeBlockType.IfBlock,
                                StartPosition = ifStart,
                                EndPosition = braceEnd + 1
                            });
                            position = braceEnd + 1;
                            continue;
                        }
                    }
                    break;
            }

            position = matches.match.Index + matches.match.Length;
        }

        return nodes;
    }

    private ElementNode? ParseElement(Match match, string content)
    {
        var tagName = match.Groups["tag"].Success ? match.Groups["tag"].Value : match.Groups["selfclosing"].Value;
        var attrsText = match.Groups["attrs"].Success ? match.Groups["attrs"].Value : match.Groups["selfattrs"].Value;
        var isSelfClosing = match.Groups["selfclosing"].Success;

        var element = new ElementNode
        {
            TagName = tagName,
            IsSelfClosing = isSelfClosing,
            StartPosition = match.Index,
            EndPosition = match.Index + match.Length
        };

        element.Attributes = ParseAttributes(attrsText);

        if (!isSelfClosing && match.Groups["content"].Success)
        {
            var innerContent = match.Groups["content"].Value;
            element.Children = ParseContent(innerContent);
        }

        return element;
    }

    private List<AttributeNode> ParseAttributes(string attrsText)
    {
        var attributes = new List<AttributeNode>();
        var matches = AttributeRegex.Matches(attrsText);

        foreach (Match match in matches)
        {
            if (match.Groups["name"].Success)
            {
                attributes.Add(new AttributeNode
                {
                    Name = match.Groups["name"].Value,
                    Value = match.Groups["value"].Value,
                    IsDirective = match.Groups["name"].Value.StartsWith("@")
                });
            }
            else if (match.Groups["boolname"].Success)
            {
                attributes.Add(new AttributeNode
                {
                    Name = match.Groups["boolname"].Value,
                    Value = null,
                    IsDirective = match.Groups["boolname"].Value.StartsWith("@")
                });
            }
        }

        return attributes;
    }
    
    private int FindMatchingBrace(string content, int openBraceIndex)
    {
        var depth = 1;
        var index = openBraceIndex + 1;
        
        while (index < content.Length && depth > 0)
        {
            if (content[index] == '{')
                depth++;
            else if (content[index] == '}')
                depth--;
                
            if (depth == 0)
                return index;
                
            index++;
        }
        
        return -1;
    }
}