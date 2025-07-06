namespace BlazorLore.Format.Core.Parsing;

public class BlazorDocument
{
    public List<BlazorNode> Nodes { get; set; } = new();
    public string OriginalContent { get; set; } = string.Empty;
}

public abstract class BlazorNode
{
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public int LineNumber { get; set; }
    public int ColumnNumber { get; set; }
}

public class ElementNode : BlazorNode
{
    public string TagName { get; set; } = string.Empty;
    public List<AttributeNode> Attributes { get; set; } = new();
    public List<BlazorNode> Children { get; set; } = new();
    public bool IsSelfClosing { get; set; }
}

public class AttributeNode : BlazorNode
{
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public bool IsDirective { get; set; }
}

public class TextNode : BlazorNode
{
    public string Content { get; set; } = string.Empty;
}

public class CodeBlockNode : BlazorNode
{
    public string Code { get; set; } = string.Empty;
    public CodeBlockType Type { get; set; }
}

public enum CodeBlockType
{
    Expression,
    Statement,
    Directive,
    CodeBlock,
    IfBlock,
    ForeachBlock,
    ElseBlock,
    ElseIfBlock
}