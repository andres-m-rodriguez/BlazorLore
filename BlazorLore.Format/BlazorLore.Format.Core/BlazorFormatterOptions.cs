namespace BlazorLore.Format.Core;

public class BlazorFormatterOptions
{
    public int IndentSize { get; set; } = 4;
    public bool UseTabs { get; set; } = false;
    public int MaxLineLength { get; set; } = 120;
    public bool FormatEmbeddedCSharp { get; set; } = true;
    public bool FormatEmbeddedCss { get; set; } = true;
    public bool FormatEmbeddedJavaScript { get; set; } = true;
    public AttributeFormatting AttributeFormatting { get; set; } = AttributeFormatting.MultilineWhenMany;
    public int AttributesPerLine { get; set; } = 1;
    public int AttributeBreakThreshold { get; set; } = 3;
    public bool BreakContentWithManyAttributes { get; set; } = true;
    public int ContentBreakThreshold { get; set; } = 2;
    public bool SortAttributes { get; set; } = false;
    public bool CollapseEmptyTags { get; set; } = true;
    public bool RemoveTrailingWhitespace { get; set; } = true;
    public bool InsertFinalNewline { get; set; } = true;
    public QuoteStyle QuoteStyle { get; set; } = QuoteStyle.Double;
}

public enum AttributeFormatting
{
    Inline,
    MultilineAlways,
    MultilineWhenMany
}

public enum QuoteStyle
{
    Single,
    Double
}