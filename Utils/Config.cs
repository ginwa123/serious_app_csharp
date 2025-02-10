
namespace App.Utils;

public class LoggingSettings
{
    public required LogLevelSettings LogLevel { get; set; }
}

public class LogLevelSettings
{
    public required string Default { get; set; }
    public required string MicrosoftAspNetCore { get; set; }
}

public class KestrelSettings
{
    public required EndpointsSettings Endpoints { get; set; }
}

public class EndpointsSettings
{
    public required HttpSettings Http { get; set; }
}

public class HttpSettings
{
    public required string Url { get; set; }
}

public class AuthenticationSettings
{
    public required SchemesSettings Schemes { get; set; }
}

public class SchemesSettings
{
    public required BearerSettings Bearer { get; set; }
}

public class BearerSettings
{
    public required List<string> ValidAudiences { get; set; }
    public required string ValidIssuer { get; set; }
}

public class SeriousAppSettings
{
    public required DbSettings DB { get; set; }
    public required JwtSettings JWT { get; set; }
    public required Worker Worker { get; set; }
}

public class Worker
{
    public required ApiUrlPermissionCleaner ApiUrlPermissionCleaner { get; set; }
    public required ApiUrlCleaner ApiUrlCleaner { get; set; }

}

public class ApiUrlPermissionCleaner
{
    public bool IsEnabled { get; set; }
    public required string CronJob { get; set; }
}


public class ApiUrlCleaner
{
    public bool IsEnabled { get; set; }
    public required string CronJob { get; set; }
}

public class DbSettings
{
    public required string ConnectionString { get; set; }
    public int RetryCreateCount { get; set; }
    public int LockingTimeout { get; set; }
}

public class JwtSettings
{
    public required string Secret { get; set; }
    public int ExpiresInMinutes { get; set; }
}

public class AppSettings : IAppSettings
{
    public AppSettings(IConfiguration configuration)
    {
        configuration.Bind(this);
    }

    public LoggingSettings Logging { get; set; }
    public string AllowedHosts { get; set; }
    public KestrelSettings Kestrel { get; set; }
    public AuthenticationSettings Authentication { get; set; }
    public SeriousAppSettings SeriousApp { get; set; }
}

public interface IAppSettings
{
    LoggingSettings Logging { get; set; }
    string AllowedHosts { get; set; }
    KestrelSettings Kestrel { get; set; }
    AuthenticationSettings Authentication { get; set; }
    SeriousAppSettings SeriousApp { get; set; }
}