using Elastic.Clients.Elasticsearch.QueryDsl;
using ManagerUser.Worker.Options;
using ManagerUser.Worker.Services;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ManagerUser.Worker.BackgroundServices;

public sealed class MultipleUserJobsWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MultipleUserJobsWorker> _logger;
    private readonly IOptions<WorkerBusinessOptions> _options;

    public MultipleUserJobsWorker(IServiceScopeFactory serviceScopeFactory, ILogger<MultipleUserJobsWorker> logger, IOptions<WorkerBusinessOptions> options)
    {
        _scopeFactory = serviceScopeFactory;
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_options.Value.BackgroundIntervalSeconds);
        
        await RunJobCycleAsync(stoppingToken);

        using var timer = new PeriodicTimer(interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunJobCycleAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Sample B - MultipleJobs: stopped.");
        }
    }

    private async Task RunJobCycleAsync(CancellationToken stoppingToken)
    {
        var cycleId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = Stopwatch.StartNew();
        var interval = TimeSpan.FromSeconds(_options.Value.BackgroundIntervalSeconds);

        _logger.LogError(
            "Sample B - MultipleJobs: cycle {CycleId} started. started with interval {Interval}. RunInParallel={RunInParallel}.",
            cycleId,
            interval,
            _options.Value.RunSampleBJobsInParallel);

        if (_options.Value.RunSampleBJobsInParallel)
        {
            _options.Value.RunSampleBJobsInParallel = false;
            await RunJobsInParallelAsync(cycleId, stoppingToken);
        }
        else
        {
            _options.Value.RunSampleBJobsInParallel = true;
            await RunJobsSequentiallyAsync(cycleId, stoppingToken);
        }

        stopwatch.Stop();

        _logger.LogInformation(
            "Sample B - MultipleJobs: cycle {CycleId} finished in {ElapsedMilliseconds} ms.",
            cycleId,
            stopwatch.ElapsedMilliseconds);
    }

    private async Task RunJobsSequentiallyAsync(string cycleId, CancellationToken stoppingToken)
    {
        await RunScopedJobAsync(
            cycleId,
            "AllUsersSnapshotJob",
            (service, token) => service.RunAllUsersSnapshotJobAsync(token),
            stoppingToken);

        await RunScopedJobAsync(
            cycleId,
            "StatusOneUsersJob",
            (service, token) => service.RunStatusOneUsersJobAsync(token),
            stoppingToken);

        await RunScopedJobAsync(
            cycleId,
            "StatusTwoUsersJob",
            (service, token) => service.RunStatusTwoUsersJobAsync(token),
            stoppingToken);
    }

    private Task RunJobsInParallelAsync(string cycleId, CancellationToken stoppingToken)
    {
        var jobs = new[]
        {
            RunScopedJobAsync(
                cycleId,
                "AllUsersSnapshotJob",
                (service, token) => service.RunAllUsersSnapshotJobAsync(token),
                stoppingToken),

            RunScopedJobAsync(
                cycleId,
                "StatusOneUsersJob",
                (service, token) => service.RunStatusOneUsersJobAsync(token),
                stoppingToken),

            RunScopedJobAsync(
                cycleId,
                "StatusTwoUsersJob",
                (service, token) => service.RunStatusTwoUsersJobAsync(token),
                stoppingToken)
        };

        return Task.WhenAll(jobs);
    }

    private async Task RunScopedJobAsync(string cycleId, string jobName, Func<UserJobBusinessService, CancellationToken, Task> runJobAsync, CancellationToken stoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation(
                "Sample B - MultipleJobs: cycle {CycleId}, job {JobName} started.",
                cycleId,
                jobName);

            using var scope = _scopeFactory.CreateScope();
            var businessService = scope.ServiceProvider.GetRequiredService<UserJobBusinessService>();
            await runJobAsync(businessService, stoppingToken);

            stopwatch.Stop();
            _logger.LogInformation(
                "Sample B - MultipleJobs: cycle {CycleId}, job {JobName} finished in {ElapsedMilliseconds} ms.",
                cycleId,
                jobName,
                stopwatch.ElapsedMilliseconds);

        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Sample B - MultipleJobs: cycle {CycleId}, job {JobName} failed after {ElapsedMilliseconds} ms.",
                cycleId,
                jobName,
                stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation($"Job completed in {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
