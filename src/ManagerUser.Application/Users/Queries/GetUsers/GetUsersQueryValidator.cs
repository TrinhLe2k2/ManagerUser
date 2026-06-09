using FluentValidation;

namespace ManagerUser.Application.Users.Queries.GetUsers;
public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Keyword)
            .MaximumLength(255);

        RuleFor(x => x.Status)
            .Must(status => status is null or 0 or 1 or 2)
            .WithMessage("Trạng thái user không hợp lệ.");
    }
}
