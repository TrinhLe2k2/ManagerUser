using FluentValidation;
using ManagerUser.Application.Common.Abstractions.Persistence;

namespace ManagerUser.Application.Users.Queries.GetUsers;
public sealed class GetUsersQueryHandler
{
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly IValidator<GetUsersQuery> _validator;

    public GetUsersQueryHandler(
        IUserQueryRepository userQueryRepository,
        IValidator<GetUsersQuery> validator)
    {
        _userQueryRepository = userQueryRepository;
        _validator = validator;
    }

    public async Task<GetUsersQueryResult> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        return await _userQueryRepository.GetListAsync(query, cancellationToken);
    }
}
