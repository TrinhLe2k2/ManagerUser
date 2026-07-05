using ManagerUser.Worker.BackgroundTaskQueues;
using System.Diagnostics;

namespace ManagerUser.Worker.BackgroundServices;

// Consumer là worker chuyên xử lý job trong queue.
//
// Nó chạy một vòng lặp dài suốt vòng đời Worker:
// - Chờ queue có job.
// - Lấy một job ra.
// - Tạo scope DI riêng cho job đó.
// - Chạy delegate ExecuteAsync của job.
// - Log lỗi nếu job fail, sau đó tiếp tục xử lý job kế tiếp.
//
// Producer có thể enqueue nhanh, consumer xử lý theo nhịp riêng để kiểm soát tải.
public sealed class BackgroundTaskQueueWorker : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundTaskQueueWorker> _logger;

    public BackgroundTaskQueueWorker(
        IBackgroundTaskQueue taskQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<BackgroundTaskQueueWorker> logger)
    {
        _taskQueue = taskQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sample D - BackgroundTaskQueue: consumer started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Nếu queue rỗng, DequeueAsync sẽ await.
                //
                // Đây là lợi thế lớn của Channel:
                // consumer không cần tự sleep vài giây rồi kiểm tra lại,
                // nên không tốn CPU cho polling và cũng không bị delay giả tạo khi job mới đến.
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);
                await ExecuteWorkItemAsync(workItem, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Sample D - BackgroundTaskQueue: consumer stopped.");
        }
    }

    private async Task ExecuteWorkItemAsync(BackgroundTaskQueueItem workItem, CancellationToken stoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var queuedFor = DateTimeOffset.UtcNow - workItem.EnqueuedAt;

        try
        {
            _logger.LogInformation(
                "Sample D - BackgroundTaskQueue: job {JobId} ({JobName}) started after waiting {QueuedMilliseconds} ms. QueueCount={QueueCount}.",
                workItem.Id,
                workItem.Name,
                queuedFor.TotalMilliseconds,
                _taskQueue.Count);

            // Mỗi job có scope DI riêng để DbSession/repository/scoped service không bị dùng chung sai vòng đời.
            //
            // Pattern này đặc biệt quan trọng trong BackgroundService:
            // BackgroundService là singleton, nhưng repository/DbSession thường là scoped.
            // Vì vậy không inject trực tiếp scoped service vào worker singleton;
            // hãy tạo scope mỗi lần xử lý job rồi resolve service bên trong scope đó.
            using var scope = _scopeFactory.CreateScope();
            await workItem.ExecuteAsync(scope.ServiceProvider, stoppingToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Sample D - BackgroundTaskQueue: job {JobId} ({JobName}) finished in {ElapsedMilliseconds} ms.",
                workItem.Id,
                workItem.Name,
                stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "Sample D - BackgroundTaskQueue: job {JobId} ({JobName}) canceled after {ElapsedMilliseconds} ms.",
                workItem.Id,
                workItem.Name,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Lỗi của một job chỉ được log lại, không làm chết consumer.
            // Job kế tiếp trong queue vẫn có cơ hội được xử lý.
            _logger.LogError(
                ex,
                "Sample D - BackgroundTaskQueue: job {JobId} ({JobName}) failed after {ElapsedMilliseconds} ms.",
                workItem.Id,
                workItem.Name,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
