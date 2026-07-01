using MediatR;
using Shared.Domain.DTOs;
using UserManagement.Application.ViewModels;

namespace UserManagement.Application.Commands.UserCommands.CreateUser;

public record CreateUserCommand
(
    string FirstName,
    string LastName,
    string Email,
    string Password
) : IRequest<Response<UserViewModel>>;

