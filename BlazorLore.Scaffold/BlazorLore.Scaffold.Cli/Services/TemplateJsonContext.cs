using System.Text.Json.Serialization;
using BlazorLore.Scaffold.Cli.Models;

namespace BlazorLore.Scaffold.Cli.Services;

[JsonSerializable(typeof(TemplateConfig))]
[JsonSerializable(typeof(TemplateFile))]
[JsonSerializable(typeof(TemplateParameter))]
[JsonSerializable(typeof(List<TemplateFile>))]
[JsonSerializable(typeof(List<TemplateParameter>))]
[JsonSerializable(typeof(TemplateConfigExample))]
[JsonSerializable(typeof(TemplateFileExample))]
[JsonSerializable(typeof(TemplateParameterExample))]
[JsonSerializable(typeof(List<TemplateFileExample>))]
[JsonSerializable(typeof(List<TemplateParameterExample>))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
internal partial class TemplateJsonContext : JsonSerializerContext
{
}