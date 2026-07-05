namespace ManagerUser.Worker.BackgroundTaskQueues;

// Interface giúp producer và consumer chỉ biết "có một hàng đợi job",
// không cần biết hàng đợi đó được lưu bằng kỹ thuật nào.
//
// Hiện tại implementation dùng in-memory Channel<T>.
// Sau này nếu cần durable queue, có thể đổi implementation sang DB/RabbitMQ/Hangfire
// mà producer/consumer gần như không phải đổi cách gọi QueueAsync/DequeueAsync.
public interface IBackgroundTaskQueue
{
    int Count { get; }

    ValueTask QueueAsync(BackgroundTaskQueueItem workItem, CancellationToken cancellationToken = default);

    ValueTask<BackgroundTaskQueueItem> DequeueAsync(CancellationToken cancellationToken);
}
