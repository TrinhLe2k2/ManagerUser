using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Common.Abstractions.Security;
using ManagerUser.Infrastructure.Persistence.Connections;
using ManagerUser.Infrastructure.Persistence.Repositories;
using ManagerUser.Infrastructure.Persistence.Sessions;
using ManagerUser.Infrastructure.Persistence.UnitOfWork;
using ManagerUser.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace ManagerUser.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<SqlConnectionFactory>();

        services.AddScoped<DbSession>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserQueryRepository, UserQueryRepository>();
        services.AddScoped<IUserCommandRepository, UserCommandRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
