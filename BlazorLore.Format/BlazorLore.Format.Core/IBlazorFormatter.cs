namespace BlazorLore.Format.Core;

public interface IBlazorFormatter
{
    Task<string> FormatAsync(string razorContent, BlazorFormatterOptions? options = null);
    string Format(string razorContent, BlazorFormatterOptions? options = null);
}