using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    var configuration = context.Configuration;

    var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
    var postgresConnectionString = configuration.GetValue<string>("Postgres:ConnectionString") ??
        "Host=localhost;Username=otel;Password=otel;Database=otel";
    var jaegerHost = configuration.GetValue<string>("Jaeger:Host") ?? "localhost";
    var jaegerPort = configuration.GetValue<int?>("Jaeger:Port") ?? 6831;

    var redisConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(redisConnectionString));

    services.AddSingleton(sp => redisConnection.Value);

    services.AddSingleton(new TelemetryOptions
    {
        RedisConnectionString = redisConnectionString,
        PostgresConnectionString = postgresConnectionString,
        JaegerHost = jaegerHost,
        JaegerPort = jaegerPort
    });

    services.AddHttpClient(TelemetryWorker.GoogleClientName, client =>
    {
        client.BaseAddress = new Uri("https://www.google.com/");
    });

    services.AddHostedService<TelemetryWorker>();

    services.AddOpenTelemetry()
        .WithTracing(tracerBuilder =>
        {
            tracerBuilder
                .AddSource(TelemetryWorker.ActivitySourceName)
                .SetResourceBuilder(ResourceBuilder
                    .CreateDefault()
                    .AddService("OpenTelemetryDemo", serviceVersion: "1.0.0"))
                .AddHttpClientInstrumentation()
                .AddRedisInstrumentation(options =>
                {
                    options.FlushInterval = TimeSpan.FromSeconds(1);
                    options.SetConnectionMultiplexerFactory(() => redisConnection.Value);
                })
                .AddNpgsql()
                .AddJaegerExporter(options =>
                {
                    options.AgentHost = jaegerHost;
                    options.AgentPort = jaegerPort;
                });
        });
});

await builder.RunConsoleAsync();

internal sealed record TelemetryOptions
{
    public string RedisConnectionString { get; init; } = string.Empty;
    public string PostgresConnectionString { get; init; } = string.Empty;
    public string JaegerHost { get; init; } = string.Empty;
    public int JaegerPort { get; init; }
}
