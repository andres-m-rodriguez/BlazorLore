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
            // Look for both @ symbols and < symbols
            var atIndex = content.IndexOf('@', position);
            var tagStartIndex = content.IndexOf('<', position);
            
            // Determine which comes first
            int nextIndex;
            bool isRazor = false;
            
            if (atIndex == -1 && tagStartIndex == -1)
            {
                // No more tags or razor expressions, add remaining content as text
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
            else if (atIndex == -1)
            {
                nextIndex = tagStartIndex;
            }
            else if (tagStartIndex == -1)
            {
                nextIndex = atIndex;
                isRazor = true;
            }
            else
            {
                nextIndex = Math.Min(atIndex, tagStartIndex);
                isRazor = (nextIndex == atIndex);
            }

            // Add text before the next element
            if (nextIndex > position)
            {
                var textBefore = content.Substring(position, nextIndex - position);
                if (!string.IsNullOrWhiteSpace(textBefore))
                {
                    nodes.Add(new TextNode
                    {
                        Content = textBefore,
                        StartPosition = position,
                        EndPosition = nextIndex
                    });
                }
            }

            if (isRazor)
            {
                // Handle Razor expressions
                var codeBlockMatch = CodeBlockRegex.Match(content, nextIndex);
                var expressionMatch = ExpressionRegex.Match(content, nextIndex);
                var directiveMatch = DirectiveRegex.Match(content, nextIndex);
                var ifBlockMatch = IfBlockRegex.Match(content, nextIndex);

                var razorMatches = new[]
                {
                    (match: directiveMatch, type: "directive"),
                    (match: ifBlockMatch, type: "ifblock"),
                    (match: codeBlockMatch, type: "codeblock"),
                    (match: expressionMatch, type: "expression")
                }
                .Where(m => m.match.Success && m.match.Index == nextIndex)
                .OrderBy(m => m.match.Index)
                .FirstOrDefault();

                if (razorMatches.match != null && razorMatches.match.Success)
                {
                    switch (razorMatches.type)
                    {
                        case "directive":
                            nodes.Add(new CodeBlockNode
                            {
                                Code = $"{razorMatches.match.Groups["directive"].Value} {razorMatches.match.Groups["value"].Value}",
                                Type = CodeBlockType.Directive,
                                StartPosition = razorMatches.match.Index,
                                EndPosition = razorMatches.match.Index + razorMatches.match.Length
                            });
                            position = razorMatches.match.Index + razorMatches.match.Length;
                            continue;
                            
                        case "codeblock":
                            nodes.Add(new CodeBlockNode
                            {
                                Code = razorMatches.match.Groups["code"].Value,
                                Type = CodeBlockType.CodeBlock,
                                StartPosition = razorMatches.match.Index,
                                EndPosition = razorMatches.match.Index + razorMatches.match.Length
                            });
                            position = razorMatches.match.Index + razorMatches.match.Length;
                            continue;

                        case "expression":
                            nodes.Add(new CodeBlockNode
                            {
                                Code = razorMatches.match.Groups["expr"].Value,
                                Type = CodeBlockType.Expression,
                                StartPosition = razorMatches.match.Index,
                                EndPosition = razorMatches.match.Index + razorMatches.match.Length
                            });
                            position = razorMatches.match.Index + razorMatches.match.Length;
                            continue;
                            
                        case "ifblock":
                            var ifStart = razorMatches.match.Index;
                            var braceStart = content.IndexOf('{', ifStart);
                            if (braceStart != -1)
                            {
                                var braceEnd = FindMatchingBrace(content, braceStart);
                                if (braceEnd != -1)
                                {
                                    var ifContent = content.Substring(braceStart + 1, braceEnd - braceStart - 1);
                                    var ifCondition = razorMatches.match.Groups["condition"].Value;
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
                }
                else
                {
                    // Couldn't parse as Razor expression, treat @ as text
                    nodes.Add(new TextNode
                    {
                        Content = "@",
                        StartPosition = nextIndex,
                        EndPosition = nextIndex + 1
                    });
                    position = nextIndex + 1;
                }
            }
            else
            {
                // Try to parse element manually
                var elementResult = ParseElementManually(content, tagStartIndex);
                if (elementResult != null)
                {
                    nodes.Add(elementResult.Element);
                    position = elementResult.EndPosition;
                }
                else
                {
                    // Couldn't parse as element, treat as text
                    nodes.Add(new TextNode
                    {
                        Content = content[tagStartIndex].ToString(),
                        StartPosition = tagStartIndex,
                        EndPosition = tagStartIndex + 1
                    });
                    position = tagStartIndex + 1;
                }
            }
        }

        return nodes;
    }

    private class ElementParseResult
    {
        public ElementNode Element { get; set; }
        public int EndPosition { get; set; }
    }

    private ElementParseResult? ParseElementManually(string content, int startPosition)
    {
        if (startPosition >= content.Length || content[startPosition] != '<')
            return null;

        var position = startPosition + 1;

        // Skip whitespace after <
        while (position < content.Length && char.IsWhiteSpace(content[position]))
            position++;

        // Get tag name
        var tagNameStart = position;
        while (position < content.Length && (char.IsLetterOrDigit(content[position]) || content[position] == '-' || content[position] == '_'))
            position++;

        if (position == tagNameStart)
            return null; // No tag name found

        var tagName = content.Substring(tagNameStart, position - tagNameStart);

        // Parse attributes
        var attributes = new List<AttributeNode>();
        var tagEndPosition = position;
        var isSelfClosing = false;

        // Find the end of the opening tag
        while (position < content.Length)
        {
            // Skip whitespace
            while (position < content.Length && char.IsWhiteSpace(content[position]))
                position++;

            if (position >= content.Length)
                return null;

            // Check for end of tag
            if (content[position] == '>')
            {
                tagEndPosition = position + 1;
                break;
            }
            else if (position + 1 < content.Length && content[position] == '/' && content[position + 1] == '>')
            {
                isSelfClosing = true;
                tagEndPosition = position + 2;
                break;
            }
            else
            {
                // Parse attribute
                var attrResult = ParseAttributeManually(content, position);
                if (attrResult != null)
                {
                    attributes.AddRange(attrResult.Attributes);
                    position = attrResult.EndPosition;
                }
                else
                {
                    position++; // Skip unknown character
                }
            }
        }

        var element = new ElementNode
        {
            TagName = tagName,
            IsSelfClosing = isSelfClosing,
            StartPosition = startPosition,
            Attributes = attributes,
            Children = new List<BlazorNode>()
        };

        if (isSelfClosing)
        {
            element.EndPosition = tagEndPosition;
            return new ElementParseResult { Element = element, EndPosition = tagEndPosition };
        }

        // Parse content and find closing tag
        var contentStart = tagEndPosition;
        var closingTag = $"</{tagName}>";
        var depth = 1;
        var searchPosition = contentStart;

        // Look for matching closing tag, accounting for nested elements of the same type
        while (searchPosition < content.Length && depth > 0)
        {
            var nextOpenTag = content.IndexOf($"<{tagName}", searchPosition);
            var nextCloseTag = content.IndexOf(closingTag, searchPosition);

            if (nextCloseTag == -1)
                return null; // No closing tag found

            // Check if there's another opening tag before the closing tag
            if (nextOpenTag != -1 && nextOpenTag < nextCloseTag)
            {
                // Make sure it's actually a tag (followed by space, >, or /)
                var charAfterTagIndex = nextOpenTag + 1 + tagName.Length;
                if (charAfterTagIndex < content.Length)
                {
                    var nextChar = content[charAfterTagIndex];
                    if (char.IsWhiteSpace(nextChar) || nextChar == '>' || nextChar == '/')
                    {
                        depth++;
                        searchPosition = nextOpenTag + tagName.Length + 1;
                        continue;
                    }
                }
            }

            if (nextCloseTag != -1)
            {
                depth--;
                if (depth == 0)
                {
                    var innerContent = content.Substring(contentStart, nextCloseTag - contentStart);
                    element.Children = ParseContent(innerContent);
                    element.EndPosition = nextCloseTag + closingTag.Length;
                    return new ElementParseResult { Element = element, EndPosition = nextCloseTag + closingTag.Length };
                }
                searchPosition = nextCloseTag + closingTag.Length;
            }
        }

        return null; // Couldn't find matching closing tag
    }

    private class AttributeParseResult
    {
        public List<AttributeNode> Attributes { get; set; } = new List<AttributeNode>();
        public int EndPosition { get; set; }
    }

    private AttributeParseResult? ParseAttributeManually(string content, int startPosition)
    {
        var position = startPosition;
        var result = new AttributeParseResult();

        // Skip whitespace
        while (position < content.Length && char.IsWhiteSpace(content[position]))
            position++;

        if (position >= content.Length)
            return null;

        // Get attribute name
        var nameStart = position;
        while (position < content.Length && 
               (char.IsLetterOrDigit(content[position]) || 
                content[position] == '-' || 
                content[position] == '_' || 
                content[position] == ':' ||
                content[position] == '@'))
        {
            position++;
        }

        if (position == nameStart)
            return null;

        var attrName = content.Substring(nameStart, position - nameStart);

        // Skip whitespace after name
        while (position < content.Length && char.IsWhiteSpace(content[position]))
            position++;

        // Check for = sign
        if (position < content.Length && content[position] == '=')
        {
            position++; // Skip =

            // Skip whitespace after =
            while (position < content.Length && char.IsWhiteSpace(content[position]))
                position++;

            // Get quote character
            if (position < content.Length && (content[position] == '"' || content[position] == '\''))
            {
                var quoteChar = content[position];
                position++; // Skip opening quote

                var valueStart = position;
                while (position < content.Length && content[position] != quoteChar)
                    position++;

                if (position < content.Length)
                {
                    var attrValue = content.Substring(valueStart, position - valueStart);
                    position++; // Skip closing quote

                    result.Attributes.Add(new AttributeNode
                    {
                        Name = attrName,
                        Value = attrValue,
                        IsDirective = attrName.StartsWith("@")
                    });
                    result.EndPosition = position;
                    return result;
                }
            }
        }
        else
        {
            // Boolean attribute
            result.Attributes.Add(new AttributeNode
            {
                Name = attrName,
                Value = null,
                IsDirective = attrName.StartsWith("@")
            });
            result.EndPosition = position;
            return result;
        }

        return null;
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