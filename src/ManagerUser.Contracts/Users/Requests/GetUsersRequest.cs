namespace ManagerUser.Contracts.Users.Requests;

public sealed class GetUsersRequest
{
    public string? Keyword { get; set; }

    public byte? Status { get; set; }

    public int PageIndex { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
