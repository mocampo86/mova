using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace ReservaCanchas.Api.HealthChecks;

public static class HealthCheckResponseWriter
{
    private const string ContentType = "application/json";

    public static async Task WriteResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = ContentType;

        var includeDescriptions = context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() ?? false;

        await using var writer = new Utf8JsonWriter(context.Response.Body);

        writer.WriteStartObject();
        writer.WriteString("status", report.Status.ToString());
        writer.WritePropertyName("dependencies");
        writer.WriteStartArray();

        foreach (var (name, entry) in report.Entries)
        {
            writer.WriteStartObject();
            writer.WriteString("name", name);
            writer.WriteString("status", entry.Status.ToString());

            if (includeDescriptions && !string.IsNullOrWhiteSpace(entry.Description))
            {
                writer.WriteString("description", entry.Description);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();

        await writer.FlushAsync();
    }
}
