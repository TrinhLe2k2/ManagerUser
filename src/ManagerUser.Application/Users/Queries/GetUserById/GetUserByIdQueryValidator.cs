using FluentValidation;

namespace ManagerUser.Application.Users.Queries.GetUserById;
public sealed class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
