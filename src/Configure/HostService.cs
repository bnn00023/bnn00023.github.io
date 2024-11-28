using Microsoft.Extensions.Options;

namespace Configure
{
    public class HostService : BackgroundService
    {
        private readonly IOptionsMonitor<TestOption> _optionsMonitor;
        private readonly ILogger<HostService> _logger;

        public HostService(IOptionsMonitor<TestOption> optionsMonitor, ILogger<HostService> logger)
        {
            _optionsMonitor = optionsMonitor;
            _logger = logger;
            _optionsMonitor.OnChange(option => _logger.LogInformation("Option changed to {text}", option.Text));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
