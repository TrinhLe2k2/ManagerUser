using ManagerUser.Worker.Options;
using ManagerUser.Worker.Services;
using Microsoft.Extensions.Options;

namespace ManagerUser.Worker.BackgroundServices;

public sealed class UserSnapshotWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UserSnapshotWorker> _logger;
    private readonly IOptions<WorkerBusinessOptions> _options;

    public UserSnapshotWorker(IServiceScopeFactory scopeFactory, ILogger<UserSnapshotWorker> logger, IOptions<WorkerBusinessOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_options.Value.BackgroundIntervalSeconds);
        _logger.LogInformation("Sample A - BackgroundService: started with interval {Interval}.", interval);

        await RunBusinessAsync(stoppingToken);

        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                // await A; await B; await C; = chạy tuần tự
                await RunBusinessAsync(stoppingToken);
                //await RunBusinessAsync2(stoppingToken);
                //await RunBusinessAsync3(stoppingToken);

                // Task.WhenAll(A, B, C) = chạy đồng thời và chờ tất cả xong
                //var userSyncTask = RunUserSyncJobAsync(stoppingToken);
                //var snapshotTask = RunUserSnapshotJobAsync(stoppingToken);
                //var reportTask = RunReportJobAsync(stoppingToken);

                //await Task.WhenAll(userSyncTask, snapshotTask, reportTask);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Sample A - BackgroundService: stopped.");
        }
    }

    private async Task RunBusinessAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var businessService = scope.ServiceProvider.GetRequiredService<UserJobBusinessService>();

            await businessService.RunBackgroundWorkerSampleAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sample A - BackgroundService: business execution failed.");
        }
    }
}
