using System.Text;

namespace BlazorLore.Scaffold.Cli.Tests.Utilities;

/// <summary>
/// A test console implementation for capturing command line output
/// </summary>
public class TestConsole : IConsole
{
    private readonly StringBuilder _outBuilder = new();
    private readonly StringBuilder _errorBuilder = new();
    
    public IStandardStreamWriter Out => new TestStreamWriter(_outBuilder);
    public IStandardStreamWriter Error => new TestStreamWriter(_errorBuilder);
    public bool IsOutputRedirected => true;
    public bool IsErrorRedirected => true;
    public bool IsInputRedirected => true;
    
    public string GetOutput() => _outBuilder.ToString();
    public string GetError() => _errorBuilder.ToString();
    
    public void Clear()
    {
        _outBuilder.Clear();
        _errorBuilder.Clear();
    }
}

public interface IConsole
{
    IStandardStreamWriter Out { get; }
    IStandardStreamWriter Error { get; }
    bool IsOutputRedirected { get; }
    bool IsErrorRedirected { get; }
    bool IsInputRedirected { get; }
}

public interface IStandardStreamWriter
{
    void Write(string? value);
    void WriteLine(string? value);
}

public class TestStreamWriter : IStandardStreamWriter
{
    private readonly StringBuilder _builder;
    
    public TestStreamWriter(StringBuilder builder)
    {
        _builder = builder;
    }
    
    public void Write(string? value)
    {
        if (value != null)
            _builder.Append(value);
    }
    
    public void WriteLine(string? value)
    {
        if (value != null)
            _builder.AppendLine(value);
        else
            _builder.AppendLine();
    }
}