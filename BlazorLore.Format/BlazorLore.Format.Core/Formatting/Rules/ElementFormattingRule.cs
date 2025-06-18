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
        context.Write($"<{element.TagName}");
        FormatAttributes(element, context);
        context.Write(" />");
        context.FinishLine();
    }

    private void FormatOpeningTag(ElementNode element, FormattingContext context)
    {
        context.Write($"<{element.TagName}");
        var attributesOnNewLine = FormatAttributes(element, context);
        
        // Check if we have simple inline content
        var hasOnlyTextAndExpressions = element.Children.All(c => 
            c is TextNode || (c is CodeBlockNode cb && cb.Type == CodeBlockType.Expression));
        
        // Check if we should break content to new line
        var shouldBreakContent = ShouldBreakContent(element, context.Options, attributesOnNewLine) && 
                                !hasOnlyTextAndExpressions;
        
        if (shouldBreakContent || (HasBlockLevelChildren(element) && !hasOnlyTextAndExpressions))
        {
            context.WriteLine(">");
        }
        else
        {
            context.Write(">");
        }
    }

    private void FormatClosingTag(ElementNode element, FormattingContext context)
    {
        // Check if we have simple inline content
        var hasOnlyTextAndExpressions = element.Children.All(c => 
            c is TextNode || (c is CodeBlockNode cb && cb.Type == CodeBlockType.Expression));
            
        var shouldBreakContent = ShouldBreakContent(element, context.Options, false) && 
                                !hasOnlyTextAndExpressions;
        
        if ((HasBlockLevelChildren(element) || shouldBreakContent) && !hasOnlyTextAndExpressions)
        {
            context.WriteLine($"</{element.TagName}>");
        }
        else
        {
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

        // Check if element has simple inline content
        var hasOnlyTextAndExpressions = element.Children.All(c => 
            c is TextNode || (c is CodeBlockNode cb && cb.Type == CodeBlockType.Expression));
            
        var shouldBreakContent = ShouldBreakContent(element, context.Options, false);
        
        // Keep simple text content inline unless forced to break
        if (hasOnlyTextAndExpressions && !shouldBreakContent)
        {
            foreach (var child in element.Children)
            {
                if (child is TextNode textNode)
                {
                    context.Write(textNode.Content.Trim());
                }
                else if (child is CodeBlockNode codeBlock && codeBlock.Type == CodeBlockType.Expression)
                {
                    // Add space only if there's preceding text
                    if (element.Children.IndexOf(child) > 0)
                    {
                        context.Write(" ");
                    }
                    context.Write($"@{codeBlock.Code}");
                }
            }
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
            new CodeBlockFormattingRule().Apply(codeBlock, context);
        }
    }

    private bool HasBlockLevelChildren(ElementNode element)
    {
        return element.Children.Any(c => c is ElementNode || c is CodeBlockNode);
    }
}