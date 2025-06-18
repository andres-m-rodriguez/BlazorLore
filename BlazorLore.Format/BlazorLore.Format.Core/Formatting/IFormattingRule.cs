using BlazorLore.Format.Core.Parsing;

namespace BlazorLore.Format.Core.Formatting;

public interface IFormattingRule
{
    string Name { get; }
    int Priority { get; }
    bool CanApply(BlazorNode node, BlazorFormatterOptions options);
    void Apply(BlazorNode node, FormattingContext context);
}

public class FormattingContext
{
    public BlazorFormatterOptions Options { get; set; } = new();
    public int CurrentIndentLevel { get; set; }
    public List<string> OutputLines { get; set; } = new();
    public string CurrentLine { get; set; } = string.Empty;
    
    public string GetIndentation()
    {
        if (Options.UseTabs)
        {
            return new string('\t', CurrentIndentLevel);
        }
        return new string(' ', CurrentIndentLevel * Options.IndentSize);
    }
    
    public void WriteLine(string content = "")
    {
        if (!string.IsNullOrEmpty(CurrentLine))
        {
            OutputLines.Add(CurrentLine);
            CurrentLine = string.Empty;
        }
        
        if (!string.IsNullOrEmpty(content))
        {
            OutputLines.Add(GetIndentation() + content);
        }
        else
        {
            OutputLines.Add(string.Empty);
        }
    }
    
    public void Write(string content)
    {
        if (string.IsNullOrEmpty(CurrentLine))
        {
            CurrentLine = GetIndentation();
        }
        CurrentLine += content;
    }
    
    public void FinishLine()
    {
        if (!string.IsNullOrEmpty(CurrentLine))
        {
            OutputLines.Add(CurrentLine);
            CurrentLine = string.Empty;
        }
    }
}