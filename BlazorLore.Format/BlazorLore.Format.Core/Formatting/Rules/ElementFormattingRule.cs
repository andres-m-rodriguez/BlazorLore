using BlazorLore.Format.Core.Parsing;

namespace BlazorLore.Format.Core.Formatting.Rules;

public class ElementFormattingRule : IFormattingRule
{
    public string Name => "ElementFormatting";
    public int Priority => 100;

    public bool CanApply(BlazorNode node, BlazorFormatterOptions options)
    {
        return node is ElementNode;
    }

    public void Apply(BlazorNode node, FormattingContext context)
    {
        var element = (ElementNode)node;
        
        if (element.IsSelfClosing)
        {
            FormatSelfClosingElement(element, context);
        }
        else
        {
            FormatOpeningTag(element, context);
            FormatChildren(element, context);
            FormatClosingTag(element, context);
        }
    }

    private void FormatSelfClosingElement(ElementNode element, FormattingContext context)
    {
        // Special handling for declarations like <!DOCTYPE html>
        if (element.TagName.StartsWith("<!"))
        {
            context.Write(element.TagName);
            context.FinishLine();
        }
        else
        {
            context.Write($"<{element.TagName}");
            FormatAttributes(element, context);
            context.Write(" />");
            context.FinishLine();
        }
    }

    private void FormatOpeningTag(ElementNode element, FormattingContext context)
    {
        context.Write($"<{element.TagName}");
        var attributesOnNewLine = FormatAttributes(element, context);
        
        // Always write ">" on the same line as the tag name for elements without attributes
        context.Write(">");
        
        // Determine if we need a line break after the opening tag
        var hasBlockContent = HasBlockLevelChildren(element);
        var hasOnlyWhitespace = element.Children.All(c => 
            c is TextNode textNode && string.IsNullOrWhiteSpace(textNode.Content));
            
        if (hasBlockContent && !hasOnlyWhitespace)
        {
            context.FinishLine();
        }
    }

    private void FormatClosingTag(ElementNode element, FormattingContext context)
    {
        // Check if we need the closing tag on a new line
        var hasBlockContent = HasBlockLevelChildren(element);
        var hasOnlyWhitespace = element.Children.All(c => 
            c is TextNode textNode && string.IsNullOrWhiteSpace(textNode.Content));
            
        if (hasBlockContent && !hasOnlyWhitespace)
        {
            // Closing tag on new line for elements with block content
            context.WriteLine($"</{element.TagName}>");
        }
        else
        {
            // Closing tag inline for empty elements or text-only content
            context.Write($"</{element.TagName}>");
            context.FinishLine();
        }
    }
    
    private bool ShouldBreakContent(ElementNode element, BlazorFormatterOptions options, bool attributesOnNewLine)
    {
        if (!options.BreakContentWithManyAttributes)
            return false;
        
        // Only break content if attributes were on new lines or we have many attributes
        if (attributesOnNewLine)
            return true;
            
        return element.Attributes.Count >= options.ContentBreakThreshold;
    }

    private bool FormatAttributes(ElementNode element, FormattingContext context)
    {
        if (!element.Attributes.Any())
            return false;

        var options = context.Options;
        var shouldMultiline = ShouldMultilineAttributes(element, options);

        if (shouldMultiline)
        {
            context.CurrentIndentLevel++;
            foreach (var attr in element.Attributes)
            {
                context.FinishLine();
                context.Write(FormatAttribute(attr, options));
            }
            context.CurrentIndentLevel--;
            
            // Don't add extra line before closing >
            if (!ShouldBreakContent(element, options, true))
            {
                context.FinishLine();
                context.Write(context.GetIndentation());
            }
            return true;
        }
        else
        {
            foreach (var attr in element.Attributes)
            {
                context.Write($" {FormatAttribute(attr, options)}");
            }
            return false;
        }
    }

    private bool ShouldMultilineAttributes(ElementNode element, BlazorFormatterOptions options)
    {
        return options.AttributeFormatting switch
        {
            AttributeFormatting.MultilineAlways => element.Attributes.Any(),
            AttributeFormatting.MultilineWhenMany => element.Attributes.Count >= options.AttributeBreakThreshold,
            _ => false
        };
    }

    private string FormatAttribute(AttributeNode attr, BlazorFormatterOptions options)
    {
        if (attr.Value == null)
            return attr.Name;

        var quote = options.QuoteStyle == QuoteStyle.Single ? "'" : "\"";
        return $"{attr.Name}={quote}{attr.Value}{quote}";
    }

