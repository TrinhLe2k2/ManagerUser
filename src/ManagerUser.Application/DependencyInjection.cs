using FluentValidation;
using ManagerUser.Application.Users.Commands.ChangeUserStatus;
using ManagerUser.Application.Users.Commands.CreateUser;
using ManagerUser.Application.Users.Commands.DeleteUser;
using ManagerUser.Application.Users.Commands.UpdateUser;
using ManagerUser.Application.Users.Queries.GetUserById;
using ManagerUser.Application.Users.Queries.GetUsers;
using Microsoft.Extensions.DependencyInjection;

namespace ManagerUser.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<GetUserByIdQueryHandler>();
        services.AddScoped<GetUsersQueryHandler>();
        services.AddScoped<CreateUserCommandHandler>();
        services.AddScoped<UpdateUserCommandHandler>();
        services.AddScoped<DeleteUserCommandHandler>();
        services.AddScoped<ChangeUserStatusCommandHandler>();

        services.AddScoped<IValidator<GetUserByIdQuery>, GetUserByIdQueryValidator>();
        services.AddScoped<IValidator<GetUsersQuery>, GetUsersQueryValidator>();
        services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
        services.AddScoped<IValidator<UpdateUserCommand>, UpdateUserCommandValidator>();
        services.AddScoped<IValidator<DeleteUserCommand>, DeleteUserCommandValidator>();
        services.AddScoped<IValidator<ChangeUserStatusCommand>, ChangeUserStatusCommandValidator>();

        return services;
    }
}
