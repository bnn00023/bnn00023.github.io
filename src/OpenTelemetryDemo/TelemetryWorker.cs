using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using StackExchange.Redis;

internal sealed class TelemetryWorker : IHostedService
{
    internal const string ActivitySourceName = "OpenTelemetryDemo.TelemetryWorker";
    internal const string GoogleClientName = "google";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TelemetryWorker> _logger;
    private readonly TelemetryOptions _options;
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public TelemetryWorker(
        IHttpClientFactory httpClientFactory,
        ILogger<TelemetryWorker> logger,
        TelemetryOptions options,
        IConnectionMultiplexer redisConnection,
        IHostApplicationLifetime applicationLifetime)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options;
        _redisConnection = redisConnection;
        _applicationLifetime = applicationLifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting telemetry operations");

        using var activity = ActivitySource.StartActivity("Demo run", ActivityKind.Internal);

        if (activity is not null)
        {
            activity.SetTag("redis.connection", _options.RedisConnectionString);
            activity.SetTag("postgres.connection", _options.PostgresConnectionString);
            activity.SetTag("jaeger.host", _options.JaegerHost);
            activity.SetTag("jaeger.port", _options.JaegerPort);
        }

        try
        {
            await ExecuteHttpRequestAsync(cancellationToken);
            await ExecutePostgresCommandsAsync(cancellationToken);
            await ExecuteRedisCommandsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "An error occurred while running telemetry operations");
        }
        finally
        {
            _applicationLifetime.StopApplication();
            _logger.LogInformation("Telemetry operations completed");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Telemetry worker is stopping");
        return Task.CompletedTask;
    }

    private async Task ExecuteHttpRequestAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("HTTP GET google", ActivityKind.Client);

        var client = _httpClientFactory.CreateClient(GoogleClientName);

        _logger.LogInformation("Sending request to {Url}", client.BaseAddress);
        using var response = await client.GetAsync(string.Empty, cancellationToken);
        var contentLength = response.Content.Headers.ContentLength;

        activity?.SetTag("http.status_code", (int)response.StatusCode);
        activity?.SetTag("http.response_content_length", contentLength);

        _logger.LogInformation("Received {StatusCode} from {Url}", response.StatusCode, client.BaseAddress);
    }

    private async Task ExecutePostgresCommandsAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("PostgreSQL commands", ActivityKind.Client);

        await using var connection = new NpgsqlConnection(_options.PostgresConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var createTable = new NpgsqlCommand(
            "CREATE TABLE IF NOT EXISTS telemetry_demo (id SERIAL PRIMARY KEY, created_at TIMESTAMPTZ NOT NULL)",
            connection);
        await createTable.ExecuteNonQueryAsync(cancellationToken);

        await using var insertCommand = new NpgsqlCommand(
            "INSERT INTO telemetry_demo(created_at) VALUES (@created_at) RETURNING id",
            connection);
        insertCommand.Parameters.AddWithValue("created_at", DateTime.UtcNow);
        var insertedId = (int)(await insertCommand.ExecuteScalarAsync(cancellationToken) ?? 0);

        await using var queryCommand = new NpgsqlCommand(
            "SELECT created_at FROM telemetry_demo WHERE id = @id",
            connection);
        queryCommand.Parameters.AddWithValue("id", insertedId);
        var queriedValue = await queryCommand.ExecuteScalarAsync(cancellationToken);

        activity?.SetTag("db.last_insert_id", insertedId);
        activity?.SetTag("db.queried_value", queriedValue);

        _logger.LogInformation("Inserted row {Id} at {Timestamp}", insertedId, queriedValue);
    }

    private async Task ExecuteRedisCommandsAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Redis commands", ActivityKind.Client);

        var database = _redisConnection.GetDatabase();
        var cacheKey = "otel-demo:timestamp";
        var cacheValue = DateTimeOffset.UtcNow.ToString("O");

        _logger.LogInformation("Writing value to Redis at {Key}", cacheKey);
        await database.StringSetAsync(cacheKey, cacheValue);

        var storedValue = await database.StringGetAsync(cacheKey);
        activity?.SetTag("redis.key", cacheKey);
        activity?.SetTag("redis.value", storedValue);

        _logger.LogInformation("Read value {Value} from Redis at {Key}", storedValue, cacheKey);
    }
}
