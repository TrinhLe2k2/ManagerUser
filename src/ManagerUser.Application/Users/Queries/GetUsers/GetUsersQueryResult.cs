namespace ManagerUser.Application.Users.Queries.GetUsers;
public sealed record GetUsersQueryResult(
    IReadOnlyList<GetUsersQueryItem> Items,
    int PageIndex,
    int PageSize,
    int TotalCount);
