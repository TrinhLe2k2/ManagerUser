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
}
