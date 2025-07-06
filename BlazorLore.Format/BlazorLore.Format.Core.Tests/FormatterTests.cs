using System.Text;
using BlazorLore.Format.Core;
using BlazorLore.Format.Core.Parsing;
using Xunit;

namespace BlazorLore.Format.Core.Tests;

public class FormatterTests
{
    private readonly IBlazorFormatter _formatter;

    public FormatterTests()
    {
        _formatter = new BlazorFormatter();
    }

    [Fact]
    public void Should_Keep_Rendermode_Directive_On_Single_Line()
    {
        // Arrange
        var input = @"@page ""/counter""
@rendermode InteractiveServer
<PageTitle>Counter</PageTitle>";

        var options = new BlazorFormatterOptions();

        // Act
        var result = _formatter.Format(input, options);

        // Assert
        Assert.Contains("@rendermode InteractiveServer", result);
        Assert.DoesNotContain("@rendermode\nInteractiveServer", result);
        Assert.DoesNotContain("@rendermode\r\nInteractiveServer", result);
    }

    [Fact]
    public void Should_Keep_Attribute_Directive_On_Single_Line()
    {
        // Arrange
        var input = @"@page ""/counter""
@attribute [StreamRendering]
<h1>Counter</h1>";

        var options = new BlazorFormatterOptions();

        // Act
        var result = _formatter.Format(input, options);

        // Assert
        Assert.Contains("@attribute [StreamRendering]", result);
        Assert.DoesNotContain("@attribute\n[StreamRendering]", result);
        Assert.DoesNotContain("@attribute\r\n[StreamRendering]", result);
    }

    [Fact]
    public void Should_Format_Multiple_Directives_Correctly()
    {
        // Arrange
        var input = @"@page ""/counter""
@rendermode InteractiveServer
@attribute [StreamRendering]
<PageTitle>Counter</PageTitle>
<h1>Counter</h1>";

        var options = new BlazorFormatterOptions();

        // Act
        var result = _formatter.Format(input, options);

        // Assert
        Assert.Contains("@page \"/counter\"", result);
        Assert.Contains("@rendermode InteractiveServer", result);
        Assert.Contains("@attribute [StreamRendering]", result);
        
        // Verify they're on separate lines but not broken up
        var lines = result.Split('\n');
        Assert.Contains(lines, l => l.Trim() == "@page \"/counter\"");
        Assert.Contains(lines, l => l.Trim() == "@rendermode InteractiveServer");
        Assert.Contains(lines, l => l.Trim() == "@attribute [StreamRendering]");
    }

    [Fact]
    public void Should_Not_Break_DOCTYPE_Declaration()
    {
        // Arrange
        var input = "<!DOCTYPE html>\n<html>\n<head>\n<title>Test</title>\n</head>\n</html>";

        var options = new BlazorFormatterOptions();

        // Act
        var result = _formatter.Format(input, options);

        // Assert
        Assert.Contains("<!DOCTYPE html>", result);
        Assert.DoesNotContain("<\n!DOCTYPE html>", result);
        Assert.DoesNotContain("< !DOCTYPE html>", result);
    }

    [Fact]
    public void Should_Handle_DOCTYPE_With_Attributes()
    {
        // Arrange
        var input = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html>
<body>Test</body>
</html>";

        var options = new BlazorFormatterOptions();

        // Act
        var result = _formatter.Format(input, options);

        // Assert
        // The DOCTYPE should remain on a single line
        var lines = result.Split('\n');
        Assert.Contains(lines, l => l.Trim().StartsWith("<!DOCTYPE") && l.Trim().EndsWith(">"));
    }
}