using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace dex_webapp.Services
{
    public class BackgroundScanningService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BackgroundScanningService> _logger;
        private readonly BackgroundScannerSettings _options;

        public BackgroundScanningService(IOptions<BackgroundScannerSettings> options, IServiceScopeFactory scopeFactory, ILogger<BackgroundScanningService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                using (var scope = _scopeFactory.CreateScope())
                {
                    try
                    {
                        var ethereumService = scope.ServiceProvider.GetRequiredService<IEthereumService>();
                        await ethereumService.ScanActivateTokenByEventsAsync();
                        await ethereumService.ScanDeactivateTokenByEventsAsync();
                        await ethereumService.ScanOrdersByEventsAsync();
                        await ethereumService.ScanCancelByEventsAsync();
                        await ethereumService.ScanDepositByEventsAsync();
                        await ethereumService.ScanTradeByEventsAsync();
                        await ethereumService.ScanWithdrawByEventsAsync();


                        await ethereumService.ScanOrdersFilledAsync();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error while scanning for new transactions");
                    }
                }

                await Task.Delay(_options.ScanIntervalMilliseconds, stoppingToken);
            }
        }
    }
}
