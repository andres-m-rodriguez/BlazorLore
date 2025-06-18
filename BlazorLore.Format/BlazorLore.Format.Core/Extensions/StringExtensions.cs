namespace BlazorLore.Format.Core.Extensions;

public static class StringExtensions
{
    private static readonly IBlazorFormatter _formatter = new BlazorFormatter();

    public static string FormatBlazor(this string razorContent, BlazorFormatterOptions? options = null)
    {
        return _formatter.Format(razorContent, options);
    }

    public static Task<string> FormatBlazorAsync(this string razorContent, BlazorFormatterOptions? options = null)
    {
        return _formatter.FormatAsync(razorContent, options);
    }

    public static string FormatBlazor(this string razorContent, Action<BlazorFormatterOptions> configureOptions)
    {
        var options = new BlazorFormatterOptions();
        configureOptions(options);
        return _formatter.Format(razorContent, options);
    }

    public static Task<string> FormatBlazorAsync(this string razorContent, Action<BlazorFormatterOptions> configureOptions)
    {
        var options = new BlazorFormatterOptions();
        configureOptions(options);
        return _formatter.FormatAsync(razorContent, options);
    }
}