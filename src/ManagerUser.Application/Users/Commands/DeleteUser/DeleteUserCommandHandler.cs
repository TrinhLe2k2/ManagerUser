using FluentValidation;
using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Common.Constants;
using ManagerUser.Application.Common.Exceptions;
using System.Net;

namespace ManagerUser.Application.Users.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler
{
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IValidator<DeleteUserCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(
        IUserQueryRepository userQueryRepository,
        IUserCommandRepository userCommandRepository,
        IValidator<DeleteUserCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _userQueryRepository = userQueryRepository;
        _userCommandRepository = userCommandRepository;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteUserCommandResult> HandleAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        if (!await _userQueryRepository.ExistsByIdAsync(command.Id, cancellationToken))
            throw new AppException(ApplicationCodes.User.NotFound, ApplicationMessages.User.NotFound, HttpStatusCode.NotFound);

        await _userCommandRepository.DeleteAsync(command, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new DeleteUserCommandResult(command.Id);
    }
}
