using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Infrastructure.Persistence.Connections;
using System.Data;

namespace ManagerUser.Infrastructure.Persistence.Sessions;
public sealed class DbSession : IDisposable
{
    private readonly SqlConnectionFactory _connectionFactory;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;

    public DbSession(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IDbConnection Connection
    {
        get
        {
            if (_connection is not null)
                return _connection;

            _connection = _connectionFactory.CreateConnection();

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            return _connection;
        }
    }

    public IDbTransaction Transaction
    {
        get
        {
            if (_transaction is not null)
                return _transaction;

            _transaction = Connection.BeginTransaction();

            return _transaction;
        }
    }

    public void Commit()
    {
        _transaction?.Commit();
        DisposeTransaction();
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        DisposeTransaction();
    }

    public void Dispose()
    {
        DisposeTransaction();

        _connection?.Dispose();
        _connection = null;
    }

    private void DisposeTransaction()
    {
        _transaction?.Dispose();
        _transaction = null;
    }
}
