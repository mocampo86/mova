using Mova.Infrastructure.Logging;
using Serilog.Events;
using Xunit;

namespace Mova.UnitTests.Infrastructure.Logging;

public class SensitiveDataRedactorTests
{
    [Fact]
    public void RedactProperties_LeavesNonSensitiveValuesUnchanged()
    {
        var properties = new Dictionary<string, LogEventPropertyValue>
        {
            ["Username"] = new ScalarValue("john_doe"),
            ["CourtName"] = new ScalarValue("Court A")
        };

        var redacted = SensitiveDataRedactor.RedactProperties(properties);

        Assert.Equal("john_doe", GetScalarValue(redacted, "Username"));
        Assert.Equal("Court A", GetScalarValue(redacted, "CourtName"));
    }

    [Fact]
    public void RedactProperties_RedactsSensitiveScalarValues()
    {
        var properties = new Dictionary<string, LogEventPropertyValue>
        {
            ["Password"] = new ScalarValue("super-secret"),
            ["Token"] = new ScalarValue("Bearer abc123"),
            ["ApiKey"] = new ScalarValue("key-123")
        };

        var redacted = SensitiveDataRedactor.RedactProperties(properties);

        Assert.Equal("[REDACTED]", GetScalarValue(redacted, "Password"));
        Assert.Equal("[REDACTED]", GetScalarValue(redacted, "Token"));
        Assert.Equal("[REDACTED]", GetScalarValue(redacted, "ApiKey"));
    }

    [Fact]
    public void RedactProperties_RedactsSensitiveNestedProperties()
    {
        var user = new StructureValue(new[]
        {
            new LogEventProperty("FullName", new ScalarValue("John Doe")),
            new LogEventProperty("Email", new ScalarValue("john@example.com")),
            new LogEventProperty("Phone", new ScalarValue("555-1234")),
            new LogEventProperty("Username", new ScalarValue("john_doe"))
        });

        var properties = new Dictionary<string, LogEventPropertyValue>
        {
            ["User"] = user
        };

        var redacted = SensitiveDataRedactor.RedactProperties(properties);
        var redactedUser = GetStructureValue(redacted, "User");

        Assert.Equal("[REDACTED]", GetPropertyValue(redactedUser, "FullName"));
        Assert.Equal("[REDACTED]", GetPropertyValue(redactedUser, "Email"));
        Assert.Equal("[REDACTED]", GetPropertyValue(redactedUser, "Phone"));
        Assert.Equal("john_doe", GetPropertyValue(redactedUser, "Username"));
    }

    [Fact]
    public void RedactProperties_RedactsSensitiveDictionaryKeys()
    {
        var dictionary = new DictionaryValue(new[]
        {
            new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("password"), new ScalarValue("secret")),
            new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("username"), new ScalarValue("john"))
        });

        var properties = new Dictionary<string, LogEventPropertyValue>
        {
            ["Payload"] = dictionary
        };

        var redacted = SensitiveDataRedactor.RedactProperties(properties);
        var redactedDictionary = GetDictionaryValue(redacted, "Payload");

        Assert.Equal("[REDACTED]", GetDictionaryEntry(redactedDictionary, "password"));
        Assert.Equal("john", GetDictionaryEntry(redactedDictionary, "username"));
    }

    [Fact]
    public void RedactProperties_LeavesNonSensitiveSequenceItemsUnchanged()
    {
        var sequence = new SequenceValue(new[]
        {
            new ScalarValue("public"),
            new ScalarValue("secret-token")
        });

        var properties = new Dictionary<string, LogEventPropertyValue>
        {
            ["Tokens"] = sequence
        };

        var redacted = SensitiveDataRedactor.RedactProperties(properties);
        var redactedSequence = GetSequenceValue(redacted, "Tokens");

        Assert.Equal("public", ((ScalarValue)redactedSequence.Elements[0]).Value);
        Assert.Equal("secret-token", ((ScalarValue)redactedSequence.Elements[1]).Value);
    }

    private static string? GetScalarValue(IReadOnlyList<LogEventProperty> properties, string name)
    {
        var property = properties.FirstOrDefault(p => p.Name == name);
        return (property?.Value as ScalarValue)?.Value as string;
    }

    private static StructureValue GetStructureValue(IReadOnlyList<LogEventProperty> properties, string name)
    {
        var property = properties.First(p => p.Name == name);
        return (StructureValue)property.Value;
    }

    private static string? GetPropertyValue(StructureValue structure, string name)
    {
        var property = structure.Properties.First(p => p.Name == name);
        return (property.Value as ScalarValue)?.Value as string;
    }

    private static DictionaryValue GetDictionaryValue(IReadOnlyList<LogEventProperty> properties, string name)
    {
        var property = properties.First(p => p.Name == name);
        return (DictionaryValue)property.Value;
    }

    private static string? GetDictionaryEntry(DictionaryValue dictionary, string key)
    {
        var entry = dictionary.Elements.FirstOrDefault(kvp => (kvp.Key.Value as string) == key);
        return (entry.Value as ScalarValue)?.Value as string;
    }

    private static SequenceValue GetSequenceValue(IReadOnlyList<LogEventProperty> properties, string name)
    {
        var property = properties.First(p => p.Name == name);
        return (SequenceValue)property.Value;
    }
}
