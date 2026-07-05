using ManagerUser.Worker.BackgroundTaskQueues;
using ManagerUser.Worker.Options;
using ManagerUser.Worker.Services;
using Microsoft.Extensions.Options;

namespace ManagerUser.Worker.BackgroundServices;

// Producer là nơi phát sinh việc.
//
// Trong sample này producer chạy theo interval:
// - Khi Worker start: enqueue một batch đầu tiên.
// - Mỗi tick tiếp theo: enqueue thêm một batch job.
//
// Producer không trực tiếp gọi repository/database.
// Nó chỉ đóng gói việc cần làm thành BackgroundTaskQueueItem rồi đưa vào queue.
// Nhờ vậy producer không bị phụ thuộc vào thời gian xử lý thật của từng job.
public sealed class BackgroundTaskQueueProducerWorker : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<BackgroundTaskQueueProducerWorker> _logger;
    private readonly IOptions<WorkerBusinessOptions> _options;

    public BackgroundTaskQueueProducerWorker(
        IBackgroundTaskQueue taskQueue,
        ILogger<BackgroundTaskQueueProducerWorker> logger,
        IOptions<WorkerBusinessOptions> options)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_options.Value.BackgroundIntervalSeconds);

        _logger.LogInformation(
            "Sample D - BackgroundTaskQueue: producer started with interval {Interval}. QueueCapacity={QueueCapacity}.",
            interval,
            _options.Value.SampleDQueueCapacity);

        // Queue batch đầu tiên ngay khi Worker start để dễ quan sát log.
        await QueueJobBatchAsync("startup", stoppingToken);

        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                // Mỗi tick chỉ phát sinh thêm việc.
                //
                // Nếu job trước còn đang chạy, producer vẫn có thể enqueue job mới
                // cho đến khi queue đầy. Khi queue đầy, QueueAsync sẽ await để tạo backpressure.
                //
                // Việc chạy nhanh hay chậm là trách nhiệm của consumer.
                await QueueJobBatchAsync("timer", stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Sample D - BackgroundTaskQueue: producer stopped.");
        }
    }

    private async Task QueueJobBatchAsync(string trigger, CancellationToken stoppingToken)
    {
        var batchId = Guid.NewGuid().ToString("N")[..8];
        // Một batch có nhiều job độc lập.
        //
        // Điểm cần chú ý:
        // ở đây ta chưa chạy job, chưa query DB, chưa tạo scope.
        // Ta chỉ tạo danh sách "phiếu việc" để consumer xử lý sau.
        var jobs = new[]
        {
            CreateUserJob(
                batchId,
                "AllUsersSnapshotJob",
                (service, token) => service.RunQueuedAllUsersSnapshotJobAsync(token)),

            CreateUserJob(
                batchId,
                "StatusOneUsersJob",
                (service, token) => service.RunQueuedStatusOneUsersJobAsync(token)),

            CreateUserJob(
                batchId,
                "SlowUsersSnapshotJob",
               (service, token) => service.RunQueuedSlowUsersSnapshotJobAsync(token)) // Truyền vào lambda:
        };

        foreach (var job in jobs)
        {
            // Nếu queue đầy, QueueAsync sẽ chờ nhờ FullMode.Wait ở BackgroundTaskQueue.
            // Đây chính là điểm khác với gọi business logic trực tiếp:
            // producer có thể bị chậm lại bởi sức chứa queue, không làm hệ thống phình memory.
            await _taskQueue.QueueAsync(job, stoppingToken);

            _logger.LogInformation(
                "Sample D - BackgroundTaskQueue: queued job {JobId} ({JobName}) from {Trigger}. QueueCount={QueueCount}.",
                job.Id,
                job.Name,
                trigger,
                _taskQueue.Count);
        }
    }

    private static BackgroundTaskQueueItem CreateUserJob(string batchId, string jobName, Func<UserJobBusinessService, CancellationToken, Task> runJobAsync)
    {
        // Ở đây chưa lấy UserJobBusinessService vì service này là scoped.
        //
        // Nếu resolve scoped service ở producer rồi giữ nó trong queue, service đó có thể
        // bị dùng ngoài vòng đời scope hoặc bị dùng chung sai cách giữa nhiều job.
        //
        // Vì vậy queue item chỉ giữ delegate. Consumer tạo scope mới cho từng job,
        // rồi delegate này mới resolve UserJobBusinessService từ scope đó.
        return new BackgroundTaskQueueItem(
            $"{batchId}:{jobName}",
            async (serviceProvider, cancellationToken) =>
            {
                var businessService = serviceProvider.GetRequiredService<UserJobBusinessService>();
                await runJobAsync(businessService, cancellationToken);
            });
    }
}
