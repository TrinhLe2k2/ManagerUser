using ManagerUser.Application.Users.Commands.ChangeUserStatus;
using ManagerUser.Application.Users.Commands.CreateUser;
using ManagerUser.Application.Users.Commands.DeleteUser;
using ManagerUser.Application.Users.Commands.UpdateUser;
using ManagerUser.Application.Users.Queries.GetUserById;
using ManagerUser.Application.Users.Queries.GetUsers;
using ManagerUser.Contracts.Users.Requests;

namespace ManagerUser.Api.Mappings;

public static class UserRequestMapping
{
    public static GetUsersQuery ToQuery(this GetUsersRequest request)
    {
        return new GetUsersQuery(
            Keyword: string.IsNullOrWhiteSpace(request.Keyword) ? null : request.Keyword.Trim(),
            Status: request.Status,
            PageIndex: request.PageIndex,
            PageSize: request.PageSize);
    }

    public static GetUserByIdQuery ToGetByIdQuery(this Guid id)
    {
        return new GetUserByIdQuery(Id: id);
    }

    public static CreateUserCommand ToCommand(this CreateUserRequest request)
    {
        return new CreateUserCommand(
            Username: request.Username.Trim(),
            Email: request.Email.Trim(),
            FullName: string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim(),
            Password: request.Password,
            Status: request.Status);
    }

    public static UpdateUserCommand ToCommand(this UpdateUserRequest request, Guid id)
    {
        return new UpdateUserCommand(
            Id: id,
            Email: request.Email.Trim(),
            FullName: string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim());
    }

    public static DeleteUserCommand ToDeleteCommand(this Guid id)
    {
        return new DeleteUserCommand(Id: id);
    }

    public static ChangeUserStatusCommand ToCommand(this ChangeUserStatusRequest request, Guid id)
    {
        return new ChangeUserStatusCommand(
            Id: id,
            Status: request.Status);
    }
}
