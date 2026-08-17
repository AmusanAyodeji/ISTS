using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Ticketing.Application.DTOs.Users;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Constants;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResponseDto>
{
    private readonly IUserCreationService _userCreationService;
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserCreationService userCreationService, IUserRepository userRepository)
    {
        _userCreationService = userCreationService;
        _userRepository = userRepository;
    }

    public async Task<CreateUserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var response = await _userCreationService.CreateUser(request.Request, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        return response;
    }
}
