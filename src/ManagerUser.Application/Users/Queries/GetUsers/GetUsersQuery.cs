namespace ManagerUser.Application.Users.Queries.GetUsers;
public sealed record GetUsersQuery(
    string? Keyword,
    byte? Status,
    int PageIndex,
    int PageSize);
