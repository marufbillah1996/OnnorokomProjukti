using AssignmentHub.Application.Users.Dtos;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Users.Mappings;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User u) =>
        new(u.Id, u.Name, u.Email, u.Role.ToString(), u.IsActive, u.CreatedAt);
}
