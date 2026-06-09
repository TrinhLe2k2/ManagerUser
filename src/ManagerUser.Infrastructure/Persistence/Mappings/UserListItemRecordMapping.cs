using ManagerUser.Application.Users.Queries.GetUsers;
using ManagerUser.Infrastructure.Persistence.Records;

namespace ManagerUser.Infrastructure.Persistence.Mappings;

internal static class UserListItemRecordMapping
{
    public static GetUsersQueryItem ToQueryItem(this UserListItemRecord record)
    {
        return new GetUsersQueryItem(
            Id: record.Id,
            Username: record.Username,
            Email: record.Email,
            FullName: record.FullName,
            Status: record.Status,
            CreatedAt: record.CreatedAt,
            ModifiedAt: record.ModifiedAt);
    }
}
