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
}
