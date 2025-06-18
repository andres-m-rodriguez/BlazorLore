using System.Text.RegularExpressions;

namespace BlazorLore.Scaffold.Cli.Services;

public class ModelAnalyzer
{
    public async Task<ModelInfo> AnalyzeModelAsync(string modelPath)
    {
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"Model file not found: {modelPath}");
        }

        var content = await File.ReadAllTextAsync(modelPath);
        var modelInfo = new ModelInfo();

        // Extract namespace
        var namespaceMatch = Regex.Match(content, @"namespace\s+([\w.]+)");
        if (namespaceMatch.Success)
        {
            modelInfo.Namespace = namespaceMatch.Groups[1].Value;
        }

        // Extract class or record name
        var classMatch = Regex.Match(content, @"public\s+(?:sealed\s+|abstract\s+|partial\s+)*(class|record)\s+(\w+)");
        if (!classMatch.Success)
        {
            throw new InvalidOperationException("Could not find class or record declaration in the file.");
        }

        modelInfo.IsRecord = classMatch.Groups[1].Value == "record";
        modelInfo.Name = classMatch.Groups[2].Value;

        // Extract properties with their attributes
        var propertyPattern = @"(?:\[([^\]]+)\]\s*)*public\s+(\w+\??)\s+(\w+)\s*{\s*get;\s*(?:set;|init;)?\s*}";
        var propertyMatches = Regex.Matches(content, propertyPattern);

        foreach (Match match in propertyMatches)
        {
            var property = new PropertyInfo
            {
                Type = match.Groups[2].Value,
                Name = match.Groups[3].Value,
                IsNullable = match.Groups[2].Value.EndsWith("?")
            };

            // Parse validation attributes
            if (match.Groups[1].Success)
            {
                var attributesText = match.Groups[1].Value;
                var attributes = ParseAttributes(attributesText);
                property.ValidationAttributes.AddRange(attributes);
            }

            modelInfo.Properties.Add(property);
        }

        // Also check for record primary constructor properties
        if (modelInfo.IsRecord)
        {
            var recordPattern = @"public\s+record\s+\w+\s*\(([^)]+)\)";
            var recordMatch = Regex.Match(content, recordPattern);
            
            if (recordMatch.Success)
            {
                var parameters = recordMatch.Groups[1].Value;
                var paramPattern = @"(?:\[([^\]]+)\]\s*)*(\w+\??)\s+(\w+)";
                var paramMatches = Regex.Matches(parameters, paramPattern);

                foreach (Match paramMatch in paramMatches)
                {
                    // Check if this property was already added (to avoid duplicates)
                    var propName = paramMatch.Groups[3].Value;
                    if (!modelInfo.Properties.Any(p => p.Name == propName))
                    {
                        var property = new PropertyInfo
                        {
                            Type = paramMatch.Groups[2].Value,
                            Name = propName,
                            IsNullable = paramMatch.Groups[2].Value.EndsWith("?")
                        };

                        // Parse validation attributes
                        if (paramMatch.Groups[1].Success)
                        {
                            var attributesText = paramMatch.Groups[1].Value;
                            var attributes = ParseAttributes(attributesText);
                            property.ValidationAttributes.AddRange(attributes);
                        }

                        modelInfo.Properties.Add(property);
                    }
                }
            }
        }

        return modelInfo;
    }

    private List<ValidationAttribute> ParseAttributes(string attributesText)
    {
        var attributes = new List<ValidationAttribute>();
        
        // Split multiple attributes
        var attributePattern = @"(\w+)(?:\(([^)]*)\))?";
        var matches = Regex.Matches(attributesText, attributePattern);

        foreach (Match match in matches)
        {
            var attribute = new ValidationAttribute
            {
                Name = match.Groups[1].Value
            };

            // Parse parameters if they exist
            if (match.Groups[2].Success && !string.IsNullOrWhiteSpace(match.Groups[2].Value))
            {
                var paramsText = match.Groups[2].Value;
                
                // Handle named parameters
                var namedParamPattern = @"(\w+)\s*=\s*""?([^"",]+)""?";
                var namedMatches = Regex.Matches(paramsText, namedParamPattern);
                
                foreach (Match namedMatch in namedMatches)
                {
                    attribute.Parameters[namedMatch.Groups[1].Value] = namedMatch.Groups[2].Value.Trim('"');
                }

                // Handle positional parameters (for simple cases like [StringLength(100)])
                if (!attribute.Parameters.Any())
                {
                    var simpleValue = paramsText.Trim().Trim('"');
                    if (!string.IsNullOrWhiteSpace(simpleValue))
                    {
                        attribute.Parameters["Value"] = simpleValue;
                    }
                }
            }

            attributes.Add(attribute);
        }

        return attributes;
    }
}