    private void FormatChildren(ElementNode element, FormattingContext context)
    {
        if (!element.Children.Any())
            return;
            
        // Skip formatting if all children are just whitespace
        var hasOnlyWhitespace = element.Children.All(c => 
            c is TextNode textNode && string.IsNullOrWhiteSpace(textNode.Content));
        if (hasOnlyWhitespace)
            return;

        // Check if element has simple inline content
        var hasOnlyTextAndExpressions = element.Children.All(c => 
            c is TextNode || (c is CodeBlockNode cb && cb.Type == CodeBlockType.Expression));
            
        var shouldBreakContent = ShouldBreakContent(element, context.Options, false);
        
        // Keep simple text content inline unless forced to break
        if (hasOnlyTextAndExpressions && !shouldBreakContent && element.Children.Count == 1)
        {
            // Single child content can stay inline
            foreach (var child in element.Children)
            {
                if (child is TextNode textNode)
                {
                    context.Write(textNode.Content.Trim());
                }
                else if (child is CodeBlockNode codeBlock && codeBlock.Type == CodeBlockType.Expression)
                {
                    context.Write($"@{codeBlock.Code}");
                }
            }
        }
        else if (hasOnlyTextAndExpressions && !shouldBreakContent)
        {
            // Multiple children or mixed content - put on new line with indentation
            context.FinishLine();
            context.CurrentIndentLevel++;
            
            var contentParts = new List<string>();
            foreach (var child in element.Children)
            {
                if (child is TextNode textNode)
                {
                    var trimmed = textNode.Content.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                        contentParts.Add(trimmed);
                }
                else if (child is CodeBlockNode codeBlock && codeBlock.Type == CodeBlockType.Expression)
                {
                    contentParts.Add($"@{codeBlock.Code}");
                }
            }
            
            if (contentParts.Any())
            {
                // Join content parts without spaces to preserve expressions like $@variable
                var content = string.Empty;
                for (int i = 0; i < contentParts.Count; i++)
                {
                    content += contentParts[i];
                    // Add space between text parts but not before @ expressions
                    if (i < contentParts.Count - 1 && 
                        !contentParts[i + 1].StartsWith("@") && 
                        !contentParts[i].EndsWith("$"))
                    {
                        content += " ";
                    }
                }
                context.WriteLine(content);
            }
            
            context.CurrentIndentLevel--;
        }
        else if (HasBlockLevelChildren(element) || shouldBreakContent)
        {
            context.CurrentIndentLevel++;
            foreach (var child in element.Children)
            {
                FormatNode(child, context);
            }
            context.CurrentIndentLevel--;
        }
        else
        {
            context.CurrentIndentLevel++;
            foreach (var child in element.Children)
            {
                FormatNode(child, context);
            }
            context.CurrentIndentLevel--;
        }
    }

    private void FormatNode(BlazorNode node, FormattingContext context)
    {
        if (node is ElementNode elementNode)
        {
            new ElementFormattingRule().Apply(elementNode, context);
        }
        else if (node is TextNode textNode)
        {
            var trimmed = textNode.Content.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                context.WriteLine(trimmed);
            }
        }
        else if (node is CodeBlockNode codeBlock)
        {
            if (codeBlock.Type == CodeBlockType.IfBlock)
            {
                new IfBlockFormattingRule().Apply(codeBlock, context);
            }
            else if (codeBlock.Type == CodeBlockType.ForeachBlock)
            {
                new ForeachBlockFormattingRule().Apply(codeBlock, context);
            }
            else if (codeBlock.Type == CodeBlockType.ElseBlock || codeBlock.Type == CodeBlockType.ElseIfBlock)
            {
                new ElseBlockFormattingRule().Apply(codeBlock, context);
            }
            else
            {
                new CodeBlockFormattingRule().Apply(codeBlock, context);
            }
        }
    }

    private bool HasBlockLevelChildren(ElementNode element)
    {
        return element.Children.Any(c => c is ElementNode || c is CodeBlockNode);
    }
    
    private bool IsEmptyElement(ElementNode element)
    {
        if (!element.Children.Any())
            return true;
            
        // Check if all children are whitespace text nodes or empty elements
        return element.Children.All(c => 
            (c is TextNode textNode && string.IsNullOrWhiteSpace(textNode.Content)) ||
            (c is ElementNode childElement && IsEmptyElement(childElement)));
    }
}