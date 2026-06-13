using ManagerUser.Application.Users.Queries.GetUserById;
using ManagerUser.Infrastructure.Persistence.Records;

namespace ManagerUser.Infrastructure.Persistence.Mappings;

internal static class UserDetailRecordMapping
{
    public static GetUserByIdQueryResult ToQueryResult(this UserDetailRecord record)
    {
        return new GetUserByIdQueryResult(
            Id: record.Id,
            Username: record.Username,
            Email: record.Email,
            FullName: record.FullName,
            Status: record.Status,
            CreatedAt: record.CreatedAt,
            ModifiedAt: record.ModifiedAt);
    }
}
