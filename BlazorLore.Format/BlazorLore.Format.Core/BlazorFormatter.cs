using BlazorLore.Format.Core.Formatting;
using BlazorLore.Format.Core.Formatting.Rules;
using BlazorLore.Format.Core.Parsing;

namespace BlazorLore.Format.Core;

public class BlazorFormatter : IBlazorFormatter
{
    private readonly IBlazorParser _parser;
    private readonly List<IFormattingRule> _rules;

    public BlazorFormatter()
        : this(new BlazorParser()) { }

    public BlazorFormatter(IBlazorParser parser)
    {
        _parser = parser;
        _rules = new List<IFormattingRule>
        {
            new ElementFormattingRule(),
            new IfBlockFormattingRule(),
            new ElseBlockFormattingRule(),
            new ForeachBlockFormattingRule(),
            new CodeBlockFormattingRule()
        }
            .OrderByDescending(r => r.Priority)
            .ToList();
    }

    public string Format(string razorContent, BlazorFormatterOptions? options = null)
    {
        options ??= new BlazorFormatterOptions();

        var document = _parser.ParseDocument(razorContent);
        var context = new FormattingContext { Options = options };

        foreach (var node in document.Nodes)
        {
            FormatNode(node, context);
        }

        context.FinishLine();

        var result = string.Join(Environment.NewLine, context.OutputLines);

        if (options.RemoveTrailingWhitespace)
        {
            var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            result = string.Join(Environment.NewLine, lines.Select(l => l.TrimEnd()));
        }

        if (options.InsertFinalNewline && !result.EndsWith(Environment.NewLine))
        {
            result += Environment.NewLine;
        }

        return result;
    }

    public Task<string> FormatAsync(string razorContent, BlazorFormatterOptions? options = null)
    {
        return Task.Run(() => Format(razorContent, options));
    }

    private void FormatNode(BlazorNode node, FormattingContext context)
    {
        var rule = _rules.FirstOrDefault(r => r.CanApply(node, context.Options));

        if (rule != null)
        {
            rule.Apply(node, context);
        }
        else if (node is TextNode textNode)
        {
            var trimmed = textNode.Content.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                context.WriteLine(trimmed);
            }
        }
    }
}
