using System.IO;
using Serilog.Events;
using Serilog.Formatting;

namespace Mova.Infrastructure.Logging;

public class SensitiveDataRedactingFormatter : ITextFormatter
{
    private readonly ITextFormatter _innerFormatter;

    public SensitiveDataRedactingFormatter(ITextFormatter innerFormatter)
    {
        _innerFormatter = innerFormatter;
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        var redactedProperties = SensitiveDataRedactor.RedactProperties(logEvent.Properties);

        var redactedLogEvent = new LogEvent(
            logEvent.Timestamp,
            logEvent.Level,
            logEvent.Exception,
            logEvent.MessageTemplate,
            redactedProperties);

        _innerFormatter.Format(redactedLogEvent, output);
    }
}
