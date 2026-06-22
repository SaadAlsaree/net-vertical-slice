namespace VsaTemplate.Models;

public sealed class KeycloakSettings
{
    public string BaseUrl { get; init; } = "";
    public string Realm { get; init; } = "";
    public string ClientId { get; init; } = "";
    public string AdminClientId { get; init; } = "";
    public string AdminClientSecret { get; init; } = "";
    public string Issuer { get; init; } = "";
    public string InternalIssuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public string WebhookSecret { get; init; } = "";
    public bool UseInternalUrlsForAdmin { get; init; }
}
