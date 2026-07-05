using ManagerUser.Worker.Options;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace ManagerUser.Worker.BackgroundTaskQueues;

// Queue nội bộ chạy trong memory của Worker process.
//
// Channel<T> là một cấu trúc producer/consumer async có sẵn trong .NET:
// - Producer ghi dữ liệu vào Channel.Writer.
// - Consumer đọc dữ liệu từ Channel.Reader.
// - Nếu chưa có dữ liệu, Reader.ReadAsync sẽ await thay vì tự sleep/poll.
// - Nếu queue bị giới hạn dung lượng và đã đầy, Writer.WriteAsync có thể await.
//
// Có thể hình dung Channel giống một "hộp thư" thread-safe:
// producer bỏ thư vào, consumer lấy thư ra. Hai bên chạy độc lập nhưng vẫn phối hợp an toàn.
//
// Lưu ý: đây là in-memory queue. Job chỉ nằm trong RAM của process hiện tại.
// Phù hợp cho job nhẹ, chấp nhận mất nếu app restart.
// Nếu cần đảm bảo không mất job, nên chuyển sang DB queue, Outbox, Hangfire, RabbitMQ hoặc Kafka.
public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<BackgroundTaskQueueItem> _queue;
    // Channel<T> không được dùng ở đây như một collection thường để hỏi Count trực tiếp.
    // Vì vậy ta tự giữ _count phục vụ log/demo, để thấy queue đang tồn bao nhiêu việc.
    private int _count;

    public BackgroundTaskQueue(IOptions<WorkerBusinessOptions> options)
    {
        // BoundedChannel là Channel có giới hạn dung lượng.
        //
        // Vì sao không dùng unbounded queue?
        // Nếu producer tạo job nhanh hơn consumer xử lý trong thời gian dài,
        // unbounded queue có thể phình to và ăn hết memory.
        //
        // FullMode.Wait tạo backpressure:
        // - Queue còn chỗ: WriteAsync ghi job vào ngay.
        // - Queue đã đầy: WriteAsync await đến khi consumer lấy bớt job ra.
        //
        // Đây là cách hệ thống tự "hãm tốc" producer thay vì drop job hoặc tăng memory vô hạn.
        var queueOptions = new BoundedChannelOptions(options.Value.SampleDQueueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            // Sample D chỉ có một consumer BackgroundTaskQueueWorker đọc queue.
            // Khai báo SingleReader=true giúp Channel tối ưu nhẹ hơn.
            SingleReader = true,
            // Có thể có nhiều nơi enqueue job trong tương lai, nên để SingleWriter=false.
            SingleWriter = false
        };

        // Tạo Channel chứa BackgroundTaskQueueItem.
        // Từ đây ta sẽ dùng _queue.Writer để enqueue và _queue.Reader để dequeue.
        _queue = Channel.CreateBounded<BackgroundTaskQueueItem>(queueOptions);
    }

    public int Count => Volatile.Read(ref _count);

    // Producer gọi hàm này để đưa một job vào queue.
    //
    // ValueTask được dùng vì WriteAsync của Channel trả về ValueTask.
    // Với code gọi, có thể await giống Task bình thường.
    public async ValueTask QueueAsync(BackgroundTaskQueueItem workItem, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        // Tăng count trước khi await WriteAsync để tránh race nhỏ:
        // nếu consumer đang chờ sẵn, nó có thể nhận job gần như ngay lập tức.
        // Count ở đây nên được hiểu là số job đã enqueue hoặc đang chờ được ghi vào Channel.
        Interlocked.Increment(ref _count);

        try
        {
            // Nếu queue còn chỗ, dòng này hoàn tất nhanh.
            // Nếu queue đã đầy, dòng này sẽ await đến khi consumer lấy bớt job ra.
            // Trong lúc await, thread không bị block cứng; nó trả quyền xử lý cho runtime.
            await _queue.Writer.WriteAsync(workItem, cancellationToken);
        }
        catch
        {
            // Nếu WriteAsync bị cancel/lỗi trước khi job vào queue, phải trừ lại count.
            Interlocked.Decrement(ref _count);
            throw;
        }
    }

    // Consumer gọi hàm này để lấy job kế tiếp.
    //
    // Nếu queue đang rỗng, ReadAsync sẽ await đến khi producer enqueue job mới.
    // Nhờ vậy consumer không cần while + delay để kiểm tra queue liên tục.
    public async ValueTask<BackgroundTaskQueueItem> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        Interlocked.Decrement(ref _count);

        return workItem;
    }
}
