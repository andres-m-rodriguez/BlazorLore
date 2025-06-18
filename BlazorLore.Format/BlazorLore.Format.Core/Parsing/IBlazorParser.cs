using Microsoft.AspNetCore.Razor.Language;

namespace BlazorLore.Format.Core.Parsing;

public interface IBlazorParser
{
    RazorSyntaxTree Parse(string razorContent);
    BlazorDocument ParseDocument(string razorContent);
}