namespace Shared.Application.Configuration;

/// <summary>
/// Application configuration settings
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Application name
    /// </summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// Application version
    /// </summary>
    public string AppVersion { get; set; } = string.Empty;

    /// <summary>
    /// Application environment (Development, Staging, Production)
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// API base URL
    /// </summary>
    public string ApiBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Enable detailed logging
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable API documentation
    /// </summary>
    public bool EnableApiDocumentation { get; set; } = true;
}
