using Equinor.OsduCsharpClient.Facade;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Osdu.Samples;

/// <summary>
/// Builds configuration and a configured <see cref="OsduClient"/> shared by all
/// samples. Configuration comes from the standard .NET sources: appsettings.json,
/// appsettings.local.json (gitignored), user secrets, and environment variables.
/// </summary>
public sealed class SampleHost : IDisposable
{
    public OsduClient Client { get; }
    public DemoOptions Demo { get; }
    private readonly ILoggerFactory _loggerFactory;

    public SampleHost(bool verbose)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.local.json", optional: true)
            .AddUserSecrets<SampleHost>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        Demo = configuration.GetSection("Demo").Get<DemoOptions>() ?? new DemoOptions();

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(o => { o.SingleLine = true; o.TimestampFormat = "HH:mm:ss "; });
            builder.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
        });

        var config = OsduConfig.FromConfiguration(configuration);
        Client = new OsduClient(config, loggerFactory: _loggerFactory);
    }

    public void Dispose()
    {
        Client.Dispose();
        _loggerFactory.Dispose();
    }
}
