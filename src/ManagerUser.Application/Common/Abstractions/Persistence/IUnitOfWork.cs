namespace ManagerUser.Application.Common.Abstractions.Persistence;
public interface IUnitOfWork : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}
