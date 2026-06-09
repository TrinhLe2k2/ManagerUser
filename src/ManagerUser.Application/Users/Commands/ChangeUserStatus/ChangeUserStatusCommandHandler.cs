using FluentValidation;
using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Common.Constants;
using ManagerUser.Application.Common.Exceptions;
using System.Net;

namespace ManagerUser.Application.Users.Commands.ChangeUserStatus;

public sealed class ChangeUserStatusCommandHandler
{
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IValidator<ChangeUserStatusCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeUserStatusCommandHandler(
        IUserQueryRepository userQueryRepository,
        IUserCommandRepository userCommandRepository,
        IValidator<ChangeUserStatusCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _userQueryRepository = userQueryRepository;
        _userCommandRepository = userCommandRepository;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<ChangeUserStatusCommandResult> HandleAsync(
        ChangeUserStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        if (!await _userQueryRepository.ExistsByIdAsync(command.Id, cancellationToken))
            throw new AppException(ApplicationCodes.User.NotFound, ApplicationMessages.User.NotFound, HttpStatusCode.NotFound);

        await _userCommandRepository.ChangeStatusAsync(command, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new ChangeUserStatusCommandResult(command.Id);
    }
}
