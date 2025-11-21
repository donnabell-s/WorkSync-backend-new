using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.HostedServices
{

    public class MetricsComputationHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MetricsComputationHostedService> _logger;
        
        // Run every 30 seconds (for demonstration purposes)
        private static readonly TimeSpan ComputationInterval = TimeSpan.FromSeconds(10);

        public MetricsComputationHostedService(
            IServiceProvider serviceProvider,
            ILogger<MetricsComputationHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MetricsComputationHostedService is starting.");

            // Wait a bit before the first run to let the application fully start
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ComputeMetricsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while computing metrics in hosted service.");
                }

                // Wait for the next interval
                try
                {
                    await Task.Delay(ComputationInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // This is expected when the service is stopping
                    _logger.LogInformation("MetricsComputationHostedService is stopping due to cancellation.");
                    break;
                }
            }

            _logger.LogInformation("MetricsComputationHostedService has stopped.");
        }

        private async Task ComputeMetricsAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting scheduled metrics computation at {Time}", DateTime.UtcNow);

            using (var scope = _serviceProvider.CreateScope())
            {
                var metricsService = scope.ServiceProvider.GetRequiredService<IMetricsService>();

                try
                {
                    var today = DateTime.Today;
                    var yesterday = today.AddDays(-1);

                    // Compute metrics for yesterday (final numbers)
                    _logger.LogDebug("Computing metrics for yesterday: {Date}", yesterday);
                    await metricsService.ComputeMetricsForDateAsync(yesterday, cancellationToken);

                    // Compute metrics for today (current numbers)
                    _logger.LogDebug("Computing metrics for today: {Date}", today);
                    await metricsService.ComputeMetricsForDateAsync(today, cancellationToken);

                    _logger.LogInformation("Successfully completed scheduled metrics computation at {Time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to compute metrics in scheduled task");
                    throw;
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MetricsComputationHostedService is stopping gracefully.");
            await base.StopAsync(cancellationToken);
        }
    }
}
