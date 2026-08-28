namespace Mova.Api.Configuration;

public sealed class ForwardedHeadersSettings
{
    public const string SectionName = "ForwardedHeaders";

    public string ForwardedHeaders { get; set; } = "XForwardedFor";

    public int? ForwardLimit { get; set; } = 1;

    public List<string> KnownProxies { get; set; } = [];

    public List<string> KnownNetworks { get; set; } = [];
}
