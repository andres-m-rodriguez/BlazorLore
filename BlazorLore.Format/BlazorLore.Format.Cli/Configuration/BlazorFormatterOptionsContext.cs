using BlazorLore.Format.Core;
using System.Text.Json.Serialization;

namespace BlazorLore.Format.Cli.Configuration;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(BlazorFormatterOptions))]
[JsonSerializable(typeof(AttributeFormatting))]
[JsonSerializable(typeof(QuoteStyle))]
public partial class BlazorFormatterOptionsContext : JsonSerializerContext
{
}