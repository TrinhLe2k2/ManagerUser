using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Infrastructure.Persistence.Sessions;

namespace ManagerUser.Infrastructure.Persistence.UnitOfWork;
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DbSession _dbSession;
    private bool _completed;

    public UnitOfWork(DbSession dbSession)
    {
        _dbSession = dbSession;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_completed)
            return Task.CompletedTask;

        _dbSession.Commit();
        _completed = true;

        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_completed)
            return Task.CompletedTask;

        _dbSession.Rollback();
        _completed = true;

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        if (!_completed)
            _dbSession.Rollback();

        return ValueTask.CompletedTask;
    }
}
