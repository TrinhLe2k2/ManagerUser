using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Common.Models;
using ManagerUser.Worker.Options;
using ManagerUser.Worker.Services;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ManagerUser.Worker.BackgroundServices;

// Sample E minh họa Outbox Dispatcher.
//
// Tư duy:
// 1. Business transaction ghi message cần publish vào bảng OutboxMessages.
// 2. Worker này poll DB theo chu kỳ.
// 3. Worker claim một batch pending message để tránh worker khác xử lý trùng cùng lúc.
// 4. Worker publish/xử lý message.
// 5. Thành công thì mark Processed, lỗi thì mark Failed hoặc hẹn retry.
//
// Đây là at-least-once pattern: message có thể được xử lý lại nếu app crash sau khi publish
// nhưng trước khi mark Processed, nên handler/publisher thực tế nên idempotent.
public sealed class OutboxDispatcherWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcherWorker> _logger;
    private readonly IOptions<WorkerBusinessOptions> _options;

    public OutboxDispatcherWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxDispatcherWorker> logger,
        IOptions<WorkerBusinessOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_options.Value.SampleEPollIntervalSeconds);

        _logger.LogInformation(
            "Sample E - Outbox: dispatcher started. PollInterval={PollInterval}, BatchSize={BatchSize}.",
            interval,
            _options.Value.SampleEBatchSize);

        // Chạy một lượt ngay khi Worker start để không phải chờ tick đầu tiên.
        await DispatchPendingBatchAsync(stoppingToken);

        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DispatchPendingBatchAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Sample E - Outbox: dispatcher stopped.");
        }
    }

    private async Task DispatchPendingBatchAsync(CancellationToken stoppingToken)
    {
        // Mỗi lần poll tạo scope riêng.
        // Repository/DbSession là scoped, nên không giữ chúng làm field trong BackgroundService singleton.
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var processor = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessor>();

        // ClaimPendingAsync không chỉ SELECT.
        // Nó update message sang Processing trong DB rồi trả về các row vừa claim.
        var messages = await repository.ClaimPendingAsync(
            _options.Value.SampleEBatchSize,
            _options.Value.SampleELockTimeoutSeconds,
            stoppingToken);

        if (messages.Count == 0)
        {
            _logger.LogInformation("Sample E - Outbox: no pending messages.");
            return;
        }

        _logger.LogInformation(
            "Sample E - Outbox: claimed {MessageCount} pending messages.",
            messages.Count);

        foreach (var message in messages)
        {
            // Sample này xử lý tuần tự để log dễ đọc.
            // Nếu cần throughput cao hơn, có thể xử lý song song có giới hạn concurrency.
            await DispatchMessageAsync(repository, processor, message, stoppingToken);
        }
    }

    private async Task DispatchMessageAsync(IOutboxMessageRepository repository, OutboxMessageProcessor processor, OutboxMessage message, CancellationToken stoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await processor.ProcessAsync(message, stoppingToken);
            // Chỉ mark processed sau khi xử lý/publish thành công.
            await repository.MarkProcessedAsync(message.Id, stoppingToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Sample E - Outbox: message {MessageId} processed in {ElapsedMilliseconds} ms.",
                message.Id,
                stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Lỗi của một message không làm chết dispatcher.
            // Repository sẽ tăng RetryCount và hẹn retry nếu chưa quá MaxRetryCount.
            await repository.MarkFailedAsync(
                message.Id,
                ex.Message,
                _options.Value.SampleERetryDelaySeconds,
                stoppingToken);

            _logger.LogError(
                ex,
                "Sample E - Outbox: message {MessageId} failed after {ElapsedMilliseconds} ms. It will be retried if retry limit is not reached.",
                message.Id,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
