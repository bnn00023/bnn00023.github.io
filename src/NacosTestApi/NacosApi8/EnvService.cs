
namespace NacosApi8
{
    public class EnvService : BackgroundService
    {
        private readonly ILogger<EnvService> _logger;

        public EnvService(ILogger<EnvService> logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            _logger.LogInformation("ASPNETCORE_URLS: {0}", url);

            var ports = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS");
            _logger.LogInformation("ASPNETCORE_HTTP_PORTS: {0}", ports);
            return Task.CompletedTask;
        }
    }
}
