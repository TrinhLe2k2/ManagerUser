using FluentValidation;
using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Common.Constants;
using ManagerUser.Application.Common.Exceptions;
using System.Net;

namespace ManagerUser.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler
{
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IValidator<UpdateUserCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(
        IUserQueryRepository userQueryRepository,
        IUserCommandRepository userCommandRepository,
        IValidator<UpdateUserCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _userQueryRepository = userQueryRepository;
        _userCommandRepository = userCommandRepository;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateUserCommandResult> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        if (!await _userQueryRepository.ExistsByIdAsync(command.Id, cancellationToken))
            throw new AppException(ApplicationCodes.User.NotFound, ApplicationMessages.User.NotFound, HttpStatusCode.NotFound);

        if (await _userQueryRepository.ExistsByEmailAsync(command.Email, command.Id, cancellationToken))
            throw new AppException(ApplicationCodes.User.EmailAlreadyExists, ApplicationMessages.User.EmailAlreadyExists);

        await _userCommandRepository.UpdateAsync(command, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new UpdateUserCommandResult(command.Id);
    }
}
