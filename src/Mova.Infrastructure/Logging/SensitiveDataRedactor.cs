using Serilog.Events;

namespace Mova.Infrastructure.Logging;

public static class SensitiveDataRedactor
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "token",
        "secret",
        "apikey",
        "api_key",
        "authorization",
        "auth",
        "jwt",
        "bearer",
        "cookie",
        "email",
        "phone",
        "phonenumber",
        "mobile",
        "fullname",
        "creditcard",
        "ssn",
        "connectionstring",
        "connection_string"
    };

    public static bool IsSensitive(string? key)
    {
        return !string.IsNullOrWhiteSpace(key) && SensitiveKeys.Contains(key);
    }

    public static IReadOnlyList<LogEventProperty> RedactProperties(IReadOnlyDictionary<string, LogEventPropertyValue> properties)
    {
        return properties
            .Select(property => new LogEventProperty(property.Key, RedactValue(property.Key, property.Value)))
            .ToList();
    }

    private static LogEventPropertyValue RedactValue(string? key, LogEventPropertyValue value)
    {
        if (IsSensitive(key))
        {
            return new ScalarValue("[REDACTED]");
        }

        return value switch
        {
            ScalarValue scalar => scalar,
            SequenceValue sequence => RedactSequence(sequence),
            StructureValue structure => RedactStructure(structure),
            DictionaryValue dictionary => RedactDictionary(dictionary),
            _ => value
        };
    }

    private static SequenceValue RedactSequence(SequenceValue sequence)
    {
        return new SequenceValue(sequence.Elements.Select((element, index) => RedactValue(index.ToString(), element)));
    }

    private static StructureValue RedactStructure(StructureValue structure)
    {
        var properties = structure.Properties
            .Select(property => new LogEventProperty(property.Name, RedactValue(property.Name, property.Value)));

        return new StructureValue(properties, structure.TypeTag);
    }

    private static DictionaryValue RedactDictionary(DictionaryValue dictionary)
    {
        var elements = dictionary.Elements
            .Select(element => new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                element.Key,
                RedactValue(GetDictionaryKeyName(element.Key), element.Value)))
            .ToList();

        return new DictionaryValue(elements);
    }

    private static string? GetDictionaryKeyName(ScalarValue scalar)
    {
        return scalar.Value as string ?? scalar.Value?.ToString();
    }
}
