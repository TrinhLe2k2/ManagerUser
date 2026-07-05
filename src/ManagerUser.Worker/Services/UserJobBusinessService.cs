using ManagerUser.Application.Users.Queries.GetUsers;
using ManagerUser.Worker.Options;
using Microsoft.Extensions.Options;

namespace ManagerUser.Worker.Services;

public class UserJobBusinessService
{
    private readonly GetUsersQueryHandler _getUsersQueryHandler;
    private readonly IOptions<WorkerBusinessOptions> _options;
    private readonly ILogger<UserJobBusinessService> _logger;

    public UserJobBusinessService(GetUsersQueryHandler getUsersQueryHandler, IOptions<WorkerBusinessOptions> options, ILogger<UserJobBusinessService> logger)
    {
        _getUsersQueryHandler = getUsersQueryHandler;
        _options = options;
        _logger = logger;
    }

    public async Task RunBackgroundWorkerSampleAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetUsersAsync(status: null, cancellationToken);

        _logger.LogInformation(
            "Sample A - BackgroundService: loaded {LoadedCount}/{TotalCount} users for operational snapshot.",
            result.Items.Count,
            result.TotalCount);
    }

    private Task<GetUsersQueryResult> GetUsersAsync(byte? status, CancellationToken cancellationToken)
    {
        var query = new GetUsersQuery(
            Keyword: null,
            Status: status,
            PageIndex: 1,
            PageSize: _options.Value.PageSize);

        return _getUsersQueryHandler.HandleAsync(query, cancellationToken);
    }

    public Task RunAllUsersSnapshotJobAsync(CancellationToken cancellationToken = default)
    {
        return RunUsersJobAsync("Sample B - AllUsersSnapshotJob", status: null, cancellationToken);
    }

    public Task RunStatusOneUsersJobAsync(CancellationToken cancellationToken = default)
    {
        return RunUsersJobAsync("Sample B - StatusOneUsersJob", status: 1, cancellationToken);
    }

    public Task RunStatusTwoUsersJobAsync(CancellationToken cancellationToken = default)
    {
        return RunUsersJobAsync("Sample B - StatusTwoUsersJob", status: 2, cancellationToken);
    }

    private async Task RunUsersJobAsync(string jobName, byte? status, CancellationToken cancellationToken)
    {
        var result = await GetUsersAsync(status, cancellationToken);

        _logger.LogInformation(
            "{JobName}: loaded {LoadedCount}/{TotalCount} users. Status={Status}.",
            jobName,
            result.Items.Count,
            result.TotalCount,
            status?.ToString() ?? "All");
    }

    public async Task RunSlowUsersSnapshotJobAsync(int durationSeconds, CancellationToken cancellationToken = default)
    {
        var duration = TimeSpan.FromSeconds(durationSeconds);

        _logger.LogInformation(
            "Sample C - SlowUsersSnapshotJob: simulating long work for {Duration}.",
            duration);

        // Đây là phần giả lập job lâu hơn interval.
        // Ví dụ Development config đang để:
        // - BackgroundIntervalSeconds = 5
        // - SampleCJobDurationSeconds = 12
        //
        // Nghĩa là timer tick mỗi 5 giây, nhưng job mất 12 giây.
        // Nhờ vậy khi chạy SampleC bạn sẽ thấy log tick bị skip.
        await Task.Delay(duration, cancellationToken);

        // Sau khi giả lập việc lâu, job vẫn gọi logic thật của project:
        // lấy danh sách user qua GetUsersQueryHandler và repository.
        await RunUsersJobAsync("Sample C - SlowUsersSnapshotJob", status: null, cancellationToken);
    }
}
