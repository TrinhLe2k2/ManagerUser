namespace ManagerUser.Worker.BackgroundTaskQueues;

// Đây là "phiếu việc" được bỏ vào queue.
//
// Thay vì producer gọi business logic ngay tại chỗ, producer tạo một object mô tả:
// - Job tên gì.
// - Job được enqueue lúc nào.
// - Khi consumer lấy job ra thì phải chạy delegate nào.
//
// Cách này giúp tách 2 việc:
// - Nơi phát sinh việc: producer.
// - Nơi thực sự xử lý việc: consumer.
public sealed class BackgroundTaskQueueItem
{
    public string Id { get; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; }
    // Dùng để log xem job đã nằm chờ trong queue bao lâu trước khi được consumer xử lý.
    // Nếu thời gian chờ tăng dần, nghĩa là producer đang tạo job nhanh hơn consumer xử lý.
    public DateTimeOffset EnqueuedAt { get; } = DateTimeOffset.UtcNow;
    public Func<IServiceProvider, CancellationToken, Task> ExecuteAsync { get; }

    public BackgroundTaskQueueItem(string name, Func<IServiceProvider, CancellationToken, Task> executeAsync)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Queue item name is required.", nameof(name));
        }

        Name = name;
        ExecuteAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
    }
}
