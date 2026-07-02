using MediatR;
using Shared.Domain.DTOs;
using UserManagement.Application.ViewModels;

namespace UserManagement.Application.Commands.UserCommands.CreateUser;

public record CreateUserCommand
(
    string UserName,
    string FirstName,
    string? MiddleName,
    string LastName,
    string Contact,
    string Email,
    string Password,
    List<Guid> Roles,
    List<Guid> ModulePermissions
) : IRequest<Response<UserViewModel>>;

