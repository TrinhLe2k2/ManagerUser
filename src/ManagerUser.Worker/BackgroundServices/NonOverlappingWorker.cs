using ManagerUser.Worker.Options;
using ManagerUser.Worker.Services;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ManagerUser.Worker.BackgroundServices;

public sealed class NonOverlappingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NonOverlappingWorker> _logger;
    private readonly IOptions<WorkerBusinessOptions> _options;

    // Dùng để bảo vệ biến _runningCycleTask khi đọc/ghi từ nhiều vị trí.
    // Biến _runningCycleTask giúp worker biết cycle cuối cùng đang chạy là task nào,
    // để khi app shutdown thì có thể chờ cycle đó kết thúc/cancel gọn gàng.
    private readonly object _runningTaskLock = new();
    // _cycleGate là cái "cổng". Cycle nào vào được cổng thì được chạy.
    // Nếu cổng đang bị giữ bởi cycle trước, tick mới sẽ bị skip.
    private readonly SemaphoreSlim _cycleGate = new(1, 1);
    // Mặc định là CompletedTask để nếu app stop khi chưa có cycle nào chạy,
    // WaitForRunningCycleToFinishAsync vẫn await được mà không bị null.
    private Task _runningCycleTask = Task.CompletedTask;

    public NonOverlappingWorker(IServiceScopeFactory scopeFactory, ILogger<NonOverlappingWorker> logger, IOptions<WorkerBusinessOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_options.Value.BackgroundIntervalSeconds);
        var jobDuration = TimeSpan.FromSeconds(_options.Value.SampleCJobDurationSeconds);

        _logger.LogInformation(
            "Sample C - NonOverlapping: started with interval {Interval}. SimulatedJobDuration={JobDuration}.",
            interval,
            jobDuration);

        // Chạy cycle đầu tiên ngay khi worker start.
        //
        // Lưu ý: ở đây KHÔNG await cycle.
        // Nếu await, ExecuteAsync sẽ dừng lại cho đến khi job xong,
        // lúc đó timer chưa bắt đầu chờ tick tiếp theo, nên mình không thấy được case "skip".
        //
        // TryStartCycle sẽ khởi động cycle và lưu task đang chạy vào _runningCycleTask.
        TryStartCycle("startup", stoppingToken);

        // PeriodicTimer tạo tick theo chu kỳ interval.
        // Mỗi tick chỉ là "cơ hội" để bắt đầu cycle mới.
        // Cycle có thực sự được chạy hay không sẽ do TryStartCycle quyết định.
        using var timer = new PeriodicTimer(interval);

        try
        {
            // WaitForNextTickAsync sẽ chờ đến tick tiếp theo.
            // Khi app đang shutdown, stoppingToken bị cancel và dòng này ném OperationCanceledException.
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                // Không await ở đây để timer loop tiếp tục nhận các tick tiếp theo.
                // Nếu cycle cũ vẫn đang chạy, TryStartCycle sẽ log "skipped" và return ngay.
                TryStartCycle("timer", stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Sample C - NonOverlapping: stop requested.");
        }
        finally
        {
            // Khi host stop, không nên bỏ mặc cycle đang chạy.
            // Ta đợi task hiện tại kết thúc. Vì stoppingToken đã cancel,
            // job bên dưới sẽ thoát sớm nếu nó tôn trọng cancellationToken.
            await WaitForRunningCycleToFinishAsync();
            _logger.LogInformation("Sample C - NonOverlapping: stopped.");
        }
    }

    public override void Dispose()
    {
        _cycleGate.Dispose();
        base.Dispose();
    }

    private void TryStartCycle(string trigger, CancellationToken stoppingToken)
    {
        // Wait(0) là điểm cốt lõi của SampleC.
        //
        // - Nếu cổng đang rảnh: Wait(0) trả về true ngay lập tức, cycle mới được chạy.
        // - Nếu cổng đang bận: Wait(0) trả về false ngay lập tức, không cho chạy.
        //
        // Số 0 nghĩa là "không chờ".
        // 2 sẽ chờ trong 2s đến khi lấy được lock hoặc token bị cancel. quá 2s thì bỏ qua tick.
        // Vì SampleC muốn skip tick mới nếu cycle cũ chưa xong,
        // chứ không muốn xếp hàng cho tick mới chờ đến khi cycle cũ xong.
        if (!_cycleGate.Wait(TimeSpan.FromSeconds(0), stoppingToken))
        {
            _logger.LogWarning(
                "Sample C - NonOverlapping: skipped {Trigger} tick because previous cycle is still running.",
                trigger);

            return;
        }

        var cycleId = Guid.NewGuid().ToString("N")[..8];

        // RunCycleAsync trả về Task đại diện cho cycle đang chạy.
        // Ta không await ngay tại đây để timer loop vẫn có thể tiếp tục nhận tick mới.
        // Nếu tick mới đến trong lúc task này chưa xong, _cycleGate sẽ chặn nó.
        var runningTask = RunCycleAsync(cycleId, trigger, stoppingToken);

        // Lưu lại task đang chạy để lúc shutdown có thể đợi nó kết thúc/cancel.
        lock (_runningTaskLock)
        {
            _runningCycleTask = runningTask;
        }
    }

    private async Task RunCycleAsync(string cycleId, string trigger, CancellationToken stoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Sample C - NonOverlapping: cycle {CycleId} started by {Trigger}.",
                cycleId,
                trigger);

            // Tạo scope riêng cho mỗi cycle.
            // Nếu có 2 cycle được chạy ở 2 thời điểm khác nhau, mỗi cycle có DbSession/repository riêng.
            // Trong SampleC, thực tế chỉ 1 cycle được phép chạy tại một thời điểm,
            // nhưng vẫn nên tạo scope theo đúng pattern của BackgroundService.
            using var scope = _scopeFactory.CreateScope();
            var businessService = scope.ServiceProvider.GetRequiredService<UserJobBusinessService>();
            // Đây là job giả lập chạy lâu.
            // Nếu SampleCJobDurationSeconds lớn hơn BackgroundIntervalSeconds,
            // các tick đến trong lúc job này đang chạy sẽ bị skip.
            await businessService.RunSlowUsersSnapshotJobAsync(
                _options.Value.SampleCJobDurationSeconds,
                stoppingToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Sample C - NonOverlapping: cycle {CycleId} finished in {ElapsedMilliseconds} ms.",
                cycleId,
                stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Cancellation do app đang shutdown là trạng thái bình thường,
            // nên log Information thay vì Error.
            stopwatch.Stop();

            _logger.LogInformation(
                "Sample C - NonOverlapping: cycle {CycleId} canceled after {ElapsedMilliseconds} ms.",
                cycleId,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            // Lỗi business/DB/logic trong một cycle không nên làm chết cả worker.
            // Ta log lỗi, sau đó finally vẫn mở cổng để tick sau có cơ hội chạy tiếp.
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Sample C - NonOverlapping: cycle {CycleId} failed after {ElapsedMilliseconds} ms.",
                cycleId,
                stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            // Bắt buộc release trong finally.
            // Nếu job lỗi mà không release, _cycleGate sẽ bị khóa vĩnh viễn,
            // và tất cả tick sau đó đều bị skip.
            _cycleGate.Release();
        }
    }

    private async Task WaitForRunningCycleToFinishAsync()
    {
        Task runningTask;

        // Lấy snapshot của task đang chạy trong lock.
        // Sau khi có biến local runningTask, mình await ở ngoài lock để tránh giữ lock quá lâu.
        lock (_runningTaskLock)
        {
            runningTask = _runningCycleTask;
        }

        // Nếu không có cycle nào đang chạy, runningTask là Task.CompletedTask và dòng này kết thúc ngay.
        // Nếu có cycle đang chạy, worker sẽ đợi nó kết thúc/cancel trước khi stop hoàn toàn.
        await runningTask;
    }
}
