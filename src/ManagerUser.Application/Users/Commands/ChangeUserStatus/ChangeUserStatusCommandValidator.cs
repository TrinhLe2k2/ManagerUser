using FluentValidation;

namespace ManagerUser.Application.Users.Commands.ChangeUserStatus;

public sealed class ChangeUserStatusCommandValidator : AbstractValidator<ChangeUserStatusCommand>
{
    public ChangeUserStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Status)
            .Must(status => status is 0 or 1 or 2)
            .WithMessage("Trạng thái user không hợp lệ.");
    }
}
