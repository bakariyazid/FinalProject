using QRCodeAttendance.Interface.Services;

namespace QRCodeAttendance.Implementation.BackgroundJobs
{
    public class QrCodeRotationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<QrCodeRotationWorker> _logger;

        public QrCodeRotationWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<QrCodeRotationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (!stoppingToken.IsCancellationRequested)
            {
                await RotateDueQrCodes(stoppingToken);

                try
                {
                    await timer.WaitForNextTickAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task RotateDueQrCodes(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();

                var changedCount = await sessionService.RotateDueQrCodesAsync();
                if (changedCount > 0)
                {
                    _logger.LogInformation("QR rotation worker updated {ChangedCount} session(s).", changedCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QR rotation worker failed.");
            }
        }
    }
}
