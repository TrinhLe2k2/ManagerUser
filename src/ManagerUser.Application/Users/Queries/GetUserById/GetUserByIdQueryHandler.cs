using FluentValidation;
using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Common.Constants;
using ManagerUser.Application.Common.Exceptions;

namespace ManagerUser.Application.Users.Queries.GetUserById;
public sealed class GetUserByIdQueryHandler
{
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly IValidator<GetUserByIdQuery> _getUserByIdQueryValidator;

    public GetUserByIdQueryHandler(IUserQueryRepository userQueryRepository, IValidator<GetUserByIdQuery> validator)
    {
        _userQueryRepository = userQueryRepository;
        _getUserByIdQueryValidator = validator;
    }

    public async Task<GetUserByIdQueryResult> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _getUserByIdQueryValidator.ValidateAndThrowAsync(query);

        var result = await _userQueryRepository.GetByIdAsync(query, cancellationToken);

        if (result is null)
            throw new AppException(ApplicationCodes.User.NotFound, ApplicationMessages.User.NotFound, System.Net.HttpStatusCode.NotFound);

        return result;
    }
}